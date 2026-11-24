using System;

public enum UpgradeType
{
    Damage
}

[Serializable]
public class GameData
{
    public int chapter;
    public int stage;

    public double gold;
 
    public int dummyWeaponLevel;  

    public int speedLevel;

    public long lastSaveUtcTicks;

    public GameData()
    {
        chapter = 1;
        stage = 1;
        gold = 0;
        dummyWeaponLevel = 1;
    }

    public int GetUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                return dummyWeaponLevel;
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
            default: throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
