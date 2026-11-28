using System.Collections.Generic;

public class SkillDamageData : IValidatable
{
    private const double DefaultBase = 1;
    private const double MinPerLevel = 0;

    public double Base { get; set; } = DefaultBase;
    public double PerLevel { get; set; } = MinPerLevel;

    public double Evaluate(int level)
    {
        return Base + PerLevel * LevelUtility.ToIndex(level);
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (!double.IsFinite(Base) || Base <= 0) 
        { 
            errors.Add($"Base는 0보다 큰 유한한 숫자여야 합니다. ({Base})"); 
        }
        if (!double.IsFinite(PerLevel) || PerLevel < MinPerLevel) 
        { 
            errors.Add($"PerLevel은 {MinPerLevel} 이상의 유한한 숫자여야 합니다. ({PerLevel})"); 
        }

        return errors;
    }
}
