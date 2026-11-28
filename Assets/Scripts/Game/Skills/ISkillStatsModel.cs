using System.ComponentModel;

public interface ISkillStatsModel : INotifyPropertyChanged
{
    float Cooldown { get; }
    double AttackPowerMultiplier { get; }
}
