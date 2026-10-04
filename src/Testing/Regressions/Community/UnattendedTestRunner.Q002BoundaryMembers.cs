using MegaCrit.Sts2.Core.Combat;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    // A fixed native prefix controls scheduling history, not autonomous route discovery.
    private async Task RunQ002TurnBoundaryMembersAsync(CombatState live,
        KnownRouteSearchContext context, PlanAction[] prefix,
        ContinuationStamp frozenBefore, ContinuationStamp liveBefore)
    {
        if (prefix.Length != 6 || prefix.Last().Kind != PlanActionKind.EndTurn)
            throw new InvalidOperationException("O003 boundary requires its six strictly verified native actions.");
        SearchPolicySnapshot policy = context.Policy with { NoveltySearch = null };
        SolverSearchProfile profile = policy.Profile with
        {
            SoftTimeBudgetMilliseconds = Math.Min(5_000, policy.Profile.SoftTimeBudgetMilliseconds / 2),
            MaxExpandedNodes = Math.Min(30_000, policy.Profile.MaxExpandedNodes / 2),
            AggressivePowerCommitment = false,
        };
        foreach (bool reset in new[] { false, true })
        {
            EnsureWithinDeadline();
            SetStage($"q002_o003_boundary_reset_{reset}");
            SolverResult result = await Task.Run(() => new CombatBeamSolver(
                context.Root, context.Names, context.Damage, policy,
                searchProfile: profile, potionPolicyOverride: SolverPotionPolicy.Disabled,
                maximumPotionUses: 0, fixedPrefixActions: prefix,
                resetFixedPrefixSchedulingBaseline: reset).Solve());
            _writer.WriteGeneratedArtifact($"O003-boundary-reset-{reset}.json", new
            {
                reset, profile, prefix,
                root = frozenBefore.StateText,
                boundary = liveBefore.StateText,
                won = result.Snapshot.AllEnemiesDead && !result.Snapshot.PlayerDead,
                result.ProjectedBattleHpLost, result.CombatEndedTurn, result.ExplicitPotionCount,
                elapsedMilliseconds = result.Elapsed.TotalMilliseconds,
                result.ExpandedNodes, result.TransitionCount,
                result.BoundaryReason, actions = result.BestNode.Actions,
            });
            if (ContinuationStamp.CaptureLive(live) != liveBefore)
                throw new InvalidOperationException("O003 boundary member changed live combat.");
            CombatBeamSolver verifier = new(context.Root, context.Names, context.Damage, policy);
            SimulationSnapshot frozen = verifier.ReplayDiagnosticPrefix([]);
            try
            {
                if (verifier.CaptureDiagnosticContinuation(frozen) != frozenBefore)
                    throw new InvalidOperationException("O003 boundary member changed its frozen root.");
            }
            finally { frozen.ReleaseSimulator(); }
        }
        _completedChecks.Add("Q002O003:BoundaryMembers:SameNativePrefix:HistoryResetControl:FrozenLiveUnchanged");
    }
}
