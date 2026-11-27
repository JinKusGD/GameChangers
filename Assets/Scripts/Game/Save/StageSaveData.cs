using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageSaveData
{   
    public bool HasPlayerPosition;
    public Vector2 PlayerPosition;

    public string LayoutId;
    public List<BlockSaveData> Blocks = new List<BlockSaveData>();

    public void SetPlayerPosition(Vector2 position)
    {
        PlayerPosition = position;
        HasPlayerPosition = true;
    }

    public bool TryGetPlayerPosition(out Vector2 position)
    {
        position = PlayerPosition;

        return HasPlayerPosition;
    }
}
