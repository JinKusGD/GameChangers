using Newtonsoft.Json;
using UnityEngine;

public static class SaveStorage
{
    private const string Tag = "SaveStorage";
    private const string SaveKey = "ClearCat.Save";

    private static readonly JsonSerializerSettings SerializerSettings = CreateSerializerSettings();

    public static GameData Load()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);

        if (string.IsNullOrEmpty(json))
        {
            return new GameData();
        }

        try
        {
            GameData data = JsonConvert.DeserializeObject<GameData>(json, SerializerSettings);

            if (data != null)
            {
                return data;
            }
        }
        catch (JsonException e)
        {
            DebugUtility.LogWarning($"[{Tag}] 저장 데이터를 읽지 못해 새로 시작합니다: {e.Message}");
        }

        return new GameData();
    }

    public static void Write(GameData data)
    {
        string jsonText = JsonConvert.SerializeObject(data, SerializerSettings);
        PlayerPrefs.SetString(SaveKey, jsonText);
        PlayerPrefs.Save();
    }

    private static JsonSerializerSettings CreateSerializerSettings()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();

        settings.Converters.Add(new Vector2JsonConverter());

        settings.Error = (sender, e) =>
        {
            DebugUtility.LogWarning($"[{Tag}] {e.ErrorContext.Path} 값을 읽지 못해 기본값을 씁니다: {e.ErrorContext.Error.Message}");
            e.ErrorContext.Handled = true;
        };

        return settings;
    }
}
