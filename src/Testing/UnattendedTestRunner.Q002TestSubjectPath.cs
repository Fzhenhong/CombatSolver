using System.Text.Json;
using CombatSolver.Engine.Common;
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
    // Original O005 player opening, used only as a watched search needle.
    // Native events establish its choice and its complete T2 state before observation.
    private async Task RunQ002TestSubjectOpeningPathAsync(CombatState combat, Player player)
    {
        if (_checkpointImport == null || _checkpointImportDirectory == null
            || _request.ReplayMode != "RestoreOnly"
            || _checkpointImport["checkpoint"]!["eventCursor"]!.GetValue<long>() != 0
            || combat.Players.Count != 1 || combat.Enemies.Count != 1
            || combat.Enemies[0].Monster?.Id.Entry != "TEST_SUBJECT"
            || player.PlayerCombatState is not { TurnNumber: 1 } pcs
            || player.Creature.CurrentHp != 40 || pcs.Energy != 5)
            throw new InvalidOperationException("O005 path requires the original playable T1 root.");

        string eventsPath = Path.Combine(_checkpointImportDirectory,
            _checkpointImport["index"]!["recording"]!["eventsPath"]!.GetValue<string>());
        RecordedCombatEvent[] events = File.ReadLines(eventsPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<RecordedCombatEvent>(line)!).ToArray();
        if (events.Length != 12 || events.Where((item, index) => item.Sequence != index).Any())
            throw new InvalidDataException("O005 opening requires exactly 12 original events.");
        PacketReader reader = new();
        reader.Reset(events[2].Payload);
        CombatReplayEvent choiceEvent = reader.Read<CombatReplayEvent>();
        if (choiceEvent.eventType != CombatReplayEventType.PlayerChoice
            || choiceEvent.playerId != player.NetId || choiceEvent.playerChoiceResult is not { } net)
            throw new InvalidDataException("O005 event 2 is not the recorded player choice.");
        var recordedChoice = PlayerChoiceResult.FromNetData(player, combat.RunState, net);
        string[] selectedIds = recordedChoice.AsCards(net.type).Select(card => card.Id.Entry).ToArray();
        _writer.WriteGeneratedArtifact("O005-recorded-choice.json", new { type = net.type, selectedIds });
        if (selectedIds.Length != 1)
            throw new InvalidDataException("O005 recorded Burning Pact choice must select exactly one card.");

        var enemy = combat.Enemies[0];
        ContinuationStamp liveBefore = ContinuationStamp.CaptureLive(combat);
        CombatRootSnapshot root = CombatRootSnapshot.Capture(combat);
        SearchPolicySnapshot policy = SolverController.CaptureSearchPolicy(
            SolverSettings.Capture(), combat, false, null);
        KnownRouteSearchContext context = new(root, SolverDisplayNames.Capture(combat),
            BattleDamageTracker.Observe(combat), policy);
        CombatBeamSolver driver = new(root, context.Names, context.Damage, policy);
        string[] cards = ["DARK_EMBRACE", "BURNING_PACT", "ARMAMENTS", "BLOODLETTING",
            "BULLY", "FIEND_FIRE", "ENTHRALLED", "CRUELTY", ""];
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
                    CaptureActual(combat, player, enemy), "Q002O005Opening", "InitialRoot");
                for (int index = 0; index < cards.Length; index++)
                {
                    EnsureWithinDeadline();
                    AssertKnownSoulStable(parent, 1, $"O005:parent:{index + 1}");
                    PlanAction action = new(PlanActionKind.EndTurn, 1);
                    if (cards[index].Length != 0)
                    {
                        var hand = parent.Simulator.State.GetPlayerCombatState(player).Hand.Cards;
                        PlanAction descriptor = new(PlanActionKind.PlayCard, 1,
                            CardId: cards[index], CardOccurrence: 0);
                        PredictedCard card = CombatBeamSolver.FindCardForReplay(hand, descriptor)
                            ?? throw new InvalidOperationException($"O005 step {index + 1}: missing {cards[index]}.");
                        string key = CardChoiceSupport.ChoiceCardKey(card);
                        bool targeted = cards[index] is "BULLY" or "FIEND_FIRE";
                        action = descriptor with
                        {
                            TargetIndex = targeted ? 0 : -1,
                            TargetCombatId = targeted ? enemy.CombatId : null,
                            CardStateKey = key,
                            CardStateOccurrence = hand.TakeWhile(candidate => !ReferenceEquals(candidate, card))
                                .Count(candidate => CardChoiceSupport.ChoiceCardKey(candidate) == key),
                            ReplayCount = Math.Max(0, card.Preview.GetEnchantedReplayCount()),
                        };
                    }
                    if (index == 1)
                    {
                        SimulationSnapshot probe = ReplayKnownCustom(driver, [action], parent, 1, index, owned);
                        try
                        {
                            var pending = ((SimulatedCombatState)probe.Simulator.State.CombatState).PendingTurnStartChoice;
                            if (pending is not { Spec: { } spec, Effect: PlanChoiceEffect.Exhaust, SourcePile: PileType.Hand }
                                || probe.BoundaryReason != SearchBoundaryReason.PendingChoice
                                || pending.Timing != PlanChoiceTiming.Action
                                || spec.Effect != PlanChoiceEffect.Exhaust || spec.SourcePile != PileType.Hand)
                                throw new InvalidOperationException("O005 Burning Pact did not request its recorded exhaust choice.");
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
                    actions.Add(action);
                    SimulationSnapshot next = ReplayKnownCustom(driver, [action], parent, 1, index, owned);
                    SimulationSnapshot full = ReplayKnownCustom(driver, actions, null, 0, 0, owned);
                    _ = InvokeKnownCustomMethod(driver, "AssertIncrementalEquivalent",
                        [action, actions.ToArray(), next, full]);
                    AssertKnownSoulStable(next, index == cards.Length - 1 ? 2 : 1, $"O005:step:{index + 1}");
                    prefixes.Add(FreezeKnownRoutePrefix(action, CaptureSimulated(next.Simulator,
                        (SimulatedCombatState)next.Simulator.State.CombatState, player, enemy), next));
                    full.ReleaseSimulator();
                    parent.ReleaseSimulator();
                    parent = next;
                }
                predicted = driver.CaptureDiagnosticContinuation(parent);
            }
            if (ContinuationStamp.CaptureLive(combat) != liveBefore)
                throw new InvalidOperationException("O005 reference replay changed the original live root.");
        }
        finally
        {
            foreach (SimulationSnapshot snapshot in owned)
                if (snapshot.HasSimulator)
                    snapshot.ReleaseSimulator();
        }

        SetStage("q002_o005_native_opening");
        using (NativeReplayDriver native = new(this, events, events.Length, player))
            await native.AdvanceAsync(Task.CompletedTask);
        ContinuationStamp actual = ContinuationStamp.CaptureLive(combat);
        _writer.WriteGeneratedArtifact("O005-opening-state.json", new
        {
            selectedIds, actions, expected = predicted.StateText, actual = actual.StateText,
            matches = predicted == actual,
        });
        if (predicted != actual)
            throw new InvalidOperationException("O005 original opening actual/simulated mismatch: " +
                predicted.DescribeFirstDifference(actual));
        _completedChecks.Add("Q002O005:OriginalNative12Events:Simulation9Actions:ExactT2Continuation:NoNormalization");
        // A newly constructed worker must still see the captured T1, after live has moved to T2.
        // This checks the frozen search input, not merely that observation leaves it unchanged.
        using (SimulationNotificationIsolation.Enter())
        {
            CombatBeamSolver fresh = new(root, context.Names, context.Damage, policy);
            SimulationSnapshot rootProbe = fresh.ReplayDiagnosticPrefix([]);
            try
            {
                ContinuationStamp restoredRoot = fresh.CaptureDiagnosticContinuation(rootProbe);
                if (restoredRoot != liveBefore)
                    throw new InvalidOperationException("O005 frozen T1 changed after native T2: " +
                        liveBefore.DescribeFirstDifference(restoredRoot));
            }
            finally { rootProbe.ReleaseSimulator(); }
        }
        _completedChecks.Add("Q002O005:FreshWorkerAfterNativeT2:ExactOriginalT1Continuation");
        await RunKnownRoutePathTraceAsync(combat, player, prefixes, "Q002O005Opening",
            "q002_o005_opening_path", requiredRetentionStep: 4, frozenSearchContext: context);
    }
}
