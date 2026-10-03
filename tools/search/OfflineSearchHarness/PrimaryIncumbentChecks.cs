using CombatSolver;

namespace OfflineSearchHarness;

internal static class PrimaryIncumbentChecks
{
    internal static int Run()
    {
        int assertions = 0;
        PrimaryIncumbentTable table = new();
        Require(table.Tighten(0, 0, new(8, 4)), "first witness");
        Require(table.Tighten(0, 1, new(3, 5)), "separate potion tier");
        Require(table.Tighten(1, 0, new(2, 3)), "separate theft class");
        Require(table.TryGet(0, 0, out var zero) && zero.StrategicHpDeficit == 8,
            "positive potion and theft witnesses do not overwrite zero tier");
        Require(!table.TryGet(0, 2, out _), "unknown tier remains unknown");
        Require(!table.Tighten(0, 0, new(9, 1)), "worse HP cannot replace witness");
        Require(table.Tighten(0, 0, new(8, 3)), "earlier equal HP replaces witness");
        Require(!table.Tighten(0, 0, new(8, 5)), "later equal HP cannot replace witness");
        Require(!CombatBeamSolver.CanUseSharedPotionTier(0, 1, false,
            SolverPotionPolicy.Smart, false), "future potion tier remains open");
        Require(CombatBeamSolver.CanUseSharedPotionTier(1, 1, false,
            SolverPotionPolicy.Smart, false), "exact tier closes at maximum");
        Require(CombatBeamSolver.CanUseSharedPotionTier(0, null, true,
            SolverPotionPolicy.Smart, true), "explicit disabled override closes tier");
        Require(!CombatBeamSolver.CanUseSharedPotionTier(0, null, false,
            SolverPotionPolicy.Disabled, true), "forced directives keep tier open");
        SolverCombatSession session = new();
        var first = session.AcquirePrimaryIncumbents("root-A/policy-A");
        first.Tighten(0, 0, new(0, 1));
        var second = session.AcquirePrimaryIncumbents("root-A/policy-A");
        Require(!second.TryGet(0, 0, out _), "bound without retained executable witness is discarded");
        second.Tighten(0, 0, new(0, 1));
        var changed = session.AcquirePrimaryIncumbents("root-B/policy-A");
        Require(!changed.TryGet(0, 0, out _), "changed root cannot inherit old bound");
        Require(!CombatBeamSolver.ShouldPruneByPrimaryIncumbent(8, 3, new(8, 3)),
            "equal HP can retain earlier victory");
        Require(CombatBeamSolver.ShouldPruneByPrimaryIncumbent(9, 1, new(8, 3)),
            "strictly worse HP is pruned");
        Console.WriteLine($"PRIMARY_INCUMBENT_CHECKS status=Passed assertions={assertions}");
        return 0;

        void Require(bool success, string message)
        {
            if (!success) throw new InvalidOperationException(message);
            assertions++;
        }
    }
}
