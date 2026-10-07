using System.Diagnostics;
using MegaCrit.Sts2.Core.Rooms;

namespace CombatSolver;

internal static partial class CombatSearchCoordinator
{
    private static SolverResult RunBossTempoSearch(SearchPassContext context, SolverResult selected)
    {
        SearchPolicySnapshot policy = context.Policy;
        if (!policy.UseBossTempoSearch || context.Root.EncounterRoomType != RoomType.Boss
            || policy.IncludeTurnSetup || selected.ResultScope != SolverResultScope.SearchCompletion
            || policy.Interaction?.CurrentTakeoverRequest != null
            || CanFinishTargetPortfolio(context.Root, policy, context.Profile, selected))
            return selected;

        SolverSearchProfile allowance = BossTempoSearchOptions.AdditionalBudget(context.Profile,
            policy.BudgetOverrideMilliseconds ?? context.Profile.SoftTimeBudgetMilliseconds);
        Stopwatch clock = Stopwatch.StartNew();
        SearchRequestWorkSnapshot before = context.Budget.WorkTotals.Snapshot();
        int iterations = 0;
        bool improved = false;
        string stop = "time_limit";
        policy.Diagnostics.Info($"[CombatSolver/Test] BOSS_TEMPO_START "
            + $"nodes={allowance.MaxExpandedNodes} time_ms={allowance.SoftTimeBudgetMilliseconds}");
        while (clock.ElapsedMilliseconds < allowance.SoftTimeBudgetMilliseconds)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            SearchRequestWorkSnapshot work = context.Budget.WorkTotals.Snapshot();
            long remainingNodes = allowance.MaxExpandedNodes - (work.ExpandedNodes - before.ExpandedNodes);
            if (remainingNodes <= 0) { stop = "node_limit"; break; }
            if (policy.Interaction?.CurrentTakeoverRequest != null) { stop = "adoption"; break; }
            SolverSearchProfile member = allowance with
            {
                MaxExpandedNodes = (int)remainingNodes,
                SoftTimeBudgetMilliseconds = (int)(allowance.SoftTimeBudgetMilliseconds - clock.ElapsedMilliseconds),
                BaseScoreOnly = false,
                SecondRankBand = false,
                ContextualRanking = null,
                BeamWeightPerturbation = null,
                ContinuousThreatRanking = false,
                BaseScoreTacticalTies = false,
                BossTempoHpPricing = policy.BossTempoNormalizeHpPricing,
            };
            SearchPolicySnapshot memberPolicy = policy with
            {
                BossTempoSearch = new(iterations),
                NoveltySearch = null,
                MemoryNoProgressRecoveryLimit = policy.MemoryNoProgressRecoveryLimit > 0
                    ? policy.MemoryNoProgressRecoveryLimit : 2,
            };
            // The scalar witness is valid only in its potion-free primary bucket.
            PrimarySearchIncumbent? bound = IsReusablePotionFreeVictory(policy, null, selected)
                ? BuildPrimarySearchIncumbent(context.Root, policy, selected) : null;
            SolverResult? candidate = SolveOptionalPotionPosterior(new CombatBeamSolver(
                context.Root, context.DisplayNames, context.BattleDamage, memberPolicy,
                context.CancellationToken, context.ProgressCallback, member,
                primaryIncumbent: bound, directSearchPurpose: DirectSearchPurpose.BossTempo),
                policy, "boss_tempo");
            iterations++;
            if (candidate is { ResultScope: not SolverResultScope.SearchCompletion })
                return candidate;
            // Shared outcome axes compare members whose intermediate annotation pools differ.
            if (candidate != null && !candidate.Snapshot.HasRisk && IsCompleteVictory(candidate)
                && IsBetterPotionPolicyResult(policy.TheftPolicy,
                    CapturePortfolioQuality(context.Root, policy, candidate) with { Score = 0 },
                    CapturePortfolioQuality(context.Root, policy, selected) with { Score = 0 }))
            {
                selected = candidate;
                improved = true;
                context.InterimResultCallback?.Invoke(selected);
            }
            if (CanFinishTargetPortfolio(context.Root, policy, context.Profile, selected))
            { stop = "hp_target"; break; }
            if (candidate?.BoundaryReason == SearchBoundaryReason.MemoryNoProgress)
            { stop = "memory_no_progress"; break; }
            if (candidate?.BossTempoIteration is { Stop: "iteration_exhausted", Deferred: 0 })
            { stop = "admitted_frontier_exhausted"; break; }
        }
        SearchRequestWorkSnapshot after = context.Budget.WorkTotals.Snapshot();
        selected.BossTempoSearch = new(stop, iterations,
            after.ExpandedNodes - before.ExpandedNodes, after.TransitionCount - before.TransitionCount,
            after.ChoiceBranchesEvaluated - before.ChoiceBranchesEvaluated, clock.ElapsedMilliseconds, improved);
        policy.Diagnostics.Info("[CombatSolver/Test] BOSS_TEMPO_END "
            + System.Text.Json.JsonSerializer.Serialize(selected.BossTempoSearch));
        return selected;
    }
}
