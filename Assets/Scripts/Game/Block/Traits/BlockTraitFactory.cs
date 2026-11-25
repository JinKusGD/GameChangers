public static class BlockTraitFactory
{
    public static bool TryCreate(BlockTraitType type, out BlockTrait trait)
    {
        trait = null;

        switch (type)
        {
            case BlockTraitType.Armor:
                trait = new ArmorTrait();
                return true;
            default:
                return false;
        }
    }
}
