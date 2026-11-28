using System.Collections.Generic;

public class SkillCostData : IValidatable
{
    private const double MinBase = 1;
    private const double MinGrowth = 1;

    public double Base { get; set; } = MinBase;
    public double Growth { get; set; } = MinGrowth;

    public double Evaluate(int level)
    {
        return LevelUtility.Grow(Base, Growth, level);
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (!double.IsFinite(Base) || Base < MinBase) 
        { 
            errors.Add($"Base는 {MinBase} 이상의 유한한 숫자여야 합니다. ({Base})"); 
        }
        if (!double.IsFinite(Growth) || Growth < MinGrowth)
        {
            errors.Add($"Growth는 {MinGrowth} 이상의 유한한 숫자여야 합니다. ({Growth})"); 
        }

        return errors;
    }
}
