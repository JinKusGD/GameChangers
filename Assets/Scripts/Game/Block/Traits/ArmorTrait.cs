using System;

public class ArmorTrait : BlockTrait
{
    private const string AmountKey = "Amount";

    private double _amount;

    public override BlockTraitType Type => BlockTraitType.Armor;

    protected override void OnInitialize(BlockTraitData data)
    {
        _amount = data.GetParam(AmountKey);
    }

    protected override void OnReset()
    {
        _amount = 0;
    }

    public override double ModifyDamage(double damage)
    {
        return Math.Max(0, damage - _amount);
    }
}
