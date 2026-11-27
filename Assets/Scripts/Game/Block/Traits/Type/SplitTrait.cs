public class SplitTrait : BlockTrait
{
    private int _splitBlockKey;

    public override BlockTraitType Type => BlockTraitType.Split;

    public override int SplitBlockKey => _splitBlockKey;

    protected override void OnInitialize(BlockTraitData data)
    {
        _splitBlockKey = DataManager.Instance.Blocks.TryGetById(data.BlockId, out BlockData target) ? target.Key : BlockKey.Empty;
    }

    protected override void OnReset()
    {
        _splitBlockKey = BlockKey.Empty;
    }
}
