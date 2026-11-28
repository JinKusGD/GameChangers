using System.ComponentModel;

public class SwordAttackModel : ObservableModel, ISkillStatsModel
{
    private static readonly PropertyChangedEventArgs TargetModeChangedArgs = new PropertyChangedEventArgs(nameof(TargetMode));
    private static readonly PropertyChangedEventArgs CooldownChangedArgs = new PropertyChangedEventArgs(nameof(Cooldown));
    private static readonly PropertyChangedEventArgs RangeChangedArgs = new PropertyChangedEventArgs(nameof(Range));
    private static readonly PropertyChangedEventArgs AttackPowerMultiplierChangedArgs = new PropertyChangedEventArgs(nameof(AttackPowerMultiplier));

    private SkillTargetMode _targetMode = SkillTargetMode.PlayerTarget;
    private float _cooldown = 1f;
    private float _range = 1f;
    private double _attackPowerMultiplier = 1;

    public SkillTargetMode TargetMode
    {
        get { return _targetMode; }
        private set { SetProperty(ref _targetMode, value, TargetModeChangedArgs); }
    }

    public float Cooldown
    {
        get { return _cooldown; }
        set { SetProperty(ref _cooldown, value, CooldownChangedArgs); }
    }

    public float Range
    {
        get { return _range; }
        set { SetProperty(ref _range, value, RangeChangedArgs); }
    }

    public double AttackPowerMultiplier
    {
        get { return _attackPowerMultiplier; }
        set { SetProperty(ref _attackPowerMultiplier, value, AttackPowerMultiplierChangedArgs); }
    }

    public void Load(SkillData data)
    {
        if (data == null) { return; }

        TargetMode = data.TargetMode;
        Cooldown = data.Cooldown;
        Range = data.Range;
        AttackPowerMultiplier = data.AttackPowerMultiplier;
    }
}
