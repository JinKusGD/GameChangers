using System.Collections.Generic;

public class BlockTraitData
{
    public BlockTraitType Type { get; set; } = BlockTraitType.None;

    public Dictionary<string, double> Params { get; set; } = new Dictionary<string, double>();

    public double GetParam(string key, double defaultValue = 0)
    {
        if (Params == null)
        {
            return defaultValue;
        }

        if (!Params.TryGetValue(key, out double value))
        {
            return defaultValue;
        }

        return value;
    }
}
