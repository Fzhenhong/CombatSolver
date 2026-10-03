namespace CombatSolver;

/// <summary>Witnessed victories shared only by searches with the same frozen root and policy.</summary>
internal sealed class PrimaryIncumbentTable
{
    private readonly Dictionary<(int Stolen, int Potions), PrimarySearchIncumbent> _bounds = [];
    internal SolverResult? PotionFreeWitness { get; set; }

    internal bool TryGet(int stolen, int potions, out PrimarySearchIncumbent incumbent)
    {
        lock (_bounds)
            return _bounds.TryGetValue((stolen, potions), out incumbent);
    }

    internal bool Tighten(int stolen, int potions, PrimarySearchIncumbent candidate)
    {
        lock (_bounds)
        {
            if (_bounds.TryGetValue((stolen, potions), out var current)
                && (candidate.StrategicHpDeficit > current.StrategicHpDeficit
                    || candidate.StrategicHpDeficit == current.StrategicHpDeficit
                        && candidate.CombatEndedTurn >= current.CombatEndedTurn))
                return false;
            _bounds[(stolen, potions)] = candidate;
            return true;
        }
    }
}
