using UnityEngine;

public sealed class DataManager
{
    private static DataManager _instance;

    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataManager();
            }

            return _instance;
        }
    }

    public BlockTable Blocks { get; } = new BlockTable();
    public BlockLayoutTable BlockLayouts { get; } = new BlockLayoutTable();
    public StageTable Stages { get; } = new StageTable();

    private DataManager() { }

    public void Load()
    {
        Blocks.Load();
        BlockLayouts.Load();
        Stages.Load();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }
}
