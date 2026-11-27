using System;

public class DamageCapTrait : BlockTrait
{
    private const string PercentKey = "Percent";
    private const double NoCapPercent = 100;
    private const double MinCapPercent = 1;

    private double _percent;

    public override BlockTraitType Type => BlockTraitType.DamageCap;

    public override double MergeParam(string key, double current, double added)
    {
        if (IsKey(key, PercentKey)) { return Math.Min(current, added); }

        return base.MergeParam(key, current, added);
    }

    protected override void OnInitialize(BlockTraitData data)
    {
        _percent = Math.Max(GetPercent(data, PercentKey, NoCapPercent), MinCapPercent);
    }

    protected override void OnReset()
    {
        _percent = NoCapPercent;
    }

    public override double ModifyDamage(double damage)
    {
        return Math.Min(damage, Owner.MaxHp * _percent / 100);
    }
}
