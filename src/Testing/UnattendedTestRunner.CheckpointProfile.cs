using MegaCrit.Sts2.Core.Combat;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    private void AssertCheckpointProfileContract(CombatState combat)
    {
        if (_protocolHost.BeamWeightPerturbationOverride != null)
            throw new InvalidOperationException("Profile contract requires no separate CLI weight override.");
        SolverSettingsSnapshot settings = SolverSettings.Capture();
        SolverSearchProfile? previous = _protocolHost.CheckpointProfileOverride;
        SolverSearchProfile expected = settings.Profile with
        {
            BeamWidth = 7,
            MaxExpandedNodes = 1234,
            SoftTimeBudgetMilliseconds = 987,
            SecondRankBand = true,
            ContinuousThreatRanking = true,
            BaseScoreTacticalTies = true,
            AdaptiveNoveltyRefinement = true,
            BeamWeightPerturbation = new(BeamWeightTerm.CurrentEnergy, 0),
            OffensiveRefinementPortfolio = true,
            BoundedOffensiveRefinementPortfolio = true,
            ReallocatedRefinementPortfolio = false,
            StopPortfolioAtHpTarget = false,
            BaseScoreOnly = true,
            AggressivePowerCommitment = true,
        };
        try
        {
            _protocolHost.ApplyRecordedSearchProfile(expected);
            SearchPolicySnapshot captured = SolverController.CaptureSearchPolicy(settings, combat, false, null);
            if (captured.Profile != expected)
                throw new InvalidOperationException("Recorded profile lost fields during policy capture.");
            _protocolHost.ApplyRecordedSearchProfile(null);
            SearchPolicySnapshot ordinary = SolverController.CaptureSearchPolicy(settings, combat, false, null);
            if (ordinary.Profile != settings.Profile)
                throw new InvalidOperationException("Recorded profile leaked into the ordinary policy.");
            _completedChecks.Add("CheckpointProfile:CompleteRecord:FrozenPolicy:ClearRestoresOrdinaryProfile");
        }
        finally
        {
            _protocolHost.ApplyRecordedSearchProfile(previous);
        }
    }
}
