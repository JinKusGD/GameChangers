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

    public SwordAttackModel SwordAttackModel { get; } = new SwordAttackModel(
    displayName: "검 공격",
    targetMode: SkillTargetMode.PlayerTarget,
    cooldown: 1.0f,
    range: 1.2f,
    attackPowerMultiplier: 1.0);

    private ModelManager() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }
}
