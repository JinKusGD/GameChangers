using System;

public abstract class BlockTrait
{
    private const string GoldBonusKey = "GoldBonus";

    public abstract BlockTraitType Type { get; }

    protected Block Owner { get; private set; }

    public double GoldBonus { get; private set; }

    public virtual double DamageTakenMultiplier => 1;
    public virtual double GuardPercent => 0;
    public virtual bool SharesDamage => false;
    public virtual int SplitBlockKey => BlockKey.Empty;

    public void Initialize(Block owner, BlockTraitData data)
    {
        Owner = owner;
        GoldBonus = data.GetParam(GoldBonusKey);
        OnInitialize(data);
    }

    public void Reset()
    {
        OnReset();
        GoldBonus = 0;
        Owner = null;
    }

    protected virtual void OnInitialize(BlockTraitData data) { }

    protected virtual void OnReset() { }

    public virtual double ModifyDamage(double damage)
    {
        return damage;
    }

    public virtual double MergeParam(string key, double current, double added)
    {
        return current + added;
    }

    protected static double GetPercent(BlockTraitData data, string key, double defaultValue = 0)
    {
        return Math.Clamp(data.GetParam(key, defaultValue), 0, 100);
    }

    protected static bool IsKey(string key, string name)
    {
        return string.Equals(key, name, StringComparison.OrdinalIgnoreCase);
    }
}
