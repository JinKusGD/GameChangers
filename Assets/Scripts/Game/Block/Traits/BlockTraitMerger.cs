using System.Collections.Generic;

public static class BlockTraitMerger
{
    private static readonly Dictionary<BlockTraitType, BlockTrait> RulesByType = new Dictionary<BlockTraitType, BlockTrait>();

    public static List<BlockTraitData> Merge(IReadOnlyList<BlockTraitData> blockTraits, IReadOnlyList<ChapterTraitData> chapterTraits, string blockId)
    {
        List<BlockTraitData> result = new List<BlockTraitData>();

        if (blockTraits != null)
        {
            foreach (BlockTraitData trait in blockTraits)
            {
                Add(result, trait);
            }
        }

        if (chapterTraits != null)
        {
            foreach (ChapterTraitData trait in chapterTraits)
            {
                if (!trait.AppliesTo(blockId)) { continue; }

                Add(result, trait);
            }
        }

        return result;
    }

    private static void Add(List<BlockTraitData> result, BlockTraitData trait)
    {
        BlockTraitData blockTraitData = GetOrAddByType(result, trait.Type);
        AddTo(blockTraitData, trait);
    }

    private static BlockTraitData GetOrAddByType(List<BlockTraitData> result, BlockTraitType type)
    {
        foreach (BlockTraitData existing in result)
        {
            if (existing.Type == type) { return existing; }
        }

        BlockTraitData added = new BlockTraitData();
        added.Type = type;
        result.Add(added);

        return added;
    }

    private static void AddTo(BlockTraitData target, BlockTraitData trait)
    {
        if (!string.IsNullOrEmpty(trait.BlockId))
        {
            target.BlockId = trait.BlockId;
        }

        foreach (KeyValuePair<string, double> param in trait.Params)
        {
            target.Params[param.Key] = MergeParam(target, param.Key, param.Value);
        }
    }

    private static double MergeParam(BlockTraitData target, string key, double added)
    {
        if (!target.Params.TryGetValue(key, out double current)) { return added; }

        BlockTrait rule = GetRule(target.Type);

        return rule != null ? rule.MergeParam(key, current, added) : current + added;
    }

    private static BlockTrait GetRule(BlockTraitType type)
    {
        if (RulesByType.TryGetValue(type, out BlockTrait rule)) { return rule; }

        BlockTraitFactory.TryCreate(type, out rule);
        RulesByType.Add(type, rule);

        return rule;
    }
}
