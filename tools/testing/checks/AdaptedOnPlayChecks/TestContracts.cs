// Real Harmony and production admission/dispatch code; game identities and effects are small managed fixtures.
namespace MegaCrit.Sts2.Core.Models
{
    internal abstract class AbstractModel
    {
        public virtual bool ShouldReceiveCombatHooks => true;
        public virtual bool IsMock => false;
        public virtual void PreviewOutsideOfCombat() { }
        public virtual void CompareTo() { }
        public virtual void ModifyGeneratedMap() { }
        public virtual void ModifyGeneratedMapLate() { }
        public virtual void AfterMapGenerated() { }
        public virtual void AfterActEntered() { }
        public virtual void ModifyNextEvent() { }
        public virtual void ModifyUnknownMapPointRoomTypes() { }
        public virtual void ModifyOddsIncreaseForUnrolledRoomType() { }
        public virtual void AfterRestSiteHeal() { }
        public virtual void AfterRestSiteSmith() { }
        public virtual void ModifyRestSiteHealAmount() { }
        public virtual void TryModifyRestSiteHealRewards() { }
        public virtual void TryModifyRestSiteOptions() { }
        public virtual void AfterItemPurchased() { }
        public virtual void ModifyMerchantCardPool() { }
        public virtual void ModifyMerchantCardRarity() { }
        public virtual void ModifyMerchantCardCreationResults() { }
        public virtual void ModifyMerchantPrice() { }
        public virtual void BeforeCombatStart() { }
        public virtual void BeforeCombatStartLate() { }
        public virtual void AfterCombatVictoryEarly() { }
        public virtual void AfterCombatVictory() { }
        public virtual void AfterCombatEnd() { }
        public virtual void BeforeCombatRewardOffered() { }
    }
    internal abstract class MonsterModel : AbstractModel;
    internal abstract class RelicModel : AbstractModel;
    internal abstract class ModifierModel : AbstractModel;
    internal abstract class CardModel : AbstractModel
    {
        public int Value;
        protected virtual int CanonicalVars => 10;
        public Dictionary<string, object> DynamicVars = [];
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void UpgradeInternal() { }
        protected virtual void OnPlay(GameActions.Multiplayer.PlayerChoiceContext context, Entities.Cards.CardPlay play) { }
        public void Play() => OnPlay(new(), new());
    }
}
namespace MegaCrit.Sts2.Core.Entities.Cards { internal sealed class CardPlay; }
namespace MegaCrit.Sts2.Core.GameActions.Multiplayer { internal sealed class PlayerChoiceContext; }
namespace MegaCrit.Sts2.Core.Modding
{
    internal sealed class ModManifest { public string id = "neutral-test"; public string name = "Neutral Test"; public bool affectsGameplay = true; }
    internal sealed class Mod
    {
        public ModManifest? manifest = new();
        public List<System.Reflection.Assembly> assemblies = [];
    }
    internal static class ModManager
    {
        public static List<Mod> Mods = [];
        public static IEnumerable<Mod> GetLoadedMods() => Mods;
    }
    internal static class AssemblyInfo
    {
        public static bool Unknown;
        public static bool Neutral;
        public static Mod? ModForType(Type type, out bool isBaseGame)
        {
            isBaseGame = false;
            return Unknown ? null : new Mod { manifest = new() { affectsGameplay = !Neutral } };
        }
    }
}
namespace CombatSolver
{
    internal static class RitsuEmptyCapabilityFastPath
    {
        public static bool HasEmptyCapabilities(MegaCrit.Sts2.Core.Models.AbstractModel model) => true;
    }
    internal static class Entry
    {
        public const string ModId = "CombatSolver";
        public static Log Logger = new();
        public sealed class Log { public void Info(string message) { } }
    }
    internal sealed class IncompatibleGameplayModException(string id, string name, string description, string scope)
        : NotSupportedException($"{id}/{name}/{scope}: {description}");
}
namespace CombatSolver.Engine.Common
{
    internal static class PredictionModModelSupport
    {
        internal static bool AttachedModifiers;
        internal static bool HasBaseLibCardModifiers(MegaCrit.Sts2.Core.Models.CardModel card) => AttachedModifiers;
    }
    internal sealed class PredictedCard(MegaCrit.Sts2.Core.Models.CardModel preview)
    {
        public MegaCrit.Sts2.Core.Models.CardModel Preview => preview;
        public MegaCrit.Sts2.Core.Models.CardModel MutablePreview => preview;
    }
    internal sealed class PredictionTrace
    {
        internal readonly struct TraceScope : IDisposable { public void Dispose() { } }
    }
}

namespace BaseLib.Extensions
{
    internal static class DynamicVarExtensions
    {
        public static readonly Dictionary<object, decimal?> DynamicVarUpgrades = [];
    }
}
namespace BaseLib.Patches.Utils
{
    internal static class UpgradeInternalPatch
    {
        public static IEnumerable<HarmonyLib.CodeInstruction> InsertVarUpgrade(IEnumerable<HarmonyLib.CodeInstruction> code) => code;
    }
}
namespace BaseLib.Abstracts
{
    internal static class UpgradeModifiers
    {
        public static void UpgradeModifiersOnCard() { }
    }
}
namespace CombatSolver.Engine.InCombat.Simulation { internal sealed class CombatPredictionSimulator; }
namespace CombatSolver.Engine.InCombat.Mirrors.Cards.OnPlay
{
    internal sealed class CardOnPlayMirrorContext : Common.Mirrors.IMethodMirrorContext<MegaCrit.Sts2.Core.Models.CardModel>
    {
        public required Simulation.CombatPredictionSimulator Simulator { get; init; }
        public required Common.PredictedCard Card { get; init; }
        public required MegaCrit.Sts2.Core.Entities.Cards.CardPlay CardPlay { get; init; }
        public Common.PredictionTrace.TraceScope PushDispatchSource(MegaCrit.Sts2.Core.Models.CardModel receiver,
            Common.Mirrors.MirrorMethodSpec method) => new();
        public void RecordMethodNotMirroredRisk() => throw new InvalidOperationException("Unexpected unsupported dispatch.");
        public void RecordMethodMirrorIncompleteRisk() => throw new InvalidOperationException("Unexpected inferred dispatch.");
    }
}
