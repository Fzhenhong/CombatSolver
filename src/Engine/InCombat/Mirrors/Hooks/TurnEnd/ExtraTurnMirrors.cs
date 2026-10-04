using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using CombatSolver.Engine.Common;
using CombatSolver.Engine.Common.Mirrors;
using CombatSolver.Engine.InCombat.Simulation;

namespace CombatSolver.Engine.InCombat.Mirrors.Hooks.TurnEnd;

using ShouldRegistry = MethodMirrorRegistry<AbstractModel, ExtraTurnMirrorContext, bool>;
using AfterRegistry = MethodMirrorRegistry<AbstractModel, ExtraTurnMirrorContext>;

/// <summary>
/// 额外回合的第三方来源：对应 <c>Hook.ShouldTakeExtraTurn</c> 与 <c>Hook.AfterTakingExtraTurn</c>。
/// </summary>
/// <remarks>
/// <para>
/// 原版只有龙涎香和帕尔之眼重写这两个钩子，它们仍由 <see cref="SimulatedCombatState.ShouldTakeExtraPlayerTurn" />
/// 与 <see cref="SimulatedCombatState.ConsumeExtraTurnSources" /> 原样结算，这里不登记，原版行为逐位不变。
/// 这两张表只派发<b>第三方</b>监听者，见 <see cref="HookMirrors.ShouldTakeExtraTurn" />。
/// </para>
/// <para>
/// 以前额外回合的来源是写死的，第三方 Power 给的额外回合在模拟里看不见，也不报：打出这类牌在搜索里
/// 等于白白结束回合，实机却多出一个回合，执行和预测对不上而反复重算。登记之后按登记结算。
/// 额外回合会改变「接下来还能做什么」，只记风险不够，所以和回合阶段那几张表同一口径：重写了却
/// 没登记的，停止搜索——来源是已加载的第三方内容模型时按「暂未适配」报给玩家。只做表现的重写，
/// 登记一个返回 <c>false</c> / 什么都不做的处理即可。
/// </para>
/// <para>登记必须在第一次建根之前完成，之后的登记直接拒绝。</para>
/// </remarks>
internal static class ExtraTurnMirrors
{
    private static readonly MirrorMethodSpec ShouldTakeExtraTurnMethod = MirrorMethodSpec.Hook(
        nameof(AbstractModel.ShouldTakeExtraTurn),
        [typeof(Player)]);

    private static readonly MirrorMethodSpec AfterTakingExtraTurnMethod = MirrorMethodSpec.Hook(
        nameof(AbstractModel.AfterTakingExtraTurn),
        [typeof(Player)]);

    private static readonly ShouldRegistry ShouldRegistry = new(ShouldTakeExtraTurnMethod);
    private static readonly AfterRegistry AfterRegistry = new(AfterTakingExtraTurnMethod);
    private static readonly object RegistrationLock = new();
    private static bool _sealed;

    /// <summary>登记一个类型的 <c>ShouldTakeExtraTurn</c>：返回 <c>true</c> 表示这名玩家要再来一个回合。</summary>
    public static void RegisterShouldTakeExtraTurn<TModel>(Func<TModel, ExtraTurnMirrorContext, bool> handler)
        where TModel : AbstractModel
    {
        ArgumentNullException.ThrowIfNull(handler);
        RegisterGuarded(typeof(TModel), () => ShouldRegistry.Register(handler));
    }

    /// <summary>登记一个类型的 <c>AfterTakingExtraTurn</c>：额外回合用掉之后的结算，比如自减一层。</summary>
    public static void RegisterAfterTakingExtraTurn<TModel>(Action<TModel, ExtraTurnMirrorContext> handler)
        where TModel : AbstractModel
    {
        ArgumentNullException.ThrowIfNull(handler);
        RegisterGuarded(typeof(TModel), () => AfterRegistry.Register(handler));
    }

    /// <summary>按运行时类型登记，给不直接引用对方程序集的适配层用。</summary>
    public static void RegisterShouldTakeExtraTurn(
        Type modelType,
        Func<AbstractModel, ExtraTurnMirrorContext, bool> handler)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(handler);
        RegisterGuarded(modelType, () => ThirdPartyMirrorRegistration.RegisterResult(ShouldRegistry, modelType, handler));
    }

    /// <summary>按运行时类型登记，给不直接引用对方程序集的适配层用。</summary>
    public static void RegisterAfterTakingExtraTurn(
        Type modelType,
        Action<AbstractModel, ExtraTurnMirrorContext> handler)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(handler);
        RegisterGuarded(modelType, () => ThirdPartyMirrorRegistration.Register(AfterRegistry, modelType, handler));
    }

    internal static void Seal()
    {
        if (Volatile.Read(ref _sealed)) return;
        lock (RegistrationLock) Volatile.Write(ref _sealed, true);
    }

    /// <summary>一个第三方监听者要不要给额外回合。重写了却没登记的停止搜索。</summary>
    internal static bool ShouldTakeExtraTurn(AbstractModel listener, ExtraTurnMirrorContext context)
    {
        MirrorDispatchResult<bool> result = ShouldRegistry.Invoke(listener, context, false);
        if (result.Kind == MirrorDispatchKind.Unsupported)
            throw Unsupported(ShouldRegistry.DescribeMirrorSupport().BaseMethod.Name, listener);
        return result.Value;
    }

    /// <summary>额外回合用掉之后通知一个第三方监听者。重写了却没登记的停止搜索。</summary>
    internal static void AfterTakingExtraTurn(AbstractModel listener, ExtraTurnMirrorContext context)
    {
        if (AfterRegistry.Invoke(listener, context).Kind == MirrorDispatchKind.Unsupported)
            throw Unsupported(AfterRegistry.DescribeMirrorSupport().BaseMethod.Name, listener);
    }

    private static NotSupportedException Unsupported(string method, AbstractModel listener)
        => PredictionUnsupportedException.ForContent(
            $"No {method} mirror is registered for {listener.GetType().FullName}.",
            listener.GetType());

    private static void RegisterGuarded(Type modelType, Action register)
    {
        if (modelType.IsAbstract)
            throw new ArgumentException("Extra-turn mirrors require a concrete runtime model type.", nameof(modelType));
        if (modelType.Assembly == typeof(AbstractModel).Assembly)
        {
            throw new ArgumentException(
                $"{modelType.FullName} 是原版类型；原版额外回合来源由 SimulatedCombatState 直接结算，不走这张表。",
                nameof(modelType));
        }
        lock (RegistrationLock)
        {
            if (_sealed)
                throw new InvalidOperationException("Extra-turn mirrors must be registered before root capture or dispatch.");
            register();
        }
    }
}

internal sealed class ExtraTurnMirrorContext : CombatMirrorContext
{
    /// <summary>刚结束回合、正在判断或已经用掉额外回合的那名玩家。原版重写第一件事都是拿它和主人比对。</summary>
    public required Player Player { get; init; }

    public SimulatedCombatState Combat => CombatState as SimulatedCombatState
        ?? throw new InvalidOperationException("额外回合结算缺少分支战斗状态。");
}
