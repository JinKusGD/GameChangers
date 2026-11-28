using System;
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

    private StageInfo(StageData stageData, ChapterData chapterData, int overflowChapters)
    {
        LayoutId = stageData.LayoutId;
        StageName = stageData.StageName;
        FieldPosition = stageData.FieldPosition.ToVector2();

        StageConfig config = DataManager.Instance.Config.Stage;
        HpMultiplier = chapterData.HpMultiplier * Math.Pow(config.OverflowHpMultiplier, overflowChapters);
        GoldMultiplier = chapterData.GoldMultiplier * Math.Pow(config.OverflowGoldMultiplier, overflowChapters);
        ChapterTraits = chapterData.Traits;
    }

    public static StageInfo Create(StageData stageData, ChapterData chapterData, int overflowChapters = 0)
    {
        return new StageInfo(stageData, chapterData, Math.Max(0, overflowChapters));
    }
}
