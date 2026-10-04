using CombatSolver;
using CombatSolver.Engine.InCombat.Simulation;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace OfflineSearchHarness;

internal static class PrimaryIncumbentChecks
{
    internal static void RunTheft(CombatState combat)
    {
        int assertions = 0;
        var root = CombatRootSnapshot.Capture(combat);
        var thief = combat.Enemies.First();
        var held = root.ForkSimulator();
        var state = (SimulatedCombatState)held.State.CombatState;
        state.RecordStolenCard(held);
        state.Apply<SwipePower>(thief, 1, thief);
        held.AddToCombat<StrikeIronclad>(root.PlayerIdentity.Creature, PileType.Discard, 1, creator: null);
        state.GetMutablePower<SwipePower>(thief)!.StolenCard =
            held.State.GetPlayerCombatState(root.PlayerIdentity).DiscardPile.Cards.Last().Preview;
        PowerLifecycleSupport.ResolvePowerAmountChanges(held, state);
        Check(state.OutstandingStolenResource(held) == 1, "held card counts as current theft");
        Check(state.MinimumOutstandingStolenResource(held) == 0, "living thief can return the card");
        var escaped = held.Fork();
        var escapedState = (SimulatedCombatState)escaped.State.CombatState;
        escapedState.CreatureEscaped(thief);
        Check(escapedState.OutstandingStolenResource(escaped) == 1, "escaped card remains lost");
        Check(escapedState.MinimumOutstandingStolenResource(escaped) == 1, "escape closes the loss floor");
        Check(state.MinimumOutstandingStolenResource(held) == 0, "escaped sibling cannot change parent");
        var recovered = held.Fork();
        var recoveredState = (SimulatedCombatState)recovered.State.CombatState;
        recoveredState.RecoverStolenResources(recovered, thief);
        Check(recoveredState.OutstandingStolenResource(recovered) == 0, "recovery closes the zero-loss bucket");
        Check(state.OutstandingStolenResource(held) == 1, "recovered sibling cannot change parent");
        PrimaryIncumbentTable table = new();
        table.Tighten(1, 0, new(0, 3));
        Check(!table.TryGet(state.MinimumOutstandingStolenResource(held), 0, out _),
            "lost-card witness cannot prune a potentially saved-card route");
        Check(table.TryGet(escapedState.MinimumOutstandingStolenResource(escaped), 0, out _),
            "lost-card witness can bound the escaped-card bucket");
        var healing = held.Fork();
        healing.AddToCombat<NotYet>(root.PlayerIdentity.Creature, PileType.Discard, 1, creator: null);
        var healCard = healing.State.GetPlayerCombatState(root.PlayerIdentity).DiscardPile.Cards.Last();
        healing.RemoveFromCombat(healCard);
        var healingState = (SimulatedCombatState)healing.State.CombatState;
        healingState.GetMutablePower<SwipePower>(thief)!.StolenCard = healCard.Preview;
        Check(StrategicHpRecoveryBound.KnownNativeHealingPotential(healing, root.PlayerIdentity, 6)
            == int.MaxValue, "stolen healing card reserves future healing after recovery");
        Console.WriteLine($"THEFT_BUCKET_CHECKS status=Passed assertions={assertions}");

        void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            assertions++;
        }
    }

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
        Require(CombatBeamSolver.ShouldPruneByPrimaryIncumbent(8, 1, new(8, 9),
            pruneEqualHp: true), "equal loss truncates even an earlier unfinished route");
        Require(!CombatBeamSolver.ShouldPruneByPrimaryIncumbent(7, 20, new(8, 9),
            pruneEqualHp: true), "a better optimistic loss bound survives");
        Require(CombatBeamSolver.ShouldPruneByPrimaryIncumbent(0, 1, new(0, 3),
            pruneEqualHp: true), "zero-loss victory immediately bounds equal-loss continuation");
        Require(!CombatBeamSolver.ShouldPruneByPrimaryIncumbent(-1, 1, new(0, 3),
            pruneEqualHp: true), "potential recovery below incumbent remains searchable");
        Console.WriteLine($"PRIMARY_INCUMBENT_CHECKS status=Passed assertions={assertions}");
        return 0;

        void Require(bool success, string message)
        {
            if (!success) throw new InvalidOperationException(message);
            assertions++;
        }
    }
}
