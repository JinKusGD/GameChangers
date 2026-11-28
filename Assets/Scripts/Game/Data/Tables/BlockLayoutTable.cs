using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BlockLayoutTable
{
    private const string Tag = "BlockLayoutTable";

    private readonly Dictionary<string, BlockLayout> _layoutsById = new Dictionary<string, BlockLayout>(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _errors = new List<string>();

    public async UniTask LoadAsync(CancellationToken token)
    {
        _layoutsById.Clear();

        AsyncOperationHandle<IList<Texture2D>> handle = Addressables.LoadAssetsAsync<Texture2D>(AddressKeys.Labels.BlockLayout, null);

        try
        {
            IList<Texture2D> textures = await LoadTexturesAsync(handle, token);

            if (textures == null) { return; }

            AddLayouts(textures);
        }
        finally
        {
            ReleaseHandle(handle);
        }
    }

    public BlockLayout GetById(string id)
    {
        if (!TryGetById(id, out BlockLayout layout))
        {
            DebugUtility.LogError($"[{Tag}] Id '{id}' 레이아웃이 없습니다.");
            return null;
        }

        return layout;
    }

    public bool TryGetById(string id, out BlockLayout layout)
    {
        layout = null;

        if (string.IsNullOrEmpty(id)) { return false; }

        return _layoutsById.TryGetValue(id, out layout);
    }

    public void CheckBlockKeys(BlockTable blockTable)
    {
        foreach (BlockLayout layout in _layoutsById.Values)
        {
            for (int blockKey = BlockKey.Min; blockKey <= BlockKey.Max; blockKey++)
            {
                if (!layout.UsesBlockKey(blockKey) || blockTable.TryGetByKey(blockKey, out _)) { continue; }

                DebugUtility.LogError($"[{Tag}] '{layout.Id}' 레이아웃이 쓰는 블록 키 {blockKey}번 블록이 없습니다. 그 칸은 비게 됩니다.");
            }
        }
    }

    private static async UniTask<IList<Texture2D>> LoadTexturesAsync(AsyncOperationHandle<IList<Texture2D>> handle, CancellationToken token)
    {
        try
        {
            return await handle.ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[{Tag}] '{AddressKeys.Labels.BlockLayout}' 레이블 이미지를 불러오지 못했습니다: {e.Message}");
            return null;
        }
    }

    private void AddLayouts(IList<Texture2D> textures)
    {
        foreach (Texture2D texture in textures)
        {
            TryAddLayout(texture);
        }

        if (_layoutsById.Count == 0)
        {
            DebugUtility.LogError($"[{Tag}] 사용할 수 있는 레이아웃이 하나도 없습니다. (불러온 이미지 {textures.Count}개)");
        }
    }

    private void TryAddLayout(Texture2D texture)
    {
        if (texture == null)
        {
            DebugUtility.LogError($"[{Tag}] null 이미지 제외됨");
            return;
        }

        try
        {
            AddLayout(texture);
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[{Tag}] '{texture.name}' 제외됨: {e.Message}");
        }
    }

    private void AddLayout(Texture2D texture)
    {
        _errors.Clear();

        BlockLayout layout = BlockLayout.Create(texture, _errors);

        if (layout == null)
        {
            DebugUtility.LogError($"[{Tag}] '{texture.name}' 제외됨: {string.Join(", ", _errors)}");
            return;
        }

        if (!_layoutsById.TryAdd(layout.Id, layout))
        {
            DebugUtility.LogError($"[{Tag}] '{layout.Id}' 제외됨: Id 중복");
        }
    }

    private static void ReleaseHandle(AsyncOperationHandle<IList<Texture2D>> handle)
    {
        if (!handle.IsValid()) { return; }

        Addressables.Release(handle);
    }
}
