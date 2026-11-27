public class ArmorTrait : BlockTrait
{
    private const string PercentKey = "Percent";

    private double _percent;

    public override BlockTraitType Type => BlockTraitType.Armor;

    public override double DamageTakenMultiplier => 1 - _percent / 100;

    protected override void OnInitialize(BlockTraitData data)
    {
        _percent = GetPercent(data, PercentKey);
    }

    protected override void OnReset()
    {
        _percent = 0;
    }
}
