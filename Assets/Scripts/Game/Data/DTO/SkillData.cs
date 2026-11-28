using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

public class SkillData : IValidatable
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Prefab { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public SkillTargetMode TargetMode { get; set; } = SkillTargetMode.PlayerTarget;
    public float Cooldown { get; set; } = 1f;
    public float Range { get; set; } = 1f;
    public double AttackPowerMultiplier { get; set; } = 1;

    public int MaxLevel { get; set; } = 1;
    public SkillCostData Cost { get; set; } = new SkillCostData();
    public SkillDamageData Damage { get; set; } = new SkillDamageData();
    public List<SkillMilestoneData> Milestones { get; set; } = new List<SkillMilestoneData>();
    public SkillMasteryData Mastery { get; set; } = new SkillMasteryData();

    public string PrefabAddress
    {
        get { return AddressKeys.Prefabs.SkillPrefix + Prefab; }
    }

    public string IconAddress
    {
        get { return AddressKeys.Sprites.SkillIconPrefix + Icon; }
    }

    [JsonIgnore]
    public Sprite IconSprite { get; internal set; }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        Cost ??= new SkillCostData();
        Damage ??= new SkillDamageData();
        Milestones ??= new List<SkillMilestoneData>();
        Mastery ??= new SkillMasteryData();

        RemoveNullMilestones();
        Milestones.Sort(CompareMilestoneLevel);
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Id)) { errors.Add("Id가 비어 있습니다."); }
        if (string.IsNullOrWhiteSpace(Name)) { errors.Add("Name이 비어 있습니다."); }
        if (string.IsNullOrWhiteSpace(Prefab)) { errors.Add("Prefab이 비어 있습니다."); }

        if (MaxLevel < 1) { errors.Add($"MaxLevel은 1 이상이어야 합니다. ({MaxLevel})"); }

        ValidateCombat(errors);

        AddSectionErrors(errors, nameof(Cost), Cost);
        AddSectionErrors(errors, nameof(Damage), Damage);
        AddSectionErrors(errors, nameof(Mastery), Mastery);

        ValidateMilestones(errors);

        return errors;
    }

    public bool IsMaxLevel(int level)
    {
        return level >= MaxLevel;
    }

    public double GetUpgradeCost(int level)
    {
        return Cost.Evaluate(level);
    }

    public int GetRequiredUseCount(int level)
    {
        return Mastery.GetRequiredCount(level);
    }

    public double GetDamageMultiplier(int level)
    {
        double multiplier = Damage.Evaluate(level);

        foreach (SkillMilestoneData milestone in Milestones)
        {
            if (milestone.Level > level) { break; }

            multiplier *= milestone.DamageMultiplier;
        }

        return multiplier;
    }

    private void ValidateCombat(List<string> errors)
    {
        if (!Enum.IsDefined(typeof(SkillTargetMode), TargetMode))
        {
            errors.Add($"TargetMode가 올바르지 않습니다. ({TargetMode})");
        }

        if (!float.IsFinite(Cooldown) || Cooldown <= 0f)
        {
            errors.Add($"Cooldown은 0보다 큰 유한한 숫자여야 합니다. ({Cooldown})");
        }

        if (!float.IsFinite(Range) || Range <= 0f)
        {
            errors.Add($"Range는 0보다 큰 유한한 숫자여야 합니다. ({Range})");
        }

        if (!double.IsFinite(AttackPowerMultiplier) || AttackPowerMultiplier <= 0)
        {
            errors.Add($"AttackPowerMultiplier는 0보다 큰 유한한 숫자여야 합니다. ({AttackPowerMultiplier})");
        }
    }

    private void RemoveNullMilestones()
    {
        for (int i = Milestones.Count - 1; i >= 0; i--)
        {
            if (Milestones[i] == null)
            {
                Milestones.RemoveAt(i);
            }
        }
    }

    private static int CompareMilestoneLevel(SkillMilestoneData skillMilestoneData1, SkillMilestoneData skillMilestoneData2)
    {
        return skillMilestoneData1.Level.CompareTo(skillMilestoneData2.Level);
    }

    private void ValidateMilestones(List<string> errors)
    {
        for (int i = 0; i < Milestones.Count; i++)
        {
            SkillMilestoneData milestone = Milestones[i];

            string section = $"{nameof(Milestones)}[Level {milestone.Level}]";

            if (milestone.Level < 2 || milestone.Level > MaxLevel)
            {
                errors.Add($"{section}: Level은 2 ~ MaxLevel이어야 합니다.");
            }

            if (i > 0 && Milestones[i - 1].Level == milestone.Level)
            {
                errors.Add($"{section}: Level이 중복됩니다.");
            }

            AddSectionErrors(errors, section, milestone);
        }
    }

    private static void AddSectionErrors(List<string> errors, string section, IValidatable target)
    {
        foreach (string error in target.Validate())
        {
            errors.Add($"{section}.{error}");
        }
    }
}
