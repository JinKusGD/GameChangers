using System.Collections.Generic;

public class StageBlock
{
    public BlockData Data { get; }
    public double MaxHp { get; }
    public double Gold { get; }
    public IReadOnlyList<BlockTraitData> Traits { get; }

    private StageBlock(BlockData data, double maxHp, double gold, IReadOnlyList<BlockTraitData> traits)
    {
        Data = data;
        MaxHp = maxHp;
        Gold = gold;
        Traits = traits;
    }

    public static StageBlock Create(BlockData data, StageInfo stageInfo)
    {
        if (data == null) { return null; }

        double maxHp = data.MaxHp * stageInfo.HpMultiplier;
        double gold = data.Gold * stageInfo.GoldMultiplier;
        List<BlockTraitData> traits = BlockTraitMerger.Merge(data.Traits, stageInfo.ChapterTraits, data.Id);

        return new StageBlock(data, maxHp, gold, traits);
    }
}
