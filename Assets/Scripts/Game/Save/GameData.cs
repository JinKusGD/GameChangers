using System;

[Serializable]
public class GameData
{
    public int Chapter = 1;
    public int Stage = 1;
    public double Gold;
    public long LastSaveUtcTicks;

    public StageSaveData StageSave = new StageSaveData();

    public void Normalize()
    {
        Chapter = Math.Max(1, Chapter);
        Stage = Math.Max(1, Stage);

        if (StageSave == null)
        {
            StageSave = new StageSaveData();
        }
    }
}
