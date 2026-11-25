using System.Collections.Generic;
using UnityEngine;

public class StageInfo
{
    public string LayoutId { get; set; } = string.Empty;
    public Vector2 FieldPosition { get; set; }
    public Dictionary<int, string> BlockIdsByKey { get; set; } = new Dictionary<int, string>();

    public string GetBlockId(int blockKey)
    {
        if (BlockIdsByKey != null && BlockIdsByKey.TryGetValue(blockKey, out string blockId))
        {
            return blockId;
        }

        return null;
    }
}
