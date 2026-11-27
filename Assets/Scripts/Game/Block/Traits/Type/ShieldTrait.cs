using System;

public class ShieldTrait : BlockTrait
{
    private const string HitsKey = "Hits";

    private int _remainingHits;
    private int _maxHits;

    public override BlockTraitType Type => BlockTraitType.Shield;

    public bool IsUsed => _remainingHits < _maxHits;

    public int RemainingHits => _remainingHits;

    protected override void OnInitialize(BlockTraitData data)
    {
        _maxHits = Math.Max(0, (int)data.GetParam(HitsKey, 1));
        _remainingHits = _maxHits;
    }

    protected override void OnReset()
    {
        _remainingHits = 0;
        _maxHits = 0;
    }

    public void RestoreHits(int remainingHits)
    {
        _remainingHits = Math.Clamp(remainingHits, 0, _maxHits);
    }

    public override double ModifyDamage(double damage)
    {
        if (_remainingHits <= 0) { return damage; }

        _remainingHits--;
        return 0;
    }
}
