using CombatSolver.Engine.InCombat.Simulation;
using CombatSolver.Engine.Common;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CombatSolver;

internal static class ResourceIncumbentPolicy
{
    internal static bool IsPlainBucket(PrimaryIncumbentBucket bucket)
        => bucket.Growth.Total == 0 && bucket.RelicMask == 0;

    internal static PrimaryIncumbentBucket CompletedBucket(SimulationSnapshot snapshot, int potions)
        => new(snapshot.OutstandingStolenResource, potions, snapshot.GrowthRewards,
            snapshot.RelicCounters.SatisfiedMask);

    internal static PrimaryIncumbentBucket CompletedBucket(SolverSnapshot snapshot, int potions)
        => new(snapshot.OutstandingStolenResource, potions, snapshot.GrowthRewards,
            snapshot.RelicCounters.SatisfiedMask);

    // A frozen opportunity target is not by itself a proof of a global reward cap.
    // Certify a small closed loadout: no generation, exhaust recovery, copying,
    // replay powers/relics, third-party callbacks, or reward-granting potions.
    internal static GrowthValues? CaptureExhaustingGrowthUpperBound(
        CombatPredictionSimulator simulator, Player player)
    {
        var combat = (SimulatedCombatState)simulator.State.CombatState;
        if (!StrategicHpRecoveryBound.CanUseKnownNativeHealingPolicy(simulator, player)
            || GrowthSourceMirrors.All.Count != 0
            || combat.RootRunModSubscriberCount != 0 || combat.RootCombatModSubscriberCount != 0
            || combat.HasPendingReturningCards
            || combat.PotionUses.Count != 0
            || Enumerable.Range(0, ((ICombatPredictionPlayerLimits)combat).GetPotionSlotCount(player))
                .Any(slot => combat.GetPotionAtSlot(player, slot) != null)
            || combat.RelicsOf(player).Any(relic => !relic.IsMelted && relic is not
                (BurningBlood or BlackBlood or RingOfTheSnake or BoundPhylactery
                or DivineRight or HappyFlower or PenNib or Nunchaku))
            || combat.EffectivePowers().Any(power => ReferenceEquals(power.Owner, player.Creature)
                && power is not (StrengthPower or DexterityPower or WeakPower or VulnerablePower
                    or FrailPower or ThornsPower or RoyaltiesPower))
            || combat.EffectivePowers().OfType<SwipePower>().Any(power => power.StolenCard != null))
            return null;

        GrowthValues upper = combat.GrowthRewards;
        if (!upper.Extras.IsEmpty) return null;
        foreach (var card in simulator.State.GetPlayerCombatState(player).AllCards)
        {
            CardModel model = card.Preview;
            if (model.Enchantment != null || model.Affliction != null) return null;
            GrowthSource? source = model switch
            {
                Feed => GrowthSource.Feed,
                TheHunt => GrowthSource.TheHunt,
                Royalties => GrowthSource.Royalties,
                GeneticAlgorithm when model.DeckVersion != null => GrowthSource.GeneticAlgorithm,
                TheScythe when model.DeckVersion != null => GrowthSource.TheScythe,
                _ => null,
            };
            if (source is { } growth)
            {
                if (!card.HasKeyword(simulator.State, CardKeyword.Exhaust)
                    && !(model is Royalties && model.Type == CardType.Power)) return null;
                if (!simulator.State.GetPlayerCombatState(player).ExhaustPile.Cards.Contains(card))
                    upper = upper.With(growth, checked(upper.Get(growth) + 1));
            }
            else if (model is not (StrikeIronclad or DefendIronclad or Bash
                or StrikeSilent or DefendSilent or Neutralize or Survivor
                or StrikeNecrobinder or DefendNecrobinder or Bodyguard or Unleash
                or StrikeRegent or DefendRegent)
                && model.Type is not (CardType.Status or CardType.Curse))
                return null;
        }
        return upper;
    }

    internal static bool TryOptimisticBucket(SearchPolicySnapshot policy, GrowthValues? growthUpper,
        SimulationSnapshot snapshot, int stolen, int potions,
        out PrimaryIncumbentBucket bucket, out int rewardCredit)
        => TryOptimisticBucket(policy, growthUpper, snapshot.GrowthRewards, stolen, potions,
            out bucket, out rewardCredit);

    internal static bool TryOptimisticBucket(SearchPolicySnapshot policy, GrowthValues? growthUpper,
        GrowthValues realized, int stolen, int potions,
        out PrimaryIncumbentBucket bucket, out int rewardCredit)
    {
        bucket = default;
        rewardCredit = 0;
        GrowthValues optimistic = default;
        if (!policy.IgnoreLongTermRewards && (policy.EffectiveHasGrowthTargets || realized.Total != 0))
        {
            if (!policy.GrowthOpportunityTargets.IsBounded
                || !policy.GrowthOpportunityTargets.RequiredRewards.Extras.IsEmpty
                || growthUpper is not { } upper
                || !upper.Satisfies(realized)) return false;
            optimistic = upper;
        }
        ulong relicMask = 0;
        foreach (var target in policy.RelicTargets)
        {
            relicMask |= 1UL << (int)target.Id;
            if (target.Id != RelicCounterId.MeatOnTheBone)
                rewardCredit = checked(rewardCredit + target.HpAllowance);
        }
        rewardCredit = checked(rewardCredit + policy.EffectiveGrowthBudgets.Credit(optimistic));
        bucket = new(stolen, potions, optimistic, relicMask);
        return true;
    }
}
