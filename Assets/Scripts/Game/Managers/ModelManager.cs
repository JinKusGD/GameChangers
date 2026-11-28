using UnityEngine;

public sealed class ModelManager
{
    private static ModelManager _instance;

    public static ModelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ModelManager();
            }

            return _instance;
        }
    }

    public PlayerModel PlayerModel { get; } = new PlayerModel(moveSpeed: 5.0f, attackPower: 3.0);
    public CurrencyModel CurrencyModel { get; } = new CurrencyModel();
    public StageModel StageModel { get; } = new StageModel();
    public SkillModel SkillModel { get; } = new SkillModel();
    public SwordAttackModel SwordAttackModel { get; } = new SwordAttackModel();

    public bool TryGetSkillStats(string skillId, out ISkillStatsModel stats)
    {
        stats = null;

        if (string.Equals(skillId, SkillIds.SwordAttack, System.StringComparison.OrdinalIgnoreCase))
        {
            stats = SwordAttackModel;
        }

        return stats != null;
    }

    private ModelManager() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }
}
