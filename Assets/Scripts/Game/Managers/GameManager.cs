using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string Tag = "GameManager";
    private const int TargetFrameRate = 60;

    public static GameManager Instance { get; private set; }

    private GameData _data;

    public bool IsLoaded => _data != null;
    public StageSaveData StageSave => _data.StageSave;

    public double OfflineSeconds { get; private set; }

    public event Action ChapterChanged;
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
            DebugUtility.Log($"[{Tag}] 오프라인 시간: {OfflineSeconds:F0}초");
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
        StageModel stageModel = ModelManager.Instance.StageModel;
        int previousChapter = stageModel.Chapter;

        stageModel.ChangeStage(chapter, stage);
        _data.StageSave = new StageSaveData();

        if (stageModel.Chapter != previousChapter)
        {
            ChapterChanged?.Invoke();
        }

        StageChanged?.Invoke();
    }

    private void ApplyToModels()
    {
        ApplyCurrencyToModel();
        ApplyStageToModel();
        ApplySkillToModel();
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

    private void ApplySkillToModel()
    {
        SkillModel skillModel = ModelManager.Instance.SkillModel;

        int maxSkillCount = _data.MaxSkillCount > 0 ? _data.MaxSkillCount : DataManager.Instance.Config.SkillUnlock.InitialMaxCount;

        skillModel.Load(_data.SkillLevels, maxSkillCount);

        if (DataManager.Instance.Skills.TryGetById(SkillIds.SwordAttack, out SkillData swordAttackData))
        {
            ModelManager.Instance.SwordAttackModel.Load(swordAttackData);
        }
    }

    private void CollectFromModels()
    {
        CollectCurrencyFromModel();
        CollectStageFromModel();
        CollectSkillFromModel();
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

    private void CollectSkillFromModel()
    {
        SkillModel skillModel = ModelManager.Instance.SkillModel;

        _data.SkillLevels = skillModel.CreateSaveData();
        _data.MaxSkillCount = skillModel.MaxOwnedCount;
    }

    private double CalculateOfflineSeconds()
    {
        if (_data.LastSaveUtcTicks <= 0) { return 0; }

        DateTime lastSaveTime = new DateTime(_data.LastSaveUtcTicks, DateTimeKind.Utc);

        return Math.Clamp((DateTime.UtcNow - lastSaveTime).TotalSeconds, 0, DataManager.Instance.Config.Offline.MaxSeconds);
    }
}
