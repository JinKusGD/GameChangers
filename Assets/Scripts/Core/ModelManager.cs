using UnityEngine;

public sealed class ModelManager
{
    private static ModelManager _instance;

    public static ModelManager Instance => _instance ??= new ModelManager();

    public PlayerModel PlayerModel { get; private set; } = new PlayerModel(moveSpeed: 5.0f, attackPower: 3.0);
    public SwordAttackModel SwordAttackModel { get; private set; } = new SwordAttackModel(
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
