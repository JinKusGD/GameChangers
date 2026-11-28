using System.Collections.Generic;

public class OfflineConfig : IValidatable
{
    public double MaxSeconds { get; set; } = 3600;

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (MaxSeconds < 0)
        {
            errors.Add($"MaxSeconds는 0 이상이어야 합니다. ({MaxSeconds})");
        }

        return errors;
    }
}
