using System.Collections.Generic;

public class StageConfig : IValidatable
{
    public double OverflowHpMultiplier { get; set; } = 8;
    public double OverflowGoldMultiplier { get; set; } = 6;
    public float NextStageDelay { get; set; } = 1f;

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (OverflowHpMultiplier < 1)
        {
            errors.Add($"OverflowHpMultiplier는 1 이상이어야 합니다. ({OverflowHpMultiplier})");
        }

        if (OverflowGoldMultiplier < 1)
        {
            errors.Add($"OverflowGoldMultiplier는 1 이상이어야 합니다. ({OverflowGoldMultiplier})");
        }

        if (NextStageDelay < 0)
        {
            errors.Add($"NextStageDelay는 0 이상이어야 합니다. ({NextStageDelay})");
        }

        return errors;
    }
}
