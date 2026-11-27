using System;
using System.Globalization;

public static class NumberUtility
{
    static readonly string[] Units = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

    private const double FloorEpsilon = 1e-9;

    public static string Format(double value)
    {
        var inv = CultureInfo.InvariantCulture;
        if (value < 1000)
        {
            if (value < 10 && value % 1 != 0)
            {
                return FloorTo(value, 1).ToString("0.#", inv);
            }

            return Math.Floor(value).ToString("0", inv);
        }

        int unit = 0;
        while (value >= 1000 && unit < Units.Length - 1)
        {
            value /= 1000;
            unit++;
        }

        string number;

        if (value < 10)
        {
            number = FloorTo(value, 2).ToString("0.##", inv);
        }
        else if (value < 100)
        {
            number = FloorTo(value, 1).ToString("0.#", inv);
        }
        else
        {
            number = Math.Floor(value).ToString("0", inv);
        }

        return number + Units[unit];
    }

    private static double FloorTo(double value, int digits)
    {
        double scale = Math.Pow(10, digits);

        return Math.Floor(value * scale + FloorEpsilon) / scale;
    }
}
