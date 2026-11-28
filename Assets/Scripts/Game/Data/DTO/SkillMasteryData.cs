using System;
using System.Collections.Generic;

public class SkillMasteryData : IValidatable
{
    private const int DefaultBaseCount = 100;
    private const double DefaultGrowth = 1.05d;

    private const int MinBaseCount = 1;
    private const double MinGrowth = 1;

    public int BaseCount { get; set; } = DefaultBaseCount;
    public double Growth { get; set; } = DefaultGrowth;

    public int GetRequiredCount(int level)
    {
        double count = Math.Ceiling(LevelUtility.Grow(BaseCount, Growth, level));

        if (double.IsNaN(count) || count >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return Math.Max(MinBaseCount, (int)count);
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (BaseCount < MinBaseCount)
        {
            errors.Add($"BaseCount는 {MinBaseCount} 이상이어야 합니다. ({BaseCount})");
        }

        if (!double.IsFinite(Growth) || Growth < MinGrowth)
        {
            errors.Add($"Growth는 {MinGrowth} 이상의 유한한 숫자여야 합니다. ({Growth})");
        }

        return errors;
    }
}
