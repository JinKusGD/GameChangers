using System.Collections.Generic;

public class BlockTable
{
    private const string Tag = "BlockTable";
    private const string ResourcePath = "Blocks/BlockTable";
    private const string SpriteFolderPath = "Blocks/Sprites/";

    private readonly Dictionary<string, BlockData> _dataById = new Dictionary<string, BlockData>();

    public void Load()
    {
        _dataById.Clear();

        List<BlockData> rows = JsonTableLoader.Load<BlockData>(ResourcePath, Tag, DescribeRow);

        foreach (BlockData data in rows)
        {
            data.ResolveAssets(SpriteFolderPath);

            if (!_dataById.TryAdd(data.Id, data))
            {
                DebugUtility.LogError($"[{Tag}] {DescribeRow(data)} 제외됨: Id 중복");
            }
        }
    }

    public BlockData GetById(string id)
    {
        if (id == null || !_dataById.TryGetValue(id, out BlockData data))
        {
            DebugUtility.LogError($"[{Tag}] Id '{id}' 데이터가 없습니다.");
            return null;
        }

        return data;
    }

    private static string DescribeRow(BlockData data)
    {
        return $"'{data.Id}'";
    }
}
