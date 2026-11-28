using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

public class SkillUnlockConfig : IValidatable
{
    public int InitialMaxCount { get; set; } = 3;
    public int ChoiceCount { get; set; } = 2;

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<double> Costs { get; set; } = new List<double> { 100 };
    public double CostGrowthAfterList { get; set; } = 1;

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        Costs ??= new List<double>();
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (InitialMaxCount < 1)
        {
            errors.Add($"InitialMaxCount는 1 이상이어야 합니다. ({InitialMaxCount})");
        }

        if (ChoiceCount < 1)
        {
            errors.Add($"ChoiceCount는 1 이상이어야 합니다. ({ChoiceCount})");
        }

        if (Costs.Count == 0)
        {
            errors.Add("Costs가 비어 있습니다.");
        }

        for (int i = 0; i < Costs.Count; i++)
        {
            if (Costs[i] < 0)
            {
                errors.Add($"Costs[{i}]는 0 이상이어야 합니다. ({Costs[i]})");
            }

            if (i > 0 && Costs[i] < Costs[i - 1])
            {
                errors.Add($"Costs[{i}]가 이전 값보다 작습니다. ({Costs[i]} < {Costs[i - 1]})");
            }
        }

        if (CostGrowthAfterList < 1)
        {
            errors.Add($"CostGrowthAfterList는 1 이상이어야 합니다. ({CostGrowthAfterList})");
        }

        return errors;
    }

    public double GetUnlockCost(int unlockNumber)
    {
        if (Costs.Count == 0) { return 0; }

        int index = Math.Max(1, unlockNumber) - 1;

        if (index < Costs.Count) { return Costs[index]; }

        int overflow = index - (Costs.Count - 1);

        return Costs[Costs.Count - 1] * Math.Pow(CostGrowthAfterList, overflow);
    }
}
