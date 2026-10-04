using System.Reflection;
using System.Runtime.CompilerServices;
using CombatSolver.Engine.Common;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace CombatSolver;

/// <summary>
/// Root-capture guard against third-party Harmony patches that replace gameplay behavior the engine mirrors.
/// </summary>
/// <remarks>
/// Compatibility is determined by the combat methods a mod changes. A replaced <see cref="CardModel.OnPlay"/>
/// requires a complete adapter: <c>CardOnPlayInferrer</c> reads the original, unpatched IL by design, and the
/// bespoke mirrors are keyed on the vanilla card type. The engine therefore keeps executing the vanilla recipe it
/// was written against and silently produces a route for a card the game no longer plays that way, which the
/// project's "unknown semantics must fail explicitly" constraint forbids.
/// </remarks>
internal static class PredictionModPatchAudit
{
    private static readonly HashSet<string> CombatModelMethodNames = new(StringComparer.Ordinal)
    {
        "get_CanonicalVars", "get_CanonicalEnergyCost", "get_CanonicalStarCost", "get_BaseStarCost", "get_CanonicalKeywords",
        "get_HasEnergyCostX", "get_HasStarCostX", "get_CurrentStarCost", "get_MinInitialHp", "get_MaxInitialHp",
        "get_TargetType", "get_IsPlayable", "get_StackType", "get_AllowNegative", "get_MaxAmount",
        "get_PassesCustomUsabilityCheck", "get_CanBeGeneratedInCombat",
        "OnUpgrade", "UpgradeInternal", "FinalizeUpgradeInternal", "BeforeApplied", "AfterApplied", "ApplyInternal",
        "BeforeRemoved", "AfterRemoved", "Use", "OnUse", "Evoke", "Passive",
        "BeforeTurnEndOrbTrigger", "AfterTurnStartOrbTrigger",
        "GenerateMoveStateMachine",
    };

    internal readonly record struct ForeignPatch(string ModId, string ModName, string Description);

    /// <summary>
    /// Throws when any card reachable from the captured root has a third-party patch on its mirrored OnPlay.
    /// </summary>
    /// <remarks>
    /// This is a best-effort boundary: card types that only appear later through in-combat generation are not
    /// visible at capture time and are not audited here.
    /// </remarks>
    public static void ValidateCardOnPlay(IEnumerable<CardModel> cards)
        => CaptureCardOnPlay(cards);

    internal static AdaptedOnPlaySnapshot? CaptureCardOnPlay(IEnumerable<CardModel> cards)
    {
        bool adapted = AdaptedCardOnPlayMirrors.Seal();
        Dictionary<Type, AdaptedCardOnPlayMirrors.Registration?>? selections = adapted ? [] : null;
        HashSet<Type> checkedTypes = [];
        foreach (CardModel card in cards)
        {
            // Harmony patches can be installed or removed between root captures.
            Type type = card.GetType();
            if (!checkedTypes.Add(type)) continue;
            AdaptedCardOnPlayMirrors.Registration? selected =
                AuditCardOnPlay(type, adapted, out ForeignPatch? firstForeign);
            if (selected is null && firstForeign is { } unsupported)
                throw new IncompatibleGameplayModException(unsupported.ModId, unsupported.ModName,
                    unsupported.Description, "combat");
            selections?.Add(type, selected);
        }
        if (selections is null)
            return null;

        Dictionary<Type, string> deferredFailures = [];
        foreach (Type type in AdaptedCardOnPlayMirrors.RegisteredTypes())
        {
            if (!checkedTypes.Add(type)) continue;
            try
            {
                AdaptedCardOnPlayMirrors.Registration? selected =
                    AuditCardOnPlay(type, adapted: true, out ForeignPatch? firstForeign);
                if (selected is null && firstForeign is { } unsupported)
                    deferredFailures.Add(type, $"{unsupported.ModName} ({unsupported.ModId}) patches the OnPlay of "
                        + $"{type.FullName} without a matching adapter: {unsupported.Description}.");
                else
                    selections.Add(type, selected);
            }
            catch (PredictionUnsupportedException error)
            {
                // The type is not reachable from this root. Keep its exact rejection for first use.
                deferredFailures.Add(type, error.Message);
            }
        }
        HashSet<MethodInfo> patchedOnPlayTargets = [];
        string stamp = AdaptedCardOnPlayMirrors.CaptureLiveStamp(patchedOnPlayTargets)!;
        return new(selections, stamp, patchedOnPlayTargets, deferredFailures);
    }

    /// <summary>
    /// Audits one card type exactly the way root capture does, leaving the caller to decide what a foreign
    /// patch means at that point.
    /// </summary>
    internal static AdaptedCardOnPlayMirrors.Registration? AuditCardOnPlay(
        Type type, bool adapted, out ForeignPatch? firstForeign)
    {
        MethodInfo target = AdaptedCardOnPlayMirrors.ResolveOnPlay(type)
            ?? throw new PredictionUnsupportedException($"Missing OnPlay for {type.FullName}.");
        Patches? patches = Harmony.GetPatchInfo(target);
        firstForeign = null;
        if (patches is not null)
            foreach (var group in AdaptedCardOnPlayMirrors.Groups(patches))
                foreach (Patch patch in group.Patches)
                {
                    // Resolve every source even when the full combination is registered.
                    ForeignPatch? foreign = TryDescribeForeignPatch(patch, target);
                    firstForeign ??= foreign;
                }
        if (target.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType is { } stateMachine)
        {
            MethodInfo moveNext = AccessTools.Method(stateMachine, "MoveNext")
                ?? throw new PredictionUnsupportedException($"Async OnPlay state machine has no MoveNext: {type.FullName}.");
            if (Harmony.GetPatchInfo(moveNext) is { } asyncPatches)
                foreach (var group in AdaptedCardOnPlayMirrors.Groups(asyncPatches))
                    foreach (Patch patch in group.Patches)
                    {
                        if (TryDescribeForeignPatch(patch, moveNext) is not { } foreign) continue;
                        throw new IncompatibleGameplayModException(foreign.ModId, foreign.ModName, foreign.Description, "combat");
                    }
        }
        return adapted ? AdaptedCardOnPlayMirrors.Select(type, target, patches) : null;
    }

    internal static void ValidateCombatModelPatches(IEnumerable<AbstractModel> models)
    {
        AbstractModel[] captured = models.ToArray();
        bool emptyCardCapabilities = captured.OfType<CardModel>().All(RitsuEmptyCapabilityFastPath.HasEmptyCapabilities);
        bool emptyCardModifiers = captured.OfType<CardModel>().All(card =>
            !PredictionModModelSupport.HasBaseLibCardModifiers(card));
        List<(Patch Patch, MethodBase Target)> variableUpgradeBridges = [];
        HashSet<Type> types = captured
            .Where(model => model is not RelicModel and not ModifierModel
                || !PredictionModHookSubscriberInertness.IsCombatInert(model.GetType(), out _))
            .Select(model => model.GetType()).ToHashSet();
        foreach (MethodBase target in Harmony.GetAllPatchedMethods())
        {
            if (target.DeclaringType is not { } declaringType) continue;
            if (types.Any(declaringType.IsAssignableFrom) && IsCombatModelMethod(target))
                RejectForeignPatches([target], AllowFrameworkBridge);
            // Harmony can patch an async body independently of its entry method.
            if (target.Name != "MoveNext" || declaringType.DeclaringType is not { } owner
                || !types.Any(owner.IsAssignableFrom)) continue;
            foreach (MethodInfo method in owner.GetMethods(BindingFlags.Instance | BindingFlags.Static
                         | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (IsCombatModelMethod(method)
                    && method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == declaringType)
                    RejectForeignPatches([target]);
        }

        // Materialize variables after auditing their getters, then prove this bridge has no numeric contribution.
        foreach ((Patch patch, MethodBase target) in variableUpgradeBridges)
        {
            Type extensions = patch.PatchMethod.DeclaringType!.Assembly.GetType("BaseLib.Extensions.DynamicVarExtensions")
                ?? throw new TypeLoadException("BaseLib.Extensions.DynamicVarExtensions");
            object upgrades = AccessTools.Field(extensions, "DynamicVarUpgrades").GetValue(null)
                ?? throw new InvalidOperationException("BaseLib DynamicVarUpgrades is not initialized.");
            PropertyInfo getter = upgrades.GetType().GetProperty("Item")
                ?? throw new MissingMemberException(upgrades.GetType().FullName, "Item");
            if (captured.OfType<CardModel>().SelectMany(card => card.DynamicVars)
                .Any(variable => getter.GetValue(upgrades, [variable.Value]) is not null))
            {
                ForeignPatch foreign = TryDescribeForeignPatch(patch, target)!.Value;
                throw new IncompatibleGameplayModException(foreign.ModId, foreign.ModName,
                    foreign.Description + " with custom variable upgrades", "combat");
            }
        }

        bool AllowFrameworkBridge(Patch patch, MethodBase target)
        {
            if (emptyCardCapabilities && IsEmptyCardCapabilityBridge(patch, target)) return true;
            if (target.DeclaringType != typeof(CardModel) || !emptyCardModifiers) return false;
            string? typeName = (target.Name, patch.PatchMethod.Name) switch
            {
                ("UpgradeInternal", "UpgradeModifiersOnCard") => "BaseLib.Abstracts.UpgradeModifiers",
                ("FinalizeUpgradeInternal", "FinalizeModifiersOnCard") => "BaseLib.Abstracts.FinalizeModifierUpgrade",
                ("UpgradeInternal", "InsertVarUpgrade") => "BaseLib.Patches.Utils.UpgradeInternalPatch",
                _ => null,
            };
            Type? bridge = typeName is null ? null : AccessTools.TypeByName(typeName);
            if (bridge is null || patch.PatchMethod != AccessTools.Method(bridge, patch.PatchMethod.Name)) return false;
            if (patch.PatchMethod.Name == "InsertVarUpgrade") variableUpgradeBridges.Add((patch, target));
            return true;
        }
    }

    private static bool IsEmptyCardCapabilityBridge(Patch patch, MethodBase target)
    {
        if (target.DeclaringType != typeof(CardModel)) return false;
        string? bridge = (target.Name, patch.PatchMethod.Name) switch
        {
            ("get_TargetType", "Postfix") => "TargetTypePatch",
            ("get_CurrentStarCost", "Postfix") => "StarCostPatch",
            ("get_IsPlayable", "Postfix") => "IsPlayablePatch",
            ("UpgradeInternal", "Prefix" or "Transpiler") => "UpgradeInternalPatch",
            ("FinalizeUpgradeInternal", "Postfix") => "FinalizeUpgradeInternalPatch",
            _ => null,
        };
        if (bridge is null) return false;
        Type? type = AccessTools.TypeByName("STS2RitsuLib.Models.Capabilities.Patches.CardModelCapabilityPatches+" + bridge);
        return type is not null && patch.PatchMethod == AccessTools.Method(type, patch.PatchMethod.Name);
    }

    private static bool IsCombatModelMethod(MethodBase target)
        => CombatModelMethodNames.Contains(target.Name)
            || target is MethodInfo { IsSpecialName: true } numeric
                && numeric.Name.StartsWith("get_", StringComparison.Ordinal)
                && numeric.DeclaringType is { IsAbstract: false } modelType
                && typeof(MonsterModel).IsAssignableFrom(modelType)
                && numeric.ReturnType is { } returnType
                && (returnType == typeof(int) || returnType == typeof(decimal))
            || target is MethodInfo { IsSpecialName: false } move
                && move.DeclaringType is { IsAbstract: false } monsterType
                && typeof(MonsterModel).IsAssignableFrom(monsterType)
                && typeof(Task).IsAssignableFrom(move.ReturnType)
            || target is MethodInfo { IsVirtual: true } method
                && method.GetBaseDefinition().DeclaringType == typeof(AbstractModel)
                && PredictionModHookSubscriberInertness.IsCombatHook(method.GetBaseDefinition().Name);

    internal static void ValidateMonsterModels(IEnumerable<MonsterModel> monsters)
    {
        foreach (MonsterModel monster in monsters)
        {
            _ = AssemblyInfo.ModForType(monster.GetType(), out bool isBaseGame);
            if (!isBaseGame)
                throw PredictionUnsupportedException.ForContent(
                    $"Monster AI and move effects require a prediction implementation: {monster.GetType().FullName}.", monster.GetType());
            RejectForeignPatches([AccessTools.Method(monster.GetType(), "GenerateMoveStateMachine")]);
        }
    }

    internal static void RejectForeignPatches(IEnumerable<MethodBase> methods, Func<Patch, MethodBase, bool>? supportedBridge = null)
    {
        foreach (MethodBase target in methods.Distinct())
            if (Harmony.GetPatchInfo(target) is { } patches)
                foreach (var group in AdaptedCardOnPlayMirrors.Groups(patches))
                    foreach (Patch patch in group.Patches)
                        if (TryDescribeForeignPatch(patch, target) is { } foreign
                            && supportedBridge?.Invoke(patch, target) != true)
                            throw new IncompatibleGameplayModException(foreign.ModId, foreign.ModName, foreign.Description, "combat");
    }

    private static ForeignPatch? TryDescribeForeignPatch(Patch patch, MethodBase target)
    {
        Type? patchType = patch.PatchMethod.DeclaringType;
        if (patchType == null)
            throw new PredictionUnsupportedException(
                $"Unknown Harmony patch {patch.PatchMethod} (owner={patch.owner}) on {target}.");

        var mod = AssemblyInfo.ModForType(patchType, out bool isBaseGame);
        if (isBaseGame)
            return null;
        if (mod?.manifest?.id is not { Length: > 0 } modId)
            throw new PredictionUnsupportedException(
                $"Unknown Harmony patch {patchType.FullName}.{patch.PatchMethod.Name} " +
                $"(owner={patch.owner}) on mirrored {target.DeclaringType?.FullName}.{target.Name}.");
        if (string.Equals(modId, Entry.ModId, StringComparison.OrdinalIgnoreCase))
            return null;

        return new ForeignPatch(
            modId,
            mod.manifest.name ?? string.Empty,
            $"Harmony patch {patchType.FullName}.{patch.PatchMethod.Name} on mirrored "
            + $"{target.DeclaringType?.FullName}.{target.Name}");
    }
}
