using System;

public class SwordAttackViewModel : IDisposable
{
    private PlayerModel _playerModel;
    private SwordAttackModel _swordAttackModel;

    private SkillModel _skillModel;
    private SkillData _skillData;

    public string DisplayName => _skillData != null ? _skillData.Name : string.Empty;
    public SkillTargetMode TargetMode => _swordAttackModel.TargetMode;
    public float Cooldown => _swordAttackModel.Cooldown;
    public float Range => _swordAttackModel.Range;
    public double Damage => _playerModel.AttackPower * _swordAttackModel.AttackPowerMultiplier * LevelMultiplier;

    private double LevelMultiplier => _skillData != null ? _skillData.GetDamageMultiplier(_skillModel.GetLevel(SkillIds.SwordAttack)) : 1;

    public SwordAttackViewModel()
    {
        _playerModel = ModelManager.Instance.PlayerModel;
        _swordAttackModel = ModelManager.Instance.SwordAttackModel;
        _skillModel = ModelManager.Instance.SkillModel;

        DataManager.Instance.Skills.TryGetById(SkillIds.SwordAttack, out _skillData);
    }

    public void Dispose()
    {
        _playerModel = null;
        _swordAttackModel = null;
        _skillModel = null;
        _skillData = null;
    }
}
