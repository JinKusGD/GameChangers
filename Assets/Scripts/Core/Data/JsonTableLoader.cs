using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonTableLoader
{
    public static List<T> Load<T>(string resourcePath, string tag, Func<T, string> describe) where T : class, IValidatable
    {
        List<T> rows = new List<T>();

        TextAsset asset = Resources.Load<TextAsset>(resourcePath);

        if (asset == null)
        {
            DebugUtility.LogError($"[{tag}] Resources/{resourcePath}.json 을 찾을 수 없습니다.");
            return rows;
        }

        List<T> parsedRows;

        try
        {
            parsedRows = JsonConvert.DeserializeObject<List<T>>(asset.text, CreateSerializerSettings(tag));
        }
        catch (JsonException e)
        {
            DebugUtility.LogError($"[{tag}] JSON 읽기 실패: {e.Message}");
            return rows;
        }

        if (parsedRows == null)
        {
            DebugUtility.LogError($"[{tag}] JSON이 비어 있습니다.");
            return rows;
        }

        foreach (T row in parsedRows)
        {
            if (row == null) { continue; }

            List<string> errors = row.Validate();

            if (errors.Count > 0)
            {
                DebugUtility.LogError($"[{tag}] {describe(row)} 제외됨: {string.Join(", ", errors)}");
                continue;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static JsonSerializerSettings CreateSerializerSettings(string tag)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();

        settings.Error = (sender, e) =>
        {
            DebugUtility.LogError($"[{tag}] {e.ErrorContext.Path} 값을 읽지 못해 기본값을 씁니다: {e.ErrorContext.Error.Message}");
            e.ErrorContext.Handled = true;
        };

        return settings;
    }
}
