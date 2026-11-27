public static class BlockKey
{
    public const int Empty = 0;
    public const int Min = 1;
    public const int Max = 9;

    public static bool IsValid(int key)
    {
        return key >= Min && key <= Max;
    }
}
