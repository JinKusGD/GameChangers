using System.Collections.Generic;

public class BlockFieldSaveHandler
{
    private readonly BlockGrid _grid;

    public BlockFieldSaveHandler(BlockGrid grid)
    {
        _grid = grid;
    }

    public void Save(StageSaveData stageSave, BlockLayout layout, StageBlockSet stageBlocks)
    {
        if (stageSave.Blocks == null)
        {
            stageSave.Blocks = new List<BlockSaveData>();
        }

        stageSave.Blocks.Clear();
        stageSave.LayoutId = layout?.Id;

        if (layout == null) { return; }

        for (int row = 0; row < _grid.Size; row++)
        {
            for (int column = 0; column < _grid.Size; column++)
            {
                if (TryCreateSaveData(column, row, layout, stageBlocks, out BlockSaveData data))
                {
                    stageSave.Blocks.Add(data);
                }
            }
        }
    }

    public void Load(StageSaveData stageSave, BlockLayout layout, StageBlockSet stageBlocks)
    {
        if (layout == null || stageSave.Blocks == null || stageSave.LayoutId != layout.Id) { return; }

        foreach (BlockSaveData data in stageSave.Blocks)
        {
            RestoreCell(data, stageBlocks);
        }
    }

    private bool TryCreateSaveData(int column, int row, BlockLayout layout, StageBlockSet stageBlocks, out BlockSaveData data)
    {
        data = default;

        int layoutKey = layout.GetBlockKey(column, row);

        if (!IsLayoutCell(layoutKey, stageBlocks)) { return false; }

        Block cell = _grid.GetCell(column, row);
        int splitBlockKey = GetSplitBlockKey(cell, layoutKey);

        if (IsUntouched(cell, splitBlockKey)) { return false; }

        data = new BlockSaveData
        {
            CellIndex = _grid.ToIndex(column, row),
            Hp = cell.Hp,
            SplitBlockKey = splitBlockKey
        };

        if (cell.TryGetShield(out ShieldTrait shield))
        {
            data.ShieldHits = shield.RemainingHits;
        }

        return true;
    }

    private void RestoreCell(BlockSaveData data, StageBlockSet stageBlocks)
    {
        if (!_grid.IsValidIndex(data.CellIndex)) { return; }

        Block cell = _grid.GetCell(data.CellIndex);

        if (!cell.IsAlive) { return; }

        if (data.Hp <= 0)
        {
            _grid.RemoveBlock(cell);
            return;
        }

        if (data.SplitBlockKey != BlockKey.Empty && !TryRestoreSplitBlock(cell, stageBlocks.Get(data.SplitBlockKey)))
        {
            return;
        }

        cell.LoadHp(data.Hp);
        RestoreShield(cell, data);
    }

    private static void RestoreShield(Block cell, BlockSaveData data)
    {
        if (!data.ShieldHits.HasValue || !cell.TryGetShield(out ShieldTrait shield)) { return; }

        shield.RestoreHits(data.ShieldHits.Value);
    }

    private bool TryRestoreSplitBlock(Block cell, StageBlock stageBlock)
    {
        if (stageBlock == null) { return false; }

        _grid.PlaceBlock(cell, stageBlock);
        return true;
    }

    private static bool IsLayoutCell(int layoutKey, StageBlockSet stageBlocks)
    {
        return stageBlocks.Get(layoutKey) != null;
    }

    private static bool IsUntouched(Block cell, int splitBlockKey)
    {
        bool isSplit = splitBlockKey != BlockKey.Empty;

        bool isShieldUsed = cell.TryGetShield(out ShieldTrait shield) && shield.IsUsed;

        return !isSplit && !isShieldUsed && cell.IsAlive && cell.Hp >= cell.MaxHp;
    }

    private static int GetSplitBlockKey(Block cell, int layoutKey)
    {
        if (!cell.IsAlive || cell.Key == layoutKey) { return BlockKey.Empty; }

        return cell.Key;
    }
}
