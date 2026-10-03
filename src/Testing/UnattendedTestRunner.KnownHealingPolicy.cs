using CombatSolver.Engine.Common;
using CombatSolver.Engine.InCombat.Simulation;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    private async Task AssertKnownHealingPolicyAsync(CombatState live, Player player)
    {
        foreach (var relic in player.Relics.ToArray()) await RelicCmd.Remove(relic);
        foreach (var power in live.Creatures.SelectMany(creature => creature.Powers).ToArray())
            await PowerCmd.Remove(power);
        await ClearPlayerPilesAsync(player);
        await InjectCardAsync(live, player, new() { CardId = "ALCHEMIZE", Pile = "Hand" });
        await InjectCardAsync(live, player, new() { CardId = "DEFEND_SILENT", Pile = "Hand" });
        CombatRootSnapshot root = CombatRootSnapshot.Capture(live);
        if (!root.UsesKnownNativeHealingPolicy || root.CanCertifyRemainingHealing
            || root.HasOnlyPostCombatHealing)
            throw new InvalidOperationException("Known-source policy must cover an Alchemize root beyond the closed certificates.");
        CombatPredictionSimulator parent = root.ForkSimulator();
        string parentBefore = DescribeContinuationContractState(parent, root, player);
        string liveBefore = ContinuationStamp.CaptureLive(live).StateText;
        int Potential(CombatPredictionSimulator sim, int? cap = null, bool potions = true)
            => StrategicHpRecoveryBound.KnownNativeHealingPotential(sim, player, 6, potions, cap);
        using (SimulationNotificationIsolation.Enter())
        {
            if (Potential(parent) != 6)
                throw new InvalidOperationException("Random Alchemize output received speculative healing allowance.");
            CombatPredictionSimulator regen = parent.Fork();
            ((SimulatedCombatState)regen.State.CombatState).SetAmount<RegenPower>(player.Creature, 3);
            if (Potential(regen, 0) != 12 || Potential(regen.Fork(), 0) != 12)
                throw new InvalidOperationException("Active regeneration or fixed postcombat healing was lost at the potion cap.");
            CombatPredictionSimulator notYet = parent.Fork();
            notYet.AddGeneratedCardToCombat(PredictedCard.Create(ModelDb.Card<NotYet>(), player),
                PileType.Hand, player, resultKind: CardGenerationResultKind.Fixed);
            if (Potential(notYet) != int.MaxValue)
                throw new InvalidOperationException("Known Not Yet healing must survive pruning, including repeated plays.");
            CombatPredictionSimulator feed = parent.Fork();
            feed.AddGeneratedCardToCombat(PredictedCard.Create(ModelDb.Card<Feed>(), player),
                PileType.Hand, player, resultKind: CardGenerationResultKind.Fixed);
            if (Potential(feed) != int.MaxValue)
                throw new InvalidOperationException("Feed must remain under the growth policy.");
            if (DescribeContinuationContractState(parent, root, player) != parentBefore
                || ContinuationStamp.CaptureLive(live).StateText != liveBefore)
                throw new InvalidOperationException("Healing policy changed its parent, live state or RNG.");
        }
        InjectPotionForTest(player, "REGEN_POTION");
        InjectPotionForTest(player, "BLOOD_POTION");
        CombatRootSnapshot potionRoot = CombatRootSnapshot.Capture(live);
        CombatPredictionSimulator doses = potionRoot.ForkSimulator();
        int blood = (int)Math.Ceiling(player.Creature.MaxHp * 0.2m);
        using (SimulationNotificationIsolation.Enter())
        {
            if (Potential(doses) != 6 + 15 + blood || Potential(doses, 0) != 6
                || Potential(doses, potions: false) != 6)
                throw new InvalidOperationException("Held regeneration/blood potion allowance or disabled-use policy is wrong.");
            ((SimulatedCombatState)doses.State.CombatState).SetAmount<RegenPower>(player.Creature, 3);
            if (Potential(doses) != 6 + 36 + blood || Potential(doses, 0) != 12)
                throw new InvalidOperationException("Regeneration doses must stack before bounding their remaining ticks.");
            if (Potential(parent) != 6)
                throw new InvalidOperationException("New live potions changed an already frozen root.");
        }
        player.AddRelicInternal(ModelDb.Relic<BookRepairKnife>().ToMutable());
        CombatRootSnapshot doomHealRoot = CombatRootSnapshot.Capture(live);
        using (SimulationNotificationIsolation.Enter())
        {
            if (!doomHealRoot.UsesKnownNativeHealingPolicy
                || Potential(doomHealRoot.ForkSimulator()) != int.MaxValue
                || Potential(parent) != 6)
                throw new InvalidOperationException("Known Doom-kill relic healing must retain its allowance without changing the old root.");
        }
        _completedChecks.Add("KnownHealingPolicy:AlchemizeExcluded:NotYetAndFeedProtected:HeldBloodAndRegen:StackedTicks:PotionCap:PostcombatHeal:DoomKillRelicProtected:FrozenRoot:ForkLiveRngIsolation");
    }
}
