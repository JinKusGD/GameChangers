using System.Collections.Generic;
using UnityEngine;

public class BlockData : IValidatable
{
    public string Id { get; set; } = string.Empty;
    public double MaxHp { get; set; }
    public string SpriteName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#FFFFFF";
    public List<BlockTraitData> Traits { get; set; } = new List<BlockTraitData>();

    public Sprite Sprite { get; private set; }
    public Color Color { get; private set; } = Color.white;

    public void ResolveAssets(string spriteFolderPath)
    {
        ColorUtility.TryParseHtmlString(ColorHex, out Color color);
        Color = color;

        if (string.IsNullOrEmpty(SpriteName)) { return; }

        Sprite = Resources.Load<Sprite>(spriteFolderPath + SpriteName);

        if (Sprite == null)
        {
            DebugUtility.LogWarning($"[BlockData] '{Id}': Resources/{spriteFolderPath}{SpriteName} 스프라이트가 없어 기본 스프라이트를 씁니다.");
        }
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Id가 비어 있습니다.");
        }

        if (MaxHp <= 0)
        {
            errors.Add($"MaxHp는 0보다 커야 합니다. ({MaxHp})");
        }

        if (!ColorUtility.TryParseHtmlString(ColorHex, out _))
        {
            errors.Add($"Color 형식이 잘못됐습니다. ({ColorHex})");
        }

        if (Traits == null)
        {
            Traits = new List<BlockTraitData>();
        }

        foreach (BlockTraitData trait in Traits)
        {
            if (trait == null)
            {
                errors.Add("특성 데이터가 비어 있습니다.");
                continue;
            }

            if (!BlockTraitFactory.TryCreate(trait.Type, out _))
            {
                errors.Add($"알 수 없는 특성입니다. ({trait.Type})");
            }
        }

        return errors;
    }
}
