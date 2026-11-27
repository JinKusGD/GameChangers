using System;

public enum UpgradeType
{
    Damage,
    MoveSpeed
}

[Serializable]
public class GameData
{
    public int Chapter = 1;
    public int Stage = 1;

    public double Gold;

    public long LastSaveUtcTicks;

    public StageSaveData StageSave = new StageSaveData();

    public int GetUpgrade(UpgradeType type)
    {
        switch (type)
        { 
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    public void Upgrade(UpgradeType type)
    {
        switch (type)
        {
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
