using System;
using System.Collections.Generic;

public static class BlockTraitFactory
{
    private static readonly Dictionary<BlockTraitType, Func<BlockTrait>> Creators = new Dictionary<BlockTraitType, Func<BlockTrait>>
    {
        { BlockTraitType.Armor, () => new ArmorTrait() },
        { BlockTraitType.Shield, () => new ShieldTrait() },
        { BlockTraitType.Treasure, () => new TreasureTrait() },
        { BlockTraitType.Regen, () => new RegenTrait() },
        { BlockTraitType.Split, () => new SplitTrait() },
        { BlockTraitType.Link, () => new LinkTrait() },
        { BlockTraitType.Guardian, () => new GuardianTrait() },
        { BlockTraitType.DamageCap, () => new DamageCapTrait() },
    };

    public static bool TryCreate(BlockTraitType type, out BlockTrait trait)
    {
        if (!Creators.TryGetValue(type, out Func<BlockTrait> create))
        {
            trait = null;
            return false;
        }

        trait = create();
        return true;
    }

    public static bool IsSupported(BlockTraitType type)
    {
        return Creators.ContainsKey(type);
    }
}
