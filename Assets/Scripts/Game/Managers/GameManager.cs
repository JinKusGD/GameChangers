using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int TargetFrameRate = 60;
    private const double MaxOfflineSeconds = 3600;

    public static GameManager Instance { get; private set; }

    private GameData _data;

    public bool IsLoaded => _data != null;
    public StageSaveData StageSave => _data.StageSave;

    public double OfflineSeconds { get; private set; }

    public event Action StageChanged;
    public event Action Saving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = TargetFrameRate;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void LoadSaveData()
    {
        _data = SaveStorage.Load();
        _data.Normalize();

        ApplyToModels();
        OfflineSeconds = CalculateOfflineSeconds();

        if (OfflineSeconds > 0)
        {
            DebugUtility.Log($"[GameManager] 오프라인 시간: {OfflineSeconds:F0}초");
        }
    }

    public void Save()
    {
        if (!IsLoaded) { return; }

        Saving?.Invoke();

        CollectFromModels();
        _data.LastSaveUtcTicks = DateTime.UtcNow.Ticks;

        SaveStorage.Write(_data);
    }

    public void ChangeStage(int chapter, int stage)
    {
        ModelManager.Instance.StageModel.ChangeStage(chapter, stage);
        _data.StageSave = new StageSaveData();

        StageChanged?.Invoke();
    }

    private void ApplyToModels()
    {
        ApplyCurrencyToModel();
        ApplyStageToModel();
    }

    private void ApplyCurrencyToModel()
    {
        CurrencyModel currencyModel = ModelManager.Instance.CurrencyModel;

        currencyModel.Load(_data.Gold);
    }

    private void ApplyStageToModel()
    {
        StageModel stageModel = ModelManager.Instance.StageModel;

        stageModel.Load(_data.Chapter, _data.Stage);
    }

    private void CollectFromModels()
    {
        CollectCurrencyFromModel();
        CollectStageFromModel();
    }

    private void CollectCurrencyFromModel()
    {
        CurrencyModel currencyModel = ModelManager.Instance.CurrencyModel;

        _data.Gold = currencyModel.Gold;
    }

    private void CollectStageFromModel()
    {
        StageModel stageModel = ModelManager.Instance.StageModel;

        _data.Chapter = stageModel.Chapter;
        _data.Stage = stageModel.Stage;
    }

    private double CalculateOfflineSeconds()
    {
        if (_data.LastSaveUtcTicks <= 0) { return 0; }

        DateTime lastSaveTime = new DateTime(_data.LastSaveUtcTicks, DateTimeKind.Utc);

        return Math.Clamp((DateTime.UtcNow - lastSaveTime).TotalSeconds, 0, MaxOfflineSeconds);
    }
}
