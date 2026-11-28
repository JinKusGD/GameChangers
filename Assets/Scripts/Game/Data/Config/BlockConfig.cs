using System.Collections.Generic;

public class BlockConfig : IValidatable
{
    public double MinDamageTakenMultiplier { get; set; } = 0.1;

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (MinDamageTakenMultiplier < 0 || MinDamageTakenMultiplier > 1)
        {
            errors.Add($"MinDamageTakenMultiplier는 0~1 사이여야 합니다. ({MinDamageTakenMultiplier})");
        }

        return errors;
    }
}
