using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class JsonTableLoader
{
    public static async UniTask<List<T>> LoadAsync<T>(string address, string tag, Func<T, string> describe, CancellationToken token) where T : class, IValidatable
    {
        List<T> rows = new List<T>();

        string json = await LoadTextAsync(address, tag, token);

        if (json == null)
        {
            return rows;
        }

        List<T> parsedRows;

        try
        {
            parsedRows = JsonConvert.DeserializeObject<List<T>>(json, CreateSerializerSettings(tag));
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

    public static async UniTask<T> LoadObjectAsync<T>(string address, string tag, CancellationToken token) where T : class, IValidatable
    {
        string json = await LoadTextAsync(address, tag, token);

        if (json == null) { return null; }

        T data;

        try
        {
            data = JsonConvert.DeserializeObject<T>(json, CreateSerializerSettings(tag));
        }
        catch (JsonException e)
        {
            DebugUtility.LogError($"[{tag}] JSON 읽기 실패: {e.Message}");
            return null;
        }

        if (data == null)
        {
            DebugUtility.LogError($"[{tag}] JSON이 비어 있습니다.");
            return null;
        }

        List<string> errors = data.Validate();

        if (errors.Count > 0)
        {
            DebugUtility.LogError($"[{tag}] 설정 오류: {string.Join(", ", errors)}");
            return null;
        }

        return data;
    }

    private static async UniTask<string> LoadTextAsync(string address, string tag, CancellationToken token)
    {
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);

        try
        {
            TextAsset asset = await handle.ToUniTask(cancellationToken: token);
            return asset != null ? asset.text : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[{tag}] Addressable '{address}' 을(를) 불러오지 못했습니다: {e.Message}");
            return null;
        }
        finally
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
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
