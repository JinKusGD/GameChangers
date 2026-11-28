using System;

public static class LevelUtility
{
    public static int ToIndex(int level)
    {
        return Math.Max(0, level - 1);
    }

    public static double Grow(double baseValue, double growth, int level)
    {
        return baseValue * Math.Pow(growth, ToIndex(level));
    }
}
