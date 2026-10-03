using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using CombatSolver.Engine.InCombat.Simulation;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    // Reproduces the block settlement boundary behind the T010 report: Shadowmeld doubles
    // block once per stack (2^Amount), so a long 0-cost Shadowmeld loop with Unceasing Top
    // pushes the factor and then the product past decimal range. The shadow simulator must
    // keep normal-range products identical to vanilla and settle out-of-range products at
    // the block ceiling instead of throwing.
    private async Task AssertB013BlockDecimalBoundaryAsync(CombatState combat, Player player)
    {
        foreach (RelicModel relic in player.Relics.ToArray()) await RelicCmd.Remove(relic);
        foreach (PowerModel power in combat.Creatures.SelectMany(creature => creature.Powers).ToArray())
            await PowerCmd.Remove(power);
        await ClearPlayerPilesAsync(player);
        await SetBlockAsync(player.Creature, 0);

        var simulator = CombatRootSnapshot.Capture(combat).ForkSimulator();
        var shadow = (SimulatedCombatState)simulator.State.CombatState;

        // Normal range keeps the native product: 6 block x 2^3 stacks = 48.
        shadow.Apply<ShadowmeldPower>(player.Creature, 3, player.Creature);
        decimal normal = simulator.GainBlock(player.Creature, 6m, ValueProp.Unpowered);
        AssertBlockBoundary(normal, 48m, simulator.State.GetCreature(player.Creature).Block, 48, "normal");
        shadow.SetAmount<ShadowmeldPower>(player.Creature, 0);

        // 2^95 leaves decimal range at the product (the reported VarDecMul failure).
        shadow.Apply<ShadowmeldPower>(player.Creature, 95, player.Creature);
        decimal saturated = simulator.GainBlock(player.Creature, 6m, ValueProp.Unpowered);
        AssertBlockBoundary(saturated, SimCreatureState.BlockSettlementCeiling,
            simulator.State.GetCreature(player.Creature).Block,
            (int)SimCreatureState.BlockSettlementCeiling, "saturated_product");
        shadow.SetAmount<ShadowmeldPower>(player.Creature, 0);

        // Stacks beyond the factor's own decimal range must not throw inside the factor.
        shadow.Apply<ShadowmeldPower>(player.Creature, 200, player.Creature);
        decimal beyondFactorRange = simulator.GainBlock(player.Creature, 6m, ValueProp.Unpowered);
        AssertBlockBoundary(beyondFactorRange, SimCreatureState.BlockSettlementCeiling,
            simulator.State.GetCreature(player.Creature).Block,
            (int)SimCreatureState.BlockSettlementCeiling, "beyond_factor_range");

        _completedChecks.Add(
            "ShadowmeldBlockDecimalBoundary:Normal=48:Saturated=999999999:BeyondFactorRange=999999999");
    }

    private static void AssertBlockBoundary(
        decimal modified,
        decimal expectedModified,
        int settled,
        int expectedSettled,
        string stage)
    {
        if (modified != expectedModified || settled != expectedSettled)
        {
            throw new InvalidOperationException(
                $"Shadowmeld block boundary {stage}: modified={modified} settled={settled}, " +
                $"expected modified={expectedModified} settled={expectedSettled}.");
        }
    }
}
