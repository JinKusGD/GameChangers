using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageSaveData
{   
    public Vector2? PlayerPosition;

    public string LayoutId;
    public List<BlockSaveData> Blocks = new List<BlockSaveData>();

    public void SetPlayerPosition(Vector2 position)
    {
        PlayerPosition = position;
    }

    public bool TryGetPlayerPosition(out Vector2 position)
    {
        if (PlayerPosition.HasValue)
        {
            position = PlayerPosition.Value;
            return true;
        }

        position = Vector2.zero;
        return false;
    }
}
