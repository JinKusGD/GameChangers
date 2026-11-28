public static class SkillDamageCalculator
{
    public static double GetDamage(double attackPower, ISkillStatsModel stats, SkillData data, int level)
    {
        if (stats == null || data == null)
        {
            return 0;
        }

        return attackPower * stats.AttackPowerMultiplier * data.GetDamageMultiplier(level);
    }

    public static double GetDamagePerSecond(double attackPower, ISkillStatsModel stats, SkillData data, int level)
    {
        if (stats == null || stats.Cooldown <= 0f)
        {
            return 0;
        }

        return GetDamage(attackPower, stats, data, level) / stats.Cooldown;
    }
}
