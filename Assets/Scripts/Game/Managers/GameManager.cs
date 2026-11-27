using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string SaveKey = "ClearCat.Save";
    private const int TargetFrameRate = 60;
    private const double MaxOfflineSeconds = 3600;

    public static GameManager Instance { get; private set; }

    public GameData Data { get; private set; }

    public int Chapter => Data.Chapter;
    public int Stage => Data.Stage;

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

    public void AddGold(double amount)
    {
        if (amount <= 0) { return; }

        Data.Gold += amount;
    }

    public void Save()
    {
        if (Data == null) { return; }

        Saving?.Invoke();

        Data.LastSaveUtcTicks = DateTime.UtcNow.Ticks;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    public void ChangeStage(int chapter, int stage)
    {
        Data.Chapter = Mathf.Max(1, chapter);
        Data.Stage = Mathf.Max(1, stage);
        Data.StageSave = new StageSaveData();

        StageChanged?.Invoke();
    }

    public void LoadSaveData()
    {
        Data = ReadSaveData();

        Data.Chapter = Mathf.Max(1, Data.Chapter);
        Data.Stage = Mathf.Max(1, Data.Stage);

        if (Data.StageSave == null)
        {
            Data.StageSave = new StageSaveData();
        }

        if (Data.LastSaveUtcTicks > 0)
        {
            DateTime lastSaveTime = new DateTime(Data.LastSaveUtcTicks, DateTimeKind.Utc);
            double offlineSeconds = Math.Clamp((DateTime.UtcNow - lastSaveTime).TotalSeconds, 0, MaxOfflineSeconds);

            DebugUtility.Log($"[GameManager] 오프라인 시간: {offlineSeconds:F0}초");
        }
    }

    private static GameData ReadSaveData()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);

        if (string.IsNullOrEmpty(json))
        {
            return new GameData();
        }

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data != null)
            {
                return data;
            }
        }
        catch (ArgumentException e)
        {
            DebugUtility.LogWarning($"[GameManager] 저장 데이터를 읽지 못해 새로 시작합니다: {e.Message}");
        }

        return new GameData();
    }
}
