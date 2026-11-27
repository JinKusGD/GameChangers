using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private readonly Dictionary<Type, UIView> _views = new Dictionary<Type, UIView>();
    private readonly Dictionary<Type, UniTask<UIView>> _loadingViews = new Dictionary<Type, UniTask<UIView>>();

    private UIRoot _uiRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DebugUtility.LogError("[UIManager] 씬에 UIManager가 두 개 이상 있습니다.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        _uiRoot = CreateUIRoot();
    }

    private void OnDestroy()
    {
        if (Instance != this) { return; }

        ReleaseAll();
        Instance = null;
    }

    private UIRoot CreateUIRoot()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(AddressKeys.UIRoot, transform, false);
        GameObject instance = handle.WaitForCompletion();

        if (instance == null)
        {
            DebugUtility.LogError($"[UIManager] Addressable '{AddressKeys.UIRoot}' 을(를) 생성하지 못했습니다.", this);
            Addressables.Release(handle);
            return null;
        }

        
        if (!instance.TryGetComponent<UIRoot>(out var uiRoot))
        {
            DebugUtility.LogError($"[UIManager] '{AddressKeys.UIRoot}' 프리팹에 UIRoot 컴포넌트가 없습니다.", this);
            Addressables.ReleaseInstance(instance);
            return null;
        }

        return uiRoot;
    }

    private Transform GetRoot(UICanvasType canvasType)
    {
        if (_uiRoot == null) { return transform; }

        Transform root = _uiRoot.GetRoot(canvasType);

        if (root == null)
        {
            DebugUtility.LogError($"[UIManager] UIRoot에 {canvasType} 루트가 연결되지 않았습니다.", this);
            return transform;
        }

        return root;
    }

    public async UniTask<T> ShowAsync<T>() where T : UIView
    {
        T view = await GetOrCreateAsync<T>();

        if (view == null) { return null; }

        view.Show();
        return view;
    }

    public void Hide<T>() where T : UIView
    {
        if (TryGet(out T view))
        {
            view.Hide();
        }
    }

    public bool IsShown<T>() where T : UIView
    {
        if (TryGet(out T view))
        {
            return view.IsShown;
        }

        return false;
    }

    public bool TryGet<T>(out T view) where T : UIView
    {
        view = null;

        if (!_views.TryGetValue(typeof(T), out UIView cachedView)) { return false; }

        if (cachedView == null)
        {
            _views.Remove(typeof(T));
            return false;
        }

        view = (T)cachedView;
        return true;
    }

    private async UniTask<T> GetOrCreateAsync<T>() where T : UIView
    {
        if (TryGet(out T cachedView)) { return cachedView; }

        Type type = typeof(T);

        if (!_loadingViews.TryGetValue(type, out UniTask<UIView> loading))
        {
            loading = CreateAsync(type).Preserve();
            _loadingViews.Add(type, loading);
        }

        UIView view = await loading;

        return view as T;
    }

    private async UniTask<UIView> CreateAsync(Type type)
    {
        string address = AddressKeys.UIPrefix + type.Name;
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, transform, false);
        GameObject instance = null;

        try
        {
            instance = await handle.ToUniTask();
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[UIManager] Addressable '{address}' 을(를) 생성하지 못했습니다: {e.Message}", this);
        }
        finally
        {
            _loadingViews.Remove(type);
        }

        if (instance == null)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return null;
        }

        if (this == null)
        {
            Addressables.ReleaseInstance(instance);
            return null;
        }

        UIView view = instance.GetComponent(type) as UIView;

        if (view == null)
        {
            DebugUtility.LogError($"[UIManager] '{address}' 프리팹에 {type.Name} 컴포넌트가 없습니다.", this);
            Addressables.ReleaseInstance(instance);
            return null;
        }

        instance.SetActive(false);
        instance.transform.SetParent(GetRoot(view.CanvasType), false);

        if (!view.Create())
        {
            DebugUtility.LogError($"[UIManager] {type.Name} 초기화에 실패해 생성을 취소합니다.", this);
            Addressables.ReleaseInstance(instance);
            return null;
        }

        _views.Add(type, view);

        return view;
    }

    private void ReleaseAll()
    {
        foreach (UIView view in _views.Values)
        {
            if (view != null)
            {
                Addressables.ReleaseInstance(view.gameObject);
            }
        }

        _views.Clear();

        if (_uiRoot != null)
        {
            Addressables.ReleaseInstance(_uiRoot.gameObject);
            _uiRoot = null;
        }
    }
}
