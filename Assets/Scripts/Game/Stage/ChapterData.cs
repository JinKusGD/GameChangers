using System.Collections.Generic;
using System.Runtime.Serialization;

public class ChapterData : IValidatable
{
    public int Chapter { get; set; }
    public double HpMultiplier { get; set; } = 1;
    public double GoldMultiplier { get; set; } = 1;
    public List<ChapterTraitData> Traits { get; set; } = new List<ChapterTraitData>();

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (Traits == null)
        {
            Traits = new List<ChapterTraitData>();
        }
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (Chapter < 1)
        {
            errors.Add($"Chapter는 1 이상이어야 합니다. ({Chapter})");
        }

        if (HpMultiplier <= 0)
        {
            errors.Add($"HpMultiplier는 0보다 커야 합니다. ({HpMultiplier})");
        }

        if (GoldMultiplier < 0)
        {
            errors.Add($"GoldMultiplier는 0 이상이어야 합니다. ({GoldMultiplier})");
        }

        return errors;
    }

    public List<string> RemoveInvalidTraits()
    {
        List<string> errors = new List<string>();
        List<ChapterTraitData> validTraits = new List<ChapterTraitData>(Traits.Count);

        for (int i = 0; i < Traits.Count; i++)
        {
            int errorCount = errors.Count;

            ValidateTrait(i, errors);

            if (errors.Count == errorCount)
            {
                validTraits.Add(Traits[i]);
            }
        }

        Traits = validTraits;

        return errors;
    }

    private void ValidateTrait(int index, List<string> errors)
    {
        ChapterTraitData trait = Traits[index];

        if (trait == null)
        {
            errors.Add($"Traits[{index}] 항목이 null입니다. JSON에서 빈 항목을 지우세요.");
            return;
        }

        trait.Validate(index, errors);

        if (trait.Type == BlockTraitType.Split)
        {
            errors.Add($"Traits[{index}] Split은 챕터 특성으로 쓸 수 없습니다. BlockTable에서 블록에 직접 넣으세요.");
        }

        if (!string.IsNullOrEmpty(trait.BlockId))
        {
            errors.Add($"Traits[{index}] 챕터 특성은 BlockId 대신 BlockIds(목록)를 씁니다. ('{trait.BlockId}')");
        }
    }
}
