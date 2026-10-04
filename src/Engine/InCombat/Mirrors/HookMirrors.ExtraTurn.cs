using CombatSolver.Engine.Common;
using CombatSolver.Engine.InCombat.Mirrors.Hooks.TurnEnd;
using CombatSolver.Engine.InCombat.Simulation;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace CombatSolver.Engine.InCombat.Mirrors;

internal static partial class HookMirrors
{
    /// <summary>
    /// 对应 <c>Hook.ShouldTakeExtraTurn</c>：原版来源照旧由 <see cref="SimulatedCombatState.ShouldTakeExtraPlayerTurn" />
    /// 判断，再问登记过的第三方监听者。
    /// </summary>
    /// <remarks>
    /// 原生钩子是「任意一个监听者返回 true 就给」，判断本身没有副作用，所以先问原版、再问第三方，
    /// 结果和原生遍历顺序无关。
    /// </remarks>
    public static bool ShouldTakeExtraTurn(
        CombatPredictionSimulator simulator, SimulatedCombatState combat, Player player)
    {
        ExtraTurnMirrors.Seal();
        if (combat.ShouldTakeExtraPlayerTurn(player))
            return true;

        ExtraTurnMirrorContext? context = null;
        foreach (AbstractModel listener in IterateCombatHookListeners(simulator, MirroredHookMask.ExtraTurnCallbacks))
        {
            if (IsBaseGameListener(listener))
                continue;
            context ??= new ExtraTurnMirrorContext { Simulator = simulator, Player = player };
            if (ExtraTurnMirrors.ShouldTakeExtraTurn(listener, context))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 对应 <c>Hook.AfterTakingExtraTurn</c>：原版来源照旧由 <see cref="SimulatedCombatState.ConsumeExtraTurnSources" />
    /// 结算，再通知登记过的第三方监听者。
    /// </summary>
    /// <remarks>
    /// 原生钩子按监听者顺序逐个通知。这里先结算原版的龙涎香与帕尔之眼、再通知第三方；两边只改各自的状态，
    /// 互不读取。第三方成员在开头固定下来，回调里移除自己不会影响本轮其他成员。
    /// </remarks>
    public static void AfterTakingExtraTurn(
        CombatPredictionSimulator simulator, SimulatedCombatState combat, Player player)
    {
        ExtraTurnMirrors.Seal();
        combat.ConsumeExtraTurnSources(player);

        List<AbstractModel>? receivers = null;
        foreach (AbstractModel listener in IterateCombatHookListeners(simulator, MirroredHookMask.ExtraTurnCallbacks))
        {
            if (!IsBaseGameListener(listener))
                (receivers ??= []).Add(listener);
        }
        if (receivers is null)
            return;

        var context = new ExtraTurnMirrorContext { Simulator = simulator, Player = player };
        foreach (AbstractModel receiver in receivers)
            ExtraTurnMirrors.AfterTakingExtraTurn(receiver, context);
    }

    private static bool IsBaseGameListener(AbstractModel listener)
        => listener.GetType().Assembly == typeof(AbstractModel).Assembly;
}
