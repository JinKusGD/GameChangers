using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int Chapter = 1;
    public int Stage = 1;
    public StageSaveData StageSave = new StageSaveData();

    public double Gold;

    public long LastSaveUtcTicks;

    public List<SkillSaveData> SkillLevels;
    public int MaxSkillCount;

    public void Normalize()
    {
        Chapter = Math.Max(1, Chapter);
        Stage = Math.Max(1, Stage);

        if (StageSave == null)
        {
            StageSave = new StageSaveData();
        }

        if (SkillLevels == null)
        {
            SkillLevels = new List<SkillSaveData> { new SkillSaveData { Id = SkillIds.SwordAttack, Level = 1 } };
        }
    }
}
