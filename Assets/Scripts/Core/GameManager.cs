using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string SaveKey = "ClearCat.Save";
    private const float AutoSaveInterval = 10f;

    public GameData Data { get; private set; }

    public StageTemplate CurrentTemplate { get; private set; }
    public IReadOnlyList<StageTemplateSlot> StagePlacements { get; private set; } = Array.Empty<StageTemplateSlot>();

    public event Action StagePlacementsChanged;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, null);

        Data = string.IsNullOrEmpty(json) ? new GameData() : JsonUtility.FromJson<GameData>(json);

        Data.chapter = Mathf.Max(1, Data.chapter);
        Data.stage = Mathf.Max(1, Data.stage);

        RefreshStagePlacements();

        if (Data.lastSaveUtcTicks > 0)
        {
            double elapsed = (DateTime.UtcNow - new DateTime(Data.lastSaveUtcTicks, DateTimeKind.Utc)).TotalSeconds;
            elapsed = Math.Clamp(elapsed, 0, 3600);

            Debug.Log($"{elapsed} 만큼 오프라인");
        }
    }

    public void Save()
    {
        Data.lastSaveUtcTicks = DateTime.UtcNow.Ticks;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    private void RefreshStagePlacements()
    {
        string mapId = GetMapId(Data.chapter, Data.stage);

        StageTemplate template = StageTemplateRepository.GetById(mapId);

        CurrentTemplate = template;
        StagePlacements = template != null ? template.GetSlots() : Array.Empty<StageTemplateSlot>();

        StagePlacementsChanged?.Invoke();
    }

    private string GetMapId(int chapter, int stage)
    {
        // TODO: 스테이지 정보 테이블에서 (chapter, stage)의 맵 Id를 받아오기
        return "heart";
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
