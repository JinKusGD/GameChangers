using System.Collections.Generic;
using UnityEngine;

public class StageInfo
{
    public string LayoutId { get; }
    public string StageName { get; }
    public Vector2 FieldPosition { get; }
    public double HpMultiplier { get; }
    public double GoldMultiplier { get; }

    public IReadOnlyList<ChapterTraitData> ChapterTraits { get; }

    private StageInfo(StageData stageData, ChapterData chapterData)
    {
        LayoutId = stageData.LayoutId;
        StageName = stageData.StageName;
        FieldPosition = stageData.FieldPosition.ToVector2();
        HpMultiplier = chapterData.HpMultiplier;
        GoldMultiplier = chapterData.GoldMultiplier;
        ChapterTraits = chapterData.Traits;
    }

    public static StageInfo Create(StageData stageData, ChapterData chapterData)
    {
        return new StageInfo(stageData, chapterData);
    }
}
