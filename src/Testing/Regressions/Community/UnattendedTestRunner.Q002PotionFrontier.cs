using MegaCrit.Sts2.Core.Combat;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    private async Task RunQ002PotionBoundarySetupAsync(CombatState live,
        KnownRouteSearchContext context, PlanAction[] setup, PlanAction[] witnessOpening,
        ContinuationStamp frozenBefore, ContinuationStamp liveBefore)
    {
        SolverSearchProfile profile = context.Policy.Profile with
        {
            MaxExpandedNodes = Math.Min(60_000, context.Policy.Profile.MaxExpandedNodes),
            SoftTimeBudgetMilliseconds = Math.Min(30_000, context.Policy.Profile.SoftTimeBudgetMilliseconds),
            BeamWidth = BeamWidthPortfolio.ScaledWidth(context.Policy.Profile.BeamWidth,
                BeamWidthPortfolio.WideRefinementRatio),
            BaseScoreOnly = false,
        };
        CombatBeamSolver builder = new(context.Root, context.Names, context.Damage, context.Policy);
        SimulationSnapshot witness = builder.ReplayDiagnosticPrefix(witnessOpening);
        StateFingerprint witnessKey;
        try { witnessKey = witness.StateKey; }
        finally { witness.ReleaseSimulator(); }
        EarlyTurnFrontierCandidate[] frontiers = [];
        SetStage("q002_o004_full_first_turn_frontier");
        SolverResult control = await Task.Run(() => new CombatBeamSolver(context.Root,
            context.Names, context.Damage, context.Policy, searchProfile: profile,
            potionPolicyOverride: SolverPotionPolicy.RequireAtLeastOne, maximumPotionUses: 2,
            fixedPrefixActions: setup, resetFixedPrefixSchedulingBaseline: false,
            earlyTurnScoutObserver: (turns, candidates) =>
            {
                if (turns == 1)
                    frontiers = candidates.Select(candidate => candidate with
                    { Actions = candidate.Actions.ToArray() }).ToArray();
            }).Solve());
        _writer.WriteGeneratedArtifact("O004-full-first-turn-frontier.json", new
        {
            profile, setup, witnessOpening, witnessKey, frontiers,
            witnessSelected = frontiers.Any(candidate => candidate.StateKey == witnessKey),
            control.OnlyDeathRoutesFound, control.ProjectedBattleHpLost,
            control.ExpandedNodes, control.TransitionCount,
        });
        PlanAction potion = builder.BuildPotionActionsAfterPrefix(witnessOpening)
            .First(action => action.Kind == PlanActionKind.UsePotion);
        PlanAction[] potionPrefix = [.. witnessOpening, potion];
        PlanAction power = builder.BuildPowerActionsAfterPrefix(potionPrefix)
            .Where(action => PowerCardValuationModels.Registry.ContainsCardId(action.CardId!))
            .OrderByDescending(action => PowerCardValuationModels.Registry
                .TryGetCommitmentDescriptor(action.CardId!, out PowerCommitmentDescriptor descriptor)
                    ? descriptor.Priority : 0).First();
        PlanAction[] memberPrefix = [.. potionPrefix, power];
        EnsureWithinDeadline();
        SetStage("q002_o004_boundary_potion_power");
        SolverResult member = await Task.Run(() => new CombatBeamSolver(context.Root,
            context.Names, context.Damage, context.Policy, searchProfile: profile,
            potionPolicyOverride: SolverPotionPolicy.RequireAtLeastOne, maximumPotionUses: 2,
            fixedPrefixActions: memberPrefix, resetFixedPrefixSchedulingBaseline: true).Solve());
        _writer.WriteGeneratedArtifact("O004-boundary-potion-power.json", new
        {
            profile, memberPrefix, member.OnlyDeathRoutesFound, member.ProjectedBattleHpLost,
            member.CombatEndedTurn, member.ExpandedNodes, member.TransitionCount,
            elapsedMilliseconds = member.Elapsed.TotalMilliseconds,
            member.Snapshot.AllEnemiesDead, member.PotionCount, actions = member.BestNode.Actions,
        });
        if (ContinuationStamp.CaptureLive(live) != liveBefore)
            throw new InvalidOperationException("O004 boundary setup modified live combat.");
        SimulationSnapshot frozen = builder.ReplayDiagnosticPrefix([]);
        try
        {
            if (builder.CaptureDiagnosticContinuation(frozen) != frozenBefore)
                throw new InvalidOperationException("O004 boundary setup modified its frozen root.");
        }
        finally { frozen.ReleaseSimulator(); }
        _completedChecks.Add("Q002O004:FullFirstTurnFrontier:BoundaryPotionPower:FrozenLiveUnchanged");
    }
}
