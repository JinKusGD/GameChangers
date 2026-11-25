using System.Collections.Generic;

public class BlockLayoutTable
{
    private const string Tag = "BlockLayoutTable";
    private const string ResourcePath = "Blocks/BlockLayouts";

    private readonly Dictionary<string, BlockLayout> _layoutsById = new Dictionary<string, BlockLayout>();

    public void Load()
    {
        _layoutsById.Clear();

        List<BlockLayout> rows = JsonTableLoader.Load<BlockLayout>(ResourcePath, Tag, DescribeRow);

        foreach (BlockLayout layout in rows)
        {
            if (!_layoutsById.TryAdd(layout.Id, layout))
            {
                DebugUtility.LogError($"[{Tag}] {DescribeRow(layout)} 제외됨: Id 중복");
            }
        }
    }

    public BlockLayout GetById(string id)
    {
        if (id == null || !_layoutsById.TryGetValue(id, out BlockLayout layout))
        {
            DebugUtility.LogError($"[{Tag}] Id '{id}' 레이아웃이 없습니다.");
            return null;
        }

        return layout;
    }

    private static string DescribeRow(BlockLayout layout)
    {
        return $"'{layout.Id}'";
    }
}
