using System;

public class SwordAttackViewModel : IDisposable
{
    private PlayerModel _playerModel;
    private SwordAttackModel _swordAttackModel;

    public string DisplayName => _swordAttackModel.DisplayName;
    public SkillTargetMode TargetMode => _swordAttackModel.TargetMode;
    public float Cooldown => _swordAttackModel.Cooldown;
    public float Range => _swordAttackModel.Range;
    public double Damage => _playerModel.AttackPower * _swordAttackModel.AttackPowerMultiplier;

    public SwordAttackViewModel()
    {
        _playerModel = ModelManager.Instance.PlayerModel;
        _swordAttackModel = ModelManager.Instance.SwordAttackModel;
    }

    public void Dispose()
    {
        _playerModel = null;
        _swordAttackModel = null;
    }
}
