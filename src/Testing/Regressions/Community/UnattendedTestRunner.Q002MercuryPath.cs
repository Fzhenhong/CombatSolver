using System.Text.Json;
using CombatSolver.Engine.Common;
using CombatSolver.Engine.InCombat.Simulation;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Multiplayer.Replay;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    // Watch the player's T3 intervention; the preceding two turns were solver-owned.
    // Native indices identify instances. These actions are never injected into search.
    private async Task RunQ002MercuryPlayerTurnPathAsync(CombatState combat, Player player,
        bool boundaryMember = false, bool boundaryPowerPath = false)
    {
        if (_checkpointImport == null || _checkpointImportDirectory == null
            || _request.ReplayMode != "RestoreOnly"
            || _checkpointImport["checkpoint"]!["eventCursor"]!.GetValue<long>() != 20
            || player.PlayerCombatState?.TurnNumber != 3 || combat.Enemies.Count != 1)
            throw new InvalidOperationException("O003 player path requires its recorded T3/event-20 root.");
        string eventsPath = Path.Combine(_checkpointImportDirectory,
            _checkpointImport["index"]!["recording"]!["eventsPath"]!.GetValue<string>());
        RecordedCombatEvent[] events = File.ReadLines(eventsPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<RecordedCombatEvent>(line)!).ToArray();
        if (events.Length != 29 || events.Where((item, index) => item.Sequence != index).Any())
            throw new InvalidDataException("O003 requires all 29 original native events.");
        PacketReader reader = new();
        reader.Reset(events[21].Payload);
        CombatReplayEvent choiceEvent = reader.Read<CombatReplayEvent>();
        if (choiceEvent.eventType != CombatReplayEventType.PlayerChoice
            || choiceEvent.playerId != player.NetId || choiceEvent.playerChoiceResult is not { } net)
            throw new InvalidDataException("O003 event 21 is not the player's Acrobatics choice.");
        var recordedChoice = PlayerChoiceResult.FromNetData(player, combat.RunState, net);
        string[] selectedIds = recordedChoice.AsCards(net.type).Select(card => card.Id.Entry).ToArray();
        if (selectedIds.Length != 1)
            throw new InvalidDataException("O003 Acrobatics must discard exactly one recorded card.");
        var enemy = combat.Enemies.Single();
        ContinuationStamp before = ContinuationStamp.CaptureLive(combat);
        CombatRootSnapshot root = CombatRootSnapshot.Capture(combat);
        SearchPolicySnapshot policy = SolverController.CaptureSearchPolicy(SolverSettings.Capture(), combat, false, null);
        KnownRouteSearchContext context = new(root, SolverDisplayNames.Capture(combat),
            BattleDamageTracker.Observe(combat), policy);
        CombatBeamSolver driver = new(root, context.Names, context.Damage, policy);
        uint[] nativeIds = [19, 22, 23, 28, 0];
        string[] cardIds = ["ACROBATICS", "FOOTWORK", "CALCULATED_GAMBLE", "ACCURACY", "SIDESTEP"];
        List<PlanAction> actions = [];
        List<KnownRoutePrefix> prefixes = [];
        List<SimulationSnapshot> owned = [];
        ContinuationStamp predicted;
        try
        {
            using (SimulationNotificationIsolation.Enter())
            {
                SimulationSnapshot parent = ReplayKnownCustom(driver, [], null, 0, 0, owned);
                AssertSnapshotEqual(CaptureSimulated(parent.Simulator,
                    (SimulatedCombatState)parent.Simulator.State.CombatState, player, enemy),
                    CaptureActual(combat, player, enemy), "Q002O003PlayerT3", "InitialRoot");
                for (int index = 0; index <= nativeIds.Length; index++)
                {
                    EnsureWithinDeadline();
                    SetStage($"q002_o003_player_T3_step_{index + 1}");
                    AssertKnownSoulStable(parent, 3, "O003:T3:parent");
                    PlanAction action = new(PlanActionKind.EndTurn, 3);
                    if (index < nativeIds.Length)
                    {
                        var hand = parent.Simulator.State.GetPlayerCombatState(player).Hand.Cards;
                        var original = NetCombatCard.ForTesting(nativeIds[index]).ToCardModel();
                        PredictedCard card = hand.Single(candidate => ReferenceEquals(candidate.Original, original));
                        if (card.Preview.Id.Entry != cardIds[index])
                            throw new InvalidOperationException("O003 recorded native card identity differs.");
                        string key = CardChoiceSupport.ChoiceCardKey(card);
                        action = new(PlanActionKind.PlayCard, 3, CardId: cardIds[index],
                            CardOccurrence: hand.TakeWhile(candidate => !ReferenceEquals(candidate, card))
                                .Count(candidate => candidate.Preview.Id.Entry == cardIds[index]),
                            CardStateKey: key,
                            CardStateOccurrence: hand.TakeWhile(candidate => !ReferenceEquals(candidate, card))
                                .Count(candidate => CardChoiceSupport.ChoiceCardKey(candidate) == key),
                            ReplayCount: Math.Max(0, card.Preview.GetEnchantedReplayCount()));
                        if (index == 0)
                        {
                            SimulationSnapshot probe = ReplayKnownCustom(driver, [action], parent, 3, index, owned);
                            try
                            {
                                var pending = ((SimulatedCombatState)probe.Simulator.State.CombatState).PendingTurnStartChoice;
                                if (pending is not { Spec: { } spec, Effect: PlanChoiceEffect.Discard, SourcePile: PileType.Hand }
                                    || probe.BoundaryReason != SearchBoundaryReason.PendingChoice
                                    || pending.Timing != PlanChoiceTiming.Action
                                    || spec.Options.Count(option => option.Preview.Id.Entry == selectedIds[0]) != 1)
                                    throw new InvalidOperationException("O003 Acrobatics lacks its unique recorded discard choice.");
                                action = action with
                                {
                                    Choice = CardChoiceSupport.BuildRequestedChoice(spec, selectedIds) with
                                    {
                                        SourceId = pending.SourceId, ContextId = pending.ContextId, Timing = pending.Timing,
                                    },
                                };
                            }
                            finally { probe.ReleaseSimulator(); }
                        }
                    }
                    actions.Add(action);
                    SimulationSnapshot next = ReplayKnownCustom(driver, [action], parent, 3, index, owned);
                    SimulationSnapshot full = ReplayKnownCustom(driver, actions, null, 0, 0, owned);
                    InvokeKnownCustomMethod(driver, "AssertIncrementalEquivalent", [action, actions.ToArray(), next, full]);
                    AssertKnownSoulStable(next, index == nativeIds.Length ? 4 : 3, "O003:T3:step");
                    prefixes.Add(FreezeKnownRoutePrefix(action, CaptureSimulated(next.Simulator,
                        (SimulatedCombatState)next.Simulator.State.CombatState, player, enemy), next));
                    full.ReleaseSimulator();
                    parent.ReleaseSimulator();
                    parent = next;
                }
                predicted = driver.CaptureDiagnosticContinuation(parent);
                var winning = _checkpointImport["index"]!["searchResults"]!.AsArray()
                    .OfType<System.Text.Json.Nodes.JsonObject>().Last(item =>
                        item["eventCursor"]?.GetValue<int>() == 29
                        && item["startTurnNumber"]?.GetValue<int>() == 4
                        && item["combatEndedTurn"] != null && item["deathTurn"] == null);
                PlanAction[] suffix = winning["plannedActions"]!.Deserialize<PlanAction[]>(UnattendedTestFiles.JsonOptions)!;
                foreach (PlanAction action in boundaryMember ? [] : suffix)
                {
                    EnsureWithinDeadline();
                    SetStage($"q002_o003_frozen_suffix_{actions.Count + 1}");
                    if (parent.PlayerDead || parent.AllEnemiesDead || action.Turn != parent.Turn)
                        throw new InvalidOperationException("O003 suffix has an invalid terminal or turn boundary.");
                    int priorCount = actions.Count;
                    actions.Add(action);
                    SimulationSnapshot next = ReplayKnownCustom(driver, [action], parent, parent.Turn, priorCount, owned);
                    SimulationSnapshot full = ReplayKnownCustom(driver, actions, null, 0, 0, owned);
                    InvokeKnownCustomMethod(driver, "AssertIncrementalEquivalent", [action, actions.ToArray(), next, full]);
                    prefixes.Add(FreezeKnownRoutePrefix(action, CaptureSimulated(next.Simulator,
                        (SimulatedCombatState)next.Simulator.State.CombatState, player, enemy), next));
                    full.ReleaseSimulator();
                    parent.ReleaseSimulator();
                    parent = next;
                }
                if (!boundaryMember && (!parent.AllEnemiesDead || parent.PlayerDead
                    || parent.CombatEndedTurn != winning["combatEndedTurn"]!.GetValue<int>()
                    || context.Damage.HpLostSoFar + parent.CumulativePlayerHpLost
                        != winning["projectedBattleHpLost"]!.GetValue<int>()))
                    throw new InvalidOperationException("O003 T3 intervention and saved suffix do not yield the recorded victory.");
            }
        }
        finally
        {
            foreach (SimulationSnapshot snapshot in owned)
                if (snapshot.HasSimulator) snapshot.ReleaseSimulator();
        }
        if (ContinuationStamp.CaptureLive(combat) != before)
            throw new InvalidOperationException("O003 replay changed its live T3 root.");
        using (NativeReplayDriver native = new(this, events.Skip(20).ToArray(), 9, player))
            await native.AdvanceAsync(Task.CompletedTask);
        ContinuationStamp actual = ContinuationStamp.CaptureLive(combat);
        _writer.WriteGeneratedArtifact("O003-player-T3-native.json", new
        {
            selectedIds, actions, expected = predicted.StateText, actual = actual.StateText, matches = predicted == actual,
        });
        if (predicted != actual)
            throw new InvalidOperationException("O003 T3 differs from native T4: " + predicted.DescribeFirstDifference(actual));
        _completedChecks.Add("Q002O003:PlayerT3:Native9Events:Simulation6Actions:ExactT4Continuation");
        CombatBeamSolver fresh = new(root, context.Names, context.Damage, policy);
        SimulationSnapshot frozen = fresh.ReplayDiagnosticPrefix([]);
        try
        {
            if (fresh.CaptureDiagnosticContinuation(frozen) != before)
                throw new InvalidOperationException("O003 frozen T3 changed after native T4.");
        }
        finally { frozen.ReleaseSimulator(); }
        if (boundaryMember)
        {
            await RunQ002TurnBoundaryMembersAsync(combat, context, actions.ToArray(), before, actual);
            return;
        }
        await RunKnownRoutePathTraceAsync(combat, player, prefixes, "Q002O003PlayerT3",
            "q002_o003_player_T3_path", requiredRetentionStep: boundaryPowerPath ? 32 : 7,
            proveRetentionAliases: true, frozenSearchContext: context,
            diagnosticFixedPrefix: boundaryPowerPath ? actions.Take(7).ToArray() : null,
            diagnosticMemberProfile: boundaryPowerPath ? policy.Profile with
            {
                MaxExpandedNodes = Math.Min(62_500, policy.Profile.MaxExpandedNodes),
                SoftTimeBudgetMilliseconds = Math.Min(10_000, policy.Profile.SoftTimeBudgetMilliseconds),
                AggressivePowerCommitment = false, SecondRankBand = false, BaseScoreOnly = false,
            } : null);
    }
}
