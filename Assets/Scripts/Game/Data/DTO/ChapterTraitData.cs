using System;
using System.Collections.Generic;

public class ChapterTraitData : BlockTraitData
{
    public List<string> BlockIds { get; set; } = new List<string>();

    public bool AppliesTo(string blockId)
    {
        if (BlockIds == null || BlockIds.Count == 0)
        {
            return true;
        }

        foreach (string id in BlockIds)
        {
            if (string.Equals(id, blockId, StringComparison.OrdinalIgnoreCase)) { return true; }
        }

        return false;
    }
}
