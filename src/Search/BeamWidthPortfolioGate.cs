namespace CombatSolver;

/// <summary>
/// 基线成员跑完之后的实测事实；精炼成员要不要启动全部由它推导。字段都是调用方按自己
/// 既有口径给出的实测量，门控本身不看搜索内部状态。
/// </summary>
/// <param name="FrontierExhausted">
/// 基线没有被任何上限截断（生产口径：<c>SearchBoundaryReason.None</c>）。
/// </param>
/// <param name="ProvenZeroDamage">基线已经拿到零战损、零用药、满血的完整胜利，没有可精炼的余地。</param>
/// <param name="ElapsedMilliseconds">基线自己的墙钟。</param>
/// <param name="ExpandedNodes">基线实际展开数。</param>
/// <param name="AllocatedBytes">基线期间的托管分配增量。</param>
/// <param name="BeamWidth">基线 Beam 宽度，作为按宽度线性外推的分母。</param>
/// <param name="AllocatedBytesFromOrigin">
/// 基线结束时内存压力信号自其自身起点累计的分配量。信号起点可能晚于基线起点（区域中途重建时基线跨在
/// 两侧），此时 <c>AllocatedBytes</c> 里有一段从没记进当前上限；用它当外推基数会让成本侧与余量侧落在
/// 不同起点上。0 表示调用方未提供，退化为只按 <c>AllocatedBytes</c> 的旧口径。
/// </param>
internal readonly record struct BeamWidthPortfolioBaseline(
    bool FrontierExhausted,
    bool ProvenZeroDamage,
    long ElapsedMilliseconds,
    long ExpandedNodes,
    long AllocatedBytes,
    int BeamWidth,
    long AllocatedBytesFromOrigin = 0);

/// <summary>
/// 精炼成员的门控。规则出自「性能不变」的四条定义：基线早早耗尽才精炼、精炼不得越过已配置的
/// 时间预算、共享节点预算要还够一轮、内存余量不足就不精炼。
/// </summary>
/// <remarks>
/// 这里只做算术与比较，没有任何搜索状态，因此可以被 <c>tools/testing/checks/BeamWidthPortfolioChecks</c> 原样编译检查。
/// 门控只决定「跑不跑」，跑起来之后的比较仍是 <c>BeamWidthPortfolio</c> 里既有的那一条规则。
/// </remarks>
internal static class BeamWidthPortfolioGate
{
    /// <summary>基线被上限截断：还没把这一宽度搜干净，先把预算留给它自己。</summary>
    internal const string SkippedBaselineNotFrontierExhausted = "BaselineNotFrontierExhausted";

    /// <summary>基线已是证明最优，精炼不可能更好。</summary>
    internal const string SkippedBaselineProvenZeroDamage = "BaselineProvenZeroDamage";

    /// <summary>基线本身就吃掉了超过 1/4 的时间预算，精炼会把总耗时推过预算。</summary>
    internal const string SkippedBaselineTimeShareExceeded = "BaselineTimeShareExceeded";

    /// <summary>共享节点预算的余量已经不够再跑一轮基线体量的搜索。</summary>
    internal const string SkippedNodeHeadroom = "NodeHeadroomInsufficient";

    /// <summary>按宽度外推的估算耗时超过剩余时间预算。</summary>
    internal const string SkippedTimeHeadroom = "TimeHeadroomInsufficient";

    /// <summary>现有内存压力信号报告的余量装不下按宽度外推的估算分配。</summary>
    internal const string SkippedMemoryHeadroom = "MemoryHeadroomInsufficient";

    /// <summary>基线耗时允许占用的时间预算份额的倒数：1/4。</summary>
    internal const int BaselineTimeShareDivisor = 4;

    /// <summary>估算保险系数 3/2。</summary>
    internal const int SafetyNumerator = 3;

    /// <summary>估算保险系数的分母。</summary>
    internal const int SafetyDenominator = 2;

    /// <summary>
    /// 内存轴的成本基数：调用方给了同一起点的量就取两者较小值（基线跨信号起点时，基线自身增量里有一段
    /// 从没记进当前上限，不能当作已被上限约束的量），否则退回基线自身增量。时间轴不走这里，语义不变。
    /// </summary>
    internal static long BaselineMemoryCostBase(in BeamWidthPortfolioBaseline baseline)
        => baseline.AllocatedBytesFromOrigin > 0
            ? Math.Min(baseline.AllocatedBytes, baseline.AllocatedBytesFromOrigin)
            : baseline.AllocatedBytes;

    /// <summary>
    /// 按节点配额投影成本 = 基线实测 × 成员配额 / 基线节点数 × 3/2，向上取整。有界精炼成员的配额是
    /// totalExpanded / 8，远小于其宽度对应的量，按宽度外推会高估，故这一类用配额而非宽度。
    /// </summary>
    internal static long EstimateQuotaCost(long baselineCost, long baselineNodes, long memberNodes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baselineCost);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baselineNodes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memberNodes);
        long numerator = baselineCost * memberNodes * SafetyNumerator;
        long denominator = baselineNodes * SafetyDenominator;
        if (numerator / Math.Max(1, denominator) >= long.MaxValue / 2)
            return long.MaxValue;
        return (numerator + denominator - 1) / denominator;
    }

    /// <summary>
    /// 估算 = 基线实测 × 成员宽度 / 基线宽度 × 3/2，向上取整。乘法先做，免得整数除法先把比例抹平。
    /// </summary>
    internal static long EstimateMemberCost(long baselineCost, int baselineBeamWidth, int memberBeamWidth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baselineCost);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baselineBeamWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memberBeamWidth);
        long numerator = baselineCost * memberBeamWidth * SafetyNumerator;
        long denominator = (long)baselineBeamWidth * SafetyDenominator;
        return (numerator + denominator - 1) / denominator;
    }

    /// <summary>
    /// 精炼成员的准入判断。返回非空即为不运行的原因（原样进成员明细），返回 null 才启动。
    /// 基线成员永远不经过这里。
    /// </summary>
    /// <param name="baseline">基线成员的实测事实。</param>
    /// <param name="memberBeamWidth">待启动成员的 Beam 宽度。</param>
    /// <param name="remainingNodes">共享节点预算的余量。</param>
    /// <param name="remainingMilliseconds">时间预算的余量。</param>
    /// <param name="timeBudgetMilliseconds">本轮已配置的时间预算。</param>
    /// <param name="remainingMemoryBytes">
    /// 现有 <c>SearchMemoryPressureSignal.RemainingBytes</c>；信号没配置时是 <see cref="long.MaxValue" />，
    /// 此时不按内存拦截。
    /// </param>
    /// <param name="memberMemoryCostBytes">
    /// 调用方换算好的该成员内存成本；提供时内存轴用它，不再按基线宽度外推（有界精炼成员按实际节点配额）。
    /// 为 null 时保持旧口径，时间轴与其余五条判据一律不变。
    /// </param>
    internal static string? RejectRefinement(
        in BeamWidthPortfolioBaseline baseline,
        int memberBeamWidth,
        long remainingNodes,
        long remainingMilliseconds,
        long timeBudgetMilliseconds,
        long remainingMemoryBytes,
        long? memberMemoryCostBytes = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memberBeamWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baseline.BeamWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeBudgetMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegative(remainingMemoryBytes);
        if (baseline.ProvenZeroDamage)
            return SkippedBaselineProvenZeroDamage;
        if (!baseline.FrontierExhausted)
            return SkippedBaselineNotFrontierExhausted;
        if (baseline.ElapsedMilliseconds * BaselineTimeShareDivisor > timeBudgetMilliseconds)
            return SkippedBaselineTimeShareExceeded;
        if (remainingNodes < baseline.ExpandedNodes)
            return SkippedNodeHeadroom;
        if (EstimateMemberCost(baseline.ElapsedMilliseconds, baseline.BeamWidth, memberBeamWidth)
            > remainingMilliseconds)
        {
            return SkippedTimeHeadroom;
        }
        if (remainingMemoryBytes != long.MaxValue)
        {
            long memoryCost = memberMemoryCostBytes ?? EstimateMemberCost(
                BaselineMemoryCostBase(baseline), baseline.BeamWidth, memberBeamWidth);
            ArgumentOutOfRangeException.ThrowIfNegative(memoryCost);
            if (memoryCost > remainingMemoryBytes)
                return SkippedMemoryHeadroom;
        }
        return null;
    }
}
