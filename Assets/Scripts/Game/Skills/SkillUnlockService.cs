using System;
using System.Collections.Generic;

public static class SkillUnlockService
{
    private static SkillModel SkillModel
    {
        get { return ModelManager.Instance.SkillModel; }
    }

    private static CurrencyModel CurrencyModel
    {
        get { return ModelManager.Instance.CurrencyModel; }
    }

    private static SkillUnlockConfig Config
    {
        get { return DataManager.Instance.Config.SkillUnlock; }
    }

    public static bool HasEmptySlot
    {
        get { return !SkillModel.IsFull; }
    }

    public static double Cost
    {
        get { return Config.GetUnlockCost(SkillModel.OwnedSkillIds.Count); }
    }

    public static bool CanUnlock
    {
        get { return HasEmptySlot && CurrencyModel.Gold >= Cost && SkillModel.HasUnlockableSkill(); }
    }

    public static IReadOnlyList<string> GetCandidates()
    {
        return SkillModel.GetUnlockCandidates(Config.ChoiceCount);
    }

    public static bool TryUnlock(string skillId)
    {
        if (!CanUnlock || !Contains(GetCandidates(), skillId)) { return false; }

        double cost = Cost;

        if (!CurrencyModel.TrySpendGold(cost)) { return false; }

        if (!SkillModel.Add(skillId))
        {
            CurrencyModel.AddGold(cost);
            return false;
        }

        return true;
    }

    private static bool Contains(IReadOnlyList<string> skillIds, string skillId)
    {
        for (int i = 0; i < skillIds.Count; i++)
        {
            if (string.Equals(skillIds[i], skillId, StringComparison.OrdinalIgnoreCase)) { return true; }
        }

        return false;
    }
}
