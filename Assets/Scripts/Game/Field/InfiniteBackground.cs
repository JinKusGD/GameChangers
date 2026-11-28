using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class InfiniteBackground : MonoBehaviour
{
    private const string Tag = "InfiniteBackground";
    private const int PanelCount = 4;

    [SerializeField, Min(1)] private int _panelSize = 48;

    [SerializeField] private Tilemap[] _panels = new Tilemap[PanelCount];

    private readonly Vector2Int[] _panelOrigins = new Vector2Int[PanelCount];
    private readonly Vector2Int[] _targetOrigins = new Vector2Int[PanelCount];
    private readonly bool[] _isTargetTaken = new bool[PanelCount];

    private Camera _camera;
    private TileBase _tile;
    private TileBase[] _fillBuffer;
    private int _cellsPerPanel;
    private float _panelWorldSize;
    private AsyncOperationHandle<TileBase> _tileHandle;
    private int _requestId;

    private void Awake()
    {
        if (!HasAllPanels())
        {
            enabled = false;
            return;
        }

        float cellSize = GetComponent<Grid>().cellSize.x;

        _cellsPerPanel = Mathf.Max(1, Mathf.RoundToInt(_panelSize / cellSize));
        _panelWorldSize = _cellsPerPanel * cellSize;
        _fillBuffer = new TileBase[_cellsPerPanel * _cellsPerPanel];

        for (int i = 0; i < PanelCount; i++)
        {
            _panelOrigins[i] = new Vector2Int(int.MinValue, int.MinValue);
        }
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void OnDestroy()
    {
        ReleaseTile();
    }

    private void LateUpdate()
    {
        if (_camera == null || _tile == null) { return; }

        UpdateTargetOrigins();
        AssignPanels();
    }

    public async UniTask ApplyStageAsync(int stage, CancellationToken token)
    {
        if (!enabled) { return; }

        int requestId = ++_requestId;

        if (!DataManager.Instance.Stages.TryGet(stage, out StageData data) || string.IsNullOrEmpty(data.Background))
        {
            ClearTiles();
            return;
        }

        string address = AddressKeys.Tiles.BackgroundPrefix + data.Background;
        AsyncOperationHandle<TileBase> handle = Addressables.LoadAssetAsync<TileBase>(address);
        TileBase tile;

        try
        {
            tile = await handle.ToUniTask(cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            ReleaseHandle(handle);
            throw;
        }
        catch (Exception exception)
        {
            DebugUtility.LogWarning($"[{Tag}] {stage}스테이지 배경 타일 '{address}' 로드 실패. {exception.Message}");
            ReleaseHandle(handle);
            return;
        }

        if (requestId != _requestId || tile == null)
        {
            ReleaseHandle(handle);
            return;
        }

        ClearTiles();
        _tileHandle = handle;
        _tile = tile;
        Array.Fill(_fillBuffer, tile);
    }

    private bool HasAllPanels()
    {
        if (_panels == null || _panels.Length != PanelCount)
        {
            DebugUtility.LogError($"[{Tag}] {nameof(_panels)}에 타일맵 {PanelCount}개를 연결해야 합니다.", this);
            return false;
        }

        bool hasAllReferences = true;

        for (int i = 0; i < PanelCount; i++)
        {
            hasAllReferences &= UnityUtility.CheckReference(_panels[i], $"{nameof(_panels)}[{i}]", this);
        }

        return hasAllReferences;
    }

    private void UpdateTargetOrigins()
    {
        Vector2 cameraPosition = _camera.transform.position;
        int cornerX = Mathf.RoundToInt(cameraPosition.x / _panelWorldSize);
        int cornerY = Mathf.RoundToInt(cameraPosition.y / _panelWorldSize);

        for (int i = 0; i < PanelCount; i++)
        {
            int panelX = cornerX + (i % 2 == 0 ? -1 : 0);
            int panelY = cornerY + (i < 2 ? -1 : 0);

            _targetOrigins[i] = new Vector2Int(panelX * _cellsPerPanel, panelY * _cellsPerPanel);
            _isTargetTaken[i] = false;
        }
    }

    private void AssignPanels()
    {
        for (int i = 0; i < PanelCount; i++)
        {
            int target = Array.IndexOf(_targetOrigins, _panelOrigins[i]);

            if (target >= 0)
            {
                _isTargetTaken[target] = true;
            }
            else
            {
                _panelOrigins[i] = new Vector2Int(int.MinValue, int.MinValue);
            }
        }

        for (int i = 0; i < PanelCount; i++)
        {
            if (_panelOrigins[i].x != int.MinValue) { continue; }

            int target = Array.IndexOf(_isTargetTaken, false);
            _isTargetTaken[target] = true;

            FillPanel(i, _targetOrigins[target]);
        }
    }

    private void FillPanel(int index, Vector2Int origin)
    {
        Tilemap panel = _panels[index];

        panel.ClearAllTiles();
        panel.SetTilesBlock(new BoundsInt(origin.x, origin.y, 0, _cellsPerPanel, _cellsPerPanel, 1), _fillBuffer);
        _panelOrigins[index] = origin;
    }

    private void ClearTiles()
    {
        for (int i = 0; i < PanelCount; i++)
        {
            _panels[i].ClearAllTiles();
            _panelOrigins[i] = new Vector2Int(int.MinValue, int.MinValue);
        }

        _tile = null;
        ReleaseTile();
    }

    private void ReleaseTile()
    {
        ReleaseHandle(_tileHandle);
        _tileHandle = default;
    }

    private static void ReleaseHandle(AsyncOperationHandle<TileBase> handle)
    {
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
    }
}
