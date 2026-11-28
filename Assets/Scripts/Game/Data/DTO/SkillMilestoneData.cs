using System.Collections.Generic;

public class SkillMilestoneData : IValidatable
{
    private const double DefaultDamageMultiplier = 1;

    public int Level { get; set; }
    public double DamageMultiplier { get; set; } = DefaultDamageMultiplier;

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (!double.IsFinite(DamageMultiplier) || DamageMultiplier <= 0)
        {
            errors.Add($"DamageMultiplier는 0보다 큰 유한한 숫자여야 합니다. ({DamageMultiplier})");
        }

        return errors;
    }
}
