using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private const string Tag = "UIManager";

    public static UIManager Instance { get; private set; }

    private readonly Dictionary<Type, UIView> _views = new Dictionary<Type, UIView>();
    private readonly Dictionary<Type, UniTask<UIView>> _loadingViews = new Dictionary<Type, UniTask<UIView>>();
    private readonly Dictionary<Type, int> _ownerSceneHandles = new Dictionary<Type, int>();
    private readonly List<Type> _releaseBuffer = new List<Type>();

    private UIRoot _uiRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _uiRoot = CreateUIRoot();

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (Instance != this) { return; }

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        ReleaseAll();
        Instance = null;
    }

    private UIRoot CreateUIRoot()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(AddressKeys.UI.Root, transform, false);
        GameObject instance = handle.WaitForCompletion();

        if (instance == null)
        {
            DebugUtility.LogError($"[{Tag}] Addressable '{AddressKeys.UI.Root}' 을(를) 생성하지 못했습니다.", this);
            Addressables.Release(handle);
            return null;
        }

        
        if (!instance.TryGetComponent<UIRoot>(out var uiRoot))
        {
            DebugUtility.LogError($"[{Tag}] '{AddressKeys.UI.Root}' 프리팹에 UIRoot 컴포넌트가 없습니다.", this);
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
            DebugUtility.LogError($"[{Tag}] UIRoot에 {canvasType} 루트가 연결되지 않았습니다.", this);
            return transform;
        }

        return root;
    }

    public async UniTask<bool> PreloadAsync<T>() where T : UIView
    {
        T view = await GetOrCreateAsync<T>();

        return view != null;
    }

    public async UniTask<T> ShowAsync<T>() where T : UIView
    {
        T view = await GetOrCreateAsync<T>();

        if (view == null) { return null; }

        view.Show();
        RegisterOwnerScene(typeof(T), view);
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
            _ownerSceneHandles.Remove(typeof(T));
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
        string address = AddressKeys.UI.Prefix + type.Name;
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, transform, false);
        GameObject instance = null;

        try
        {
            instance = await handle.ToUniTask();
        }
        catch (Exception e)
        {
            DebugUtility.LogError($"[{Tag}] Addressable '{address}' 을(를) 생성하지 못했습니다: {e.Message}", this);
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
            DebugUtility.LogError($"[{Tag}] '{address}' 프리팹에 {type.Name} 컴포넌트가 없습니다.", this);
            Addressables.ReleaseInstance(instance);
            return null;
        }

        instance.SetActive(false);
        instance.transform.SetParent(GetRoot(view.CanvasType), false);

        if (!view.Create())
        {
            DebugUtility.LogError($"[{Tag}] {type.Name} 초기화에 실패해 생성을 취소합니다.", this);
            Addressables.ReleaseInstance(instance);
            return null;
        }

        _views.Add(type, view);

        return view;
    }

    private void RegisterOwnerScene(Type type, UIView view)
    {
        if (view.Lifetime != UILifetime.Scene) { return; }

        if (_ownerSceneHandles.ContainsKey(type)) { return; }

        _ownerSceneHandles.Add(type, SceneManager.GetActiveScene().handle);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        _releaseBuffer.Clear();

        foreach (KeyValuePair<Type, int> owner in _ownerSceneHandles)
        {
            if (owner.Value == scene.handle)
            {
                _releaseBuffer.Add(owner.Key);
            }
        }

        foreach (Type type in _releaseBuffer)
        {
            Release(type);
        }
    }

    private void Release(Type type)
    {
        _ownerSceneHandles.Remove(type);

        if (!_views.TryGetValue(type, out UIView view)) { return; }

        _views.Remove(type);

        if (view == null) { return; }

        view.Hide();
        Addressables.ReleaseInstance(view.gameObject);
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
        _ownerSceneHandles.Clear();

        if (_uiRoot != null)
        {
            Addressables.ReleaseInstance(_uiRoot.gameObject);
            _uiRoot = null;
        }
    }
}
