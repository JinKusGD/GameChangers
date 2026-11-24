using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const string SaveKey = "ClearCat.Save";
    private const float AutoSaveInterval = 10f;

    [SerializeField] private TMP_Text TMP_Text;

    public GameData Data { get; private set; }

    private void Awake()
    {
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
        Debug.Log("오브젝트 재배치");
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
