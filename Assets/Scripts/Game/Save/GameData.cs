using System;

public enum UpgradeType
{
    Damage,
    MoveSpeed
}

[Serializable]
public class GameData
{
    public int chapter = 1;
    public int stage = 1;

    public double gold;

    public int dummyWeaponLevel = 1;
    public int speedLevel;

    public long lastSaveUtcTicks;

    public int GetUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                return dummyWeaponLevel;
            case UpgradeType.MoveSpeed:
                return speedLevel;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    public void Upgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                dummyWeaponLevel++;
                break;
            case UpgradeType.MoveSpeed:
                speedLevel++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
