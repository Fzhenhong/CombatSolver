using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using CombatSolver.Engine.Common;
using CombatSolver.Engine.InCombat.Simulation;

namespace CombatSolver;

internal sealed partial class UnattendedTestRunner
{
    /// <summary>
    /// B016/T024：永久的跑局牌组身份（<see cref="CardModel.DeckVersion"/>）只属于「牌组原牌在战斗里的克隆」。
    /// 预测侧任何把既有牌当成生成牌或玩法复制处理的路径都会让跨回合续接戳丢掉这份身份，
    /// 实机边界比较随即将该牌报成单字段差异（SEARCH_REUSE_MISS）。
    /// 本夹具把身份合同钉在同一张实机原牌上，按真实牌堆迁移逐段核对：
    /// 根捕获、写时复制、洗牌回抽牌堆、回手、玩法复制、以及同名牌转换替换。
    /// </summary>
    private async Task AssertCardCloneIdentityContractAsync(CombatState combat, Player player)
    {
        await Task.Yield();
        PlayerCombatState live = player.PlayerCombatState
            ?? throw new InvalidOperationException("卡牌克隆身份测试要求玩家战斗状态。");

        CardPile[] piles = [live.Hand, live.DrawPile, live.DiscardPile, live.ExhaustPile];
        CardModel[] allCards = [.. piles.SelectMany(pile => pile.Cards)];
        // 现场的单字段差异来自「同 ID 的永久牌组原牌 + 战斗生成副本」这一对实例，
        // 所以优先挑同时存在这两种实例的牌；没有副本时退回任意带身份的牌。
        CardModel deckCard = allCards
            .Where(card => card.DeckVersion != null)
            .OrderByDescending(card => allCards.Count(
                other => other.Id.Entry == card.Id.Entry && other.DeckVersion == null))
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "卡牌克隆身份测试要求牌堆里至少有一张带永久牌组版本的实机原牌。");

        CardModel deckVersion = deckCard.DeckVersion
            ?? throw new InvalidOperationException("卡牌克隆身份测试找不到永久牌组版本。");
        string cardId = deckCard.Id.Entry;

        var simulator = new CombatPredictionSimulator(new SimulatedCombatState(combat));
        SimPlayerCombatState simState = simulator.State.GetPlayerCombatState(player);
        PredictedCard wrapper = simState.FindCard(deckCard)
            ?? throw new InvalidOperationException($"预测根找不到实机原牌 {cardId}，既有牌被当成新牌处理。");

        RequireDeckVersion(wrapper, deckVersion, cardId, "根捕获");

        if (wrapper.Preview.HasBeenRemovedFromState)
            throw new InvalidOperationException($"{cardId} 的预测根包装错误地带上了移出战斗标记。");

        wrapper.MaterializePreview();
        RequireDeckVersion(wrapper, deckVersion, cardId, "材质化预览");

        _ = wrapper.MutablePreview;
        RequireDeckVersion(wrapper, deckVersion, cardId, "写时复制");

        simulator.Shuffle(player);
        PredictedCard shuffled = simState.FindCard(deckCard)
            ?? throw new InvalidOperationException($"洗牌回抽牌堆后预测根丢失了实机原牌 {cardId}。");
        if (!ReferenceEquals(shuffled, wrapper))
            throw new InvalidOperationException($"洗牌为 {cardId} 重建了预测包装，既有牌被当成新牌处理。");
        RequireDeckVersion(shuffled, deckVersion, cardId, "洗牌回抽牌堆");

        simulator.AddToPile(wrapper, PileType.Hand);
        RequireDeckVersion(wrapper, deckVersion, cardId, "回手");

        // 跨回合循环：手牌弃空、抽牌堆空则把弃牌堆洗回抽牌堆、再抽牌。
        List<PredictedCard> handCards = [.. simState.Hand.Cards];
        if (handCards.Count > 0)
            simulator.Discard(handCards);
        RequireDeckVersion(wrapper, deckVersion, cardId, "弃牌堆");
        simulator.Draw(player, 5);
        RequireDeckVersion(wrapper, deckVersion, cardId, "洗牌回抽牌堆后抽牌");

        // 玩法复制是唯一按原版清空身份的分支；它也必须真的清空，并记下实机原牌。
        PredictedCard gameplayCopy = wrapper.CreateClone();
        if (gameplayCopy.Preview.DeckVersion != null)
            throw new InvalidOperationException($"{cardId} 的玩法复制错误地保留了永久牌组身份。");
        if (!ReferenceEquals(gameplayCopy.Preview.CloneOf, deckCard))
            throw new InvalidOperationException($"{cardId} 的玩法复制没有记下它的实机原牌。");

        // 同名牌转换替换：替换牌按原版是全新生成牌，实机原牌对象本身必须原封不动。
        CardModel canonical = ModelDb.AllCards.First(card => card.Id.Entry == cardId);
        CardModel replacement = PredictionUtils.CreateCard(canonical, player);
        CardChoiceSupport.TransformCardToGeneratedReplacement(simulator, wrapper, replacement);
        if (!ReferenceEquals(deckCard.DeckVersion, deckVersion))
            throw new InvalidOperationException($"预测侧转换改写了实机原牌 {cardId} 的永久牌组关联。");
        if (simState.FindCard(deckCard) != null)
            throw new InvalidOperationException($"{cardId} 转换后预测根仍能找到已被替换的原牌包装。");

        _completedChecks.Add("CardCloneIdentity:DeckVersionContractSurvivesPileTransfers");
    }

    private static void RequireDeckVersion(
        PredictedCard card,
        CardModel expected,
        string cardId,
        string stage)
    {
        if (ReferenceEquals(card.Preview.DeckVersion, expected))
            return;
        string actual = card.Preview.DeckVersion is { } version
            ? version.Id.Entry + "(" + version.GetHashCode() + ")"
            : "null";
        throw new InvalidOperationException(
            $"{cardId} 在「{stage}」丢失了永久牌组身份："
            + $"expected={expected.Id.Entry}({expected.GetHashCode()}) actual={actual}。");
    }
}
