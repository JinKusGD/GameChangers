public abstract class BlockTrait
{
    public abstract BlockTraitType Type { get; }

    protected Block Owner { get; private set; }

    public void Initialize(Block owner, BlockTraitData data)
    {
        Owner = owner;
        OnInitialize(data);
    }

    public void Reset()
    {
        OnReset();
        Owner = null;
    }

    protected abstract void OnInitialize(BlockTraitData data);

    protected virtual void OnReset() { }

    public virtual double ModifyDamage(double damage)
    {
        return damage;
    }

    public virtual void OnDamaged(double damage) { }

    public virtual void OnBroken() { }
}
