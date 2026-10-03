using System.Text.Json;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    // Isolate one existing posterior member, not an autonomous coordinator result.
    private async Task RunQ002OpeningPotionPosteriorAsync(CombatState combat, Player player,
        bool baseScoreOnly, bool tracePath = false, bool boundaryMember = false,
        bool frontierMember = false, bool continueFrontier = false)
    {
        if (_checkpointImport == null || _checkpointImportDirectory == null
            || _request.ReplayMode != "RestoreOnly"
            || _checkpointImport["checkpoint"]!["eventCursor"]!.GetValue<int>() != 0
            || player.PlayerCombatState?.TurnNumber != 1)
            throw new InvalidDataException("O004 posterior requires the original opening root.");
        CombatRootSnapshot root = CombatRootSnapshot.Capture(combat);
        SolverDisplayNames names = SolverDisplayNames.Capture(combat);
        BattleDamageSnapshot damage = BattleDamageTracker.Observe(combat);
        SearchPolicySnapshot policy = SolverController.CaptureSearchPolicy(SolverSettings.Capture(), combat, false, null);
        ContinuationStamp before = ContinuationStamp.CaptureLive(combat);
        CombatBeamSolver builder = new(root, names, damage, policy,
            potionPolicyOverride: SolverPotionPolicy.RequireAtLeastOne, maximumPotionUses: 2);
        PlanAction potion = builder.BuildOpeningPotionActions().Single(action =>
            action.PotionId == "POWER_POTION"
            && action.Choice?.Cards.SingleOrDefault()?.CardId == "TOOLS_OF_THE_TRADE");
        PlanAction power = builder.BuildPowerActionsAfterPrefix([potion])
            .Single(action => action.CardId == "TOOLS_OF_THE_TRADE");
        PlanAction[] prefix = [potion, power];
        SimulationSnapshot setup = builder.ReplayDiagnosticPrefix(prefix);
        ContinuationStamp expected;
        try { expected = builder.CaptureDiagnosticContinuation(setup); }
        finally { setup.ReleaseSimulator(); }
        JsonObject recorded = _checkpointImport["index"]!["searchResults"]!.AsArray()
            .OfType<JsonObject>().Last(item => item["eventCursor"]?.GetValue<int>() == 4
                && item["combatEndedTurn"] != null && item["deathTurn"] == null);
        PlanAction[] suffix = recorded["plannedActions"]!.Deserialize<PlanAction[]>(UnattendedTestFiles.JsonOptions)!;
        SimulationSnapshot witness = builder.ReplayDiagnosticPrefix([.. prefix, .. suffix]);
        try
        {
            if (!witness.AllEnemiesDead || witness.PlayerDead || witness.CombatEndedTurn != 12
                || damage.HpLostSoFar + witness.CumulativePlayerHpLost != 5 || witness.PotionUseCount != 2)
                throw new InvalidOperationException("O004 complete opening witness differs from its winning outcome.");
        }
        finally { witness.ReleaseSimulator(); }
        if (ContinuationStamp.CaptureLive(combat) != before)
            throw new InvalidOperationException("O004 posterior setup changed the live opening root.");
        string eventsPath = Path.Combine(_checkpointImportDirectory,
            _checkpointImport["index"]!["recording"]!["eventsPath"]!.GetValue<string>());
        RecordedCombatEvent[] events = File.ReadLines(eventsPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<RecordedCombatEvent>(line)!).ToArray();
        if (events.Length != 4 || events.Where((item, index) => item.Sequence != index).Any())
            throw new InvalidDataException("O004 requires all four original native events.");
        using (NativeReplayDriver native = new(this, events, 4, player))
            await native.AdvanceAsync(Task.CompletedTask);
        ContinuationStamp actual = ContinuationStamp.CaptureLive(combat);
        if (expected != actual)
            throw new InvalidOperationException("O004 opening differs from native setup: " + expected.DescribeFirstDifference(actual));
        SimulationSnapshot frozen = builder.ReplayDiagnosticPrefix([]);
        try
        {
            if (builder.CaptureDiagnosticContinuation(frozen) != before)
                throw new InvalidOperationException("O004 frozen opening changed after native advancement.");
        }
        finally { frozen.ReleaseSimulator(); }
        PlanAction[] memberPrefix = boundaryMember
            ? [.. prefix, .. suffix.TakeWhile(action => action.Turn == 1)] : prefix;
        if (boundaryMember && memberPrefix.LastOrDefault()?.Kind != PlanActionKind.EndTurn)
            throw new InvalidOperationException("O004 boundary member requires the complete saved first turn.");
        if (boundaryMember)
        {
            SimulationSnapshot boundary = builder.ReplayDiagnosticPrefix(memberPrefix);
            try
            {
                if (boundary.Turn != 2 || boundary.PlayerDead || boundary.AllEnemiesDead)
                    throw new InvalidOperationException("O004 saved first turn did not reach a searchable T2.");
                _writer.WriteGeneratedArtifact("O004-boundary-root.json", new
                { memberPrefix, boundary = builder.CaptureDiagnosticContinuation(boundary).StateText });
            }
            finally { boundary.ReleaseSimulator(); }
        }
        StateFingerprint[] watchedSteps = new StateFingerprint[8];
        PlanAction[] witnessPrefix = [.. prefix, .. suffix.Take(6)];
        if (tracePath)
        {
            for (int index = 0; index < watchedSteps.Length; index++)
            {
                SimulationSnapshot needle = builder.ReplayDiagnosticPrefix(witnessPrefix.Take(index + 1).ToArray());
                try { watchedSteps[index] = needle.StateKey; }
                finally { needle.ReleaseSimulator(); }
            }
        }
        _writer.WriteGeneratedArtifact("O004-posterior-root.json", new
        { prefix, expected = expected.StateText, actual = actual.StateText, witnessActions = suffix.Length + 2 });
        _completedChecks.Add("Q002O004:OpeningWitness:Native4Events:ExactContinuation:FrozenRoot:T12Loss5Potions2");
        SolverSearchProfile profile = policy.Profile with
        {
            MaxExpandedNodes = Math.Min(policy.Profile.MaxExpandedNodes, 60_000),
            SoftTimeBudgetMilliseconds = Math.Min(policy.Profile.SoftTimeBudgetMilliseconds, 30_000),
            BaseScoreOnly = baseScoreOnly,
            BeamWidth = BeamWidthPortfolio.ScaledWidth(policy.Profile.BeamWidth, BeamWidthPortfolio.WideRefinementRatio),
        };
        for (int maximum = baseScoreOnly ? 1 : 2; maximum <= 2; maximum++)
        {
            EnsureWithinDeadline();
            SetStage($"q002_o004_posterior_maximum_{maximum}");
            List<SearchPathObservation> observations = [];
            object observationGate = new();
            int dropped = 0;
            SearchPolicySnapshot memberPolicy = policy;
            if (tracePath)
            {
                HashSet<StateFingerprint> watched = watchedSteps.ToHashSet();
                HashSet<StateFingerprint> retention = [watchedSteps[5]];
                SearchPathObserver observer = new(watched.Contains, observation =>
                {
                    lock (observationGate)
                    {
                        if (observations.Count < 16_384) observations.Add(observation);
                        else dropped++;
                    }
                }, retention.Contains);
                memberPolicy = policy with
                { Diagnostics = new SearchDiagnosticsSink(policy.Diagnostics.Info, policy.Diagnostics.Debug, observer) };
            }
            List<SolverCurrentTurnPreview> previews = [];
            HashSet<string> previewKeys = [];
            bool captureLimitReached = false;
            void ObserveFrontier(SolverProgress progress)
            {
                if (progress.CurrentTurnPreview is not { } preview) return;
                if (previews.Count >= 64) { captureLimitReached = true; return; }
                if (previewKeys.Add(JsonSerializer.Serialize(preview.Actions, UnattendedTestFiles.JsonOptions)))
                    previews.Add(preview with { Actions = preview.Actions.ToArray(), FrontierTurns = null });
            }
            SolverResult candidate = await Task.Run(() => new CombatBeamSolver(root, names, damage,
                memberPolicy, progressCallback: frontierMember ? ObserveFrontier : null,
                searchProfile: profile, potionPolicyOverride: SolverPotionPolicy.RequireAtLeastOne,
                maximumPotionUses: maximum, fixedPrefixActions: memberPrefix,
                resetFixedPrefixSchedulingBaseline: boundaryMember).Solve());
            _writer.WriteGeneratedArtifact($"O004-posterior-{maximum}.json", new
            {
                profile, maximum, memberPrefix, resetFixedPrefixSchedulingBaseline = boundaryMember,
                candidate.OnlyDeathRoutesFound, candidate.ProjectedBattleHpLost,
                candidate.CombatEndedTurn, candidate.ExpandedNodes, candidate.TransitionCount,
                elapsedMilliseconds = candidate.Elapsed.TotalMilliseconds,
                candidate.Snapshot.AllEnemiesDead, candidate.Snapshot.EnemyHp,
                candidate.PotionCount, actions = candidate.BestNode.Actions,
            });
            if (frontierMember)
                _writer.WriteGeneratedArtifact("O004-posterior-frontier.json", new
                { captureLimitReached, previews });
            if (continueFrontier)
            {
                EnsureWithinDeadline();
                if (captureLimitReached || previews.Count == 0)
                    throw new InvalidOperationException("O004 continuation lacks a complete frontier capture.");
                PlanAction[] frontierPrefix = previews.Last().Actions.ToArray();
                if (frontierPrefix.LastOrDefault()?.Kind != PlanActionKind.EndTurn)
                    throw new InvalidOperationException("O004 frontier did not end its first turn.");
                SetStage("q002_o004_frontier_continuation");
                SolverResult continuation = await Task.Run(() => new CombatBeamSolver(
                    root, names, damage, policy, searchProfile: profile,
                    potionPolicyOverride: SolverPotionPolicy.RequireAtLeastOne,
                    maximumPotionUses: maximum, fixedPrefixActions: frontierPrefix,
                    resetFixedPrefixSchedulingBaseline: true).Solve());
                _writer.WriteGeneratedArtifact("O004-frontier-continuation.json", new
                {
                    profile, maximum, frontierPrefix,
                    continuation.OnlyDeathRoutesFound, continuation.ProjectedBattleHpLost,
                    continuation.CombatEndedTurn, continuation.ExpandedNodes, continuation.TransitionCount,
                    elapsedMilliseconds = continuation.Elapsed.TotalMilliseconds,
                    continuation.Snapshot.AllEnemiesDead, continuation.PotionCount,
                    actions = continuation.BestNode.Actions,
                });
            }
            if (tracePath)
            {
                _writer.WriteGeneratedArtifact("O004-posterior-path.json", new
                {
                    dropped, witnessPrefix, watchedSteps,
                    observations = observations.Select(observation => new
                    {
                        steps = Enumerable.Range(0, watchedSteps.Length)
                            .Where(index => watchedSteps[index] == observation.StateKey).Select(index => index + 1),
                        exactSteps = Enumerable.Range(0, watchedSteps.Length)
                            .Where(index => watchedSteps[index] == observation.StateKey
                                && observation.Actions.Select(KnownRouteActionIdentity).SequenceEqual(
                                    witnessPrefix.Take(index + 1).Select(KnownRouteActionIdentity)))
                            .Select(index => index + 1),
                        observation,
                    }),
                });
                if (dropped != 0)
                    throw new InvalidOperationException("O004 posterior path observations were truncated.");
            }
        }
        if (ContinuationStamp.CaptureLive(combat) != actual)
            throw new InvalidOperationException("O004 posterior search changed the live advanced root.");
        SimulationSnapshot afterSearch = builder.ReplayDiagnosticPrefix([]);
        try
        {
            if (builder.CaptureDiagnosticContinuation(afterSearch) != before)
                throw new InvalidOperationException("O004 posterior search changed its frozen opening.");
        }
        finally { afterSearch.ReleaseSimulator(); }
    }
}
