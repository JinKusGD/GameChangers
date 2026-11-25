using System.ComponentModel;

public class SwordAttackModel : ObservableModel
{
    private static readonly PropertyChangedEventArgs CooldownChangedArgs = new PropertyChangedEventArgs(nameof(Cooldown));
    private static readonly PropertyChangedEventArgs RangeChangedArgs = new PropertyChangedEventArgs(nameof(Range));
    private static readonly PropertyChangedEventArgs AttackPowerMultiplierChangedArgs = new PropertyChangedEventArgs(nameof(AttackPowerMultiplier));

    private float _cooldown;
    private float _range;
    private double _attackPowerMultiplier;

    public string DisplayName { get; }
    public SkillTargetMode TargetMode { get; }

    public float Cooldown
    {
        get => _cooldown;
        set => SetProperty(ref _cooldown, value, CooldownChangedArgs);
    }

    public float Range
    {
        get => _range;
        set => SetProperty(ref _range, value, RangeChangedArgs);
    }

    public double AttackPowerMultiplier
    {
        get => _attackPowerMultiplier;
        set => SetProperty(ref _attackPowerMultiplier, value, AttackPowerMultiplierChangedArgs);
    }

    public SwordAttackModel(string displayName, SkillTargetMode targetMode, float cooldown, float range, double attackPowerMultiplier)
    {
        DisplayName = displayName;
        TargetMode = targetMode;
        _cooldown = cooldown;
        _range = range;
        _attackPowerMultiplier = attackPowerMultiplier;
    }
}
