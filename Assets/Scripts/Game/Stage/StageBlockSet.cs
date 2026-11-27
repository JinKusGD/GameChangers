public class StageBlockSet
{
    public static readonly StageBlockSet Empty = new StageBlockSet();

    private readonly StageBlock[] _blocksByKey = new StageBlock[BlockKey.Max + 1];

    private StageBlockSet() { }

    public static StageBlockSet Create(BlockTable blockTable, StageInfo stageInfo)
    {
        StageBlockSet stageBlocks = new StageBlockSet();

        for (int blockKey = BlockKey.Min; blockKey <= BlockKey.Max; blockKey++)
        {
            blockTable.TryGetByKey(blockKey, out BlockData data);

            stageBlocks._blocksByKey[blockKey] = StageBlock.Create(data, stageInfo);
        }

        return stageBlocks;
    }

    public StageBlock Get(int blockKey)
    {
        if (!BlockKey.IsValid(blockKey)) { return null; }

        return _blocksByKey[blockKey];
    }
}
