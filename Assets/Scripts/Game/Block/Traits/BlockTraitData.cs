using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class BlockTraitData
{
    public BlockTraitType Type { get; set; } = BlockTraitType.None;

    public Dictionary<string, double> Params { get; set; } = CreateParams();

    public string BlockId { get; set; } = string.Empty;

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        Params = ToCaseInsensitive(Params);

        if (BlockId == null)
        {
            BlockId = string.Empty;
        }
    }

    private static Dictionary<string, double> CreateParams()
    {
        return new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, double> ToCaseInsensitive(Dictionary<string, double> source)
    {
        Dictionary<string, double> result = CreateParams();

        if (source == null) { return result; }

        foreach (KeyValuePair<string, double> param in source)
        {
            result[param.Key] = param.Value;
        }

        return result;
    }

    public double GetParam(string key, double defaultValue = 0)
    {
        return Params.TryGetValue(key, out double value) ? value : defaultValue;
    }

    public static void ValidateList(IReadOnlyList<BlockTraitData> traits, List<string> errors)
    {
        for (int i = 0; i < traits.Count; i++)
        {
            BlockTraitData trait = traits[i];

            if (trait == null)
            {
                errors.Add($"Traits[{i}] 항목이 null입니다. JSON에서 빈 항목을 지우세요.");
                continue;
            }

            trait.Validate(i, errors);
        }
    }

    public void Validate(int index, List<string> errors)
    {
        if (!BlockTraitFactory.IsSupported(Type))
        {
            errors.Add($"Traits[{index}] 알 수 없는 특성입니다. ({Type})");
        }

        if (Type == BlockTraitType.Split && string.IsNullOrEmpty(BlockId))
        {
            errors.Add($"Traits[{index}] Split에는 BlockId가 필요합니다.");
        }
    }
}
