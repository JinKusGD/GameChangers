public class GuardianTrait : BlockTrait
{
    private const string PercentKey = "Percent";

    private double _percent;

    public override BlockTraitType Type => BlockTraitType.Guardian;

    public override double GuardPercent => _percent;

    protected override void OnInitialize(BlockTraitData data)
    {
        _percent = GetPercent(data, PercentKey);
    }

    protected override void OnReset()
    {
        _percent = 0;
    }
}
