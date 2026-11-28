using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockField : MonoBehaviour
{
    private const string Tag = "BlockField";
    private const float BlockWorldSize = 1f;

    [SerializeField] private Block _blockPrefab;
    [SerializeField] private Transform _cellRoot;

    private StageBlockSet _stageBlocks = StageBlockSet.Empty;

    private BlockGrid _grid = BlockGrid.Empty;
    private BlockDamageResolver _damageResolver;
    private BlockFieldSaveHandler _saveHandler;

    private BlockLayout _layout;
    private GameManager _gameManager;

    private bool _isAllBlocksBrokenRaised;

    public int AliveCount => _grid.AliveCount;
    public int GridSize => _grid.Size;

    public event Action AllBlocksBroken;
    public event Action BlocksChanged;

    private void Awake()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_blockPrefab, nameof(_blockPrefab), this);
        hasAllReferences &= UnityUtility.CheckReference(_cellRoot, nameof(_cellRoot), this);

        if (!hasAllReferences)
        {
            enabled = false;
            return;
        }

        CreateGrid();

        _damageResolver = new BlockDamageResolver(_grid);
        _saveHandler = new BlockFieldSaveHandler(_grid);
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

        if (_gameManager == null)
        {
            DebugUtility.LogError($"[{Tag}] GameManager가 없습니다.", this);
            return;
        }

        _gameManager.StageChanged += OnStageChanged;
        _gameManager.Saving += OnSaving;

        BuildCurrentStage();
        _saveHandler.Load(_gameManager.StageSave, _layout, _stageBlocks);
    }

    private void Update()
    {
        RegenTrait.TickAll(Time.deltaTime);
        CheckAllBlocksBroken();
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
        {
            _gameManager.StageChanged -= OnStageChanged;
            _gameManager.Saving -= OnSaving;
        }

        _grid.Changed -= OnGridChanged;
        _grid.Dispose();
    }

    public void Damage(Block target, double damage)
    {
        if (_damageResolver == null) { return; }

        _damageResolver.Damage(target, damage);
    }

    public Block FindNearest(Vector2 position, float maxDistance = float.PositiveInfinity)
    {
        Vector2 gridPosition = ToGridPosition(position);
        float gridMaxDistance = maxDistance / GetCellWorldSize();

        return _grid.FindNearest(gridPosition, gridMaxDistance);
    }

    public void FindInRadius(Vector2 center, float radius, List<Block> results)
    {
        Vector2 gridCenter = ToGridPosition(center);
        float gridRadius = radius / GetCellWorldSize();

        _grid.FindInRadius(gridCenter, gridRadius, results);
    }

    public bool TryGetCellColor(int column, int row, out Color color)
    {
        color = default;

        if (_layout == null) { return false; }

        Block cell = _grid.GetCell(column, row);

        if (!cell.IsAlive) { return false; }

        color = cell.Color;
        return true;
    }

    public Vector2 GetNormalizedPosition(Vector2 worldPosition)
    {
        return ToGridPosition(worldPosition) / _grid.Size;
    }

    private void OnGridChanged()
    {
        BlocksChanged?.Invoke();
    }

    private void OnStageChanged()
    {
        BuildCurrentStage();
    }

    private void OnSaving()
    {
        _saveHandler.Save(_gameManager.StageSave, _layout, _stageBlocks);
    }

    private void OnCellBroken(Block cell)
    {
        ModelManager.Instance.CurrencyModel.AddGold(cell.Gold);
    }

    private void OnSplitRequested(Block cell, int splitBlockKey)
    {
        StageBlock stageBlock = _stageBlocks.Get(splitBlockKey);

        if (stageBlock == null) { return; }

        _grid.PlaceBlock(cell, stageBlock);
    }

    private void CreateGrid()
    {
        _grid = new BlockGrid(BlockLayout.Size);
        _grid.Changed += OnGridChanged;

        for (int row = 0; row < _grid.Size; row++)
        {
            for (int column = 0; column < _grid.Size; column++)
            {
                Block cell = CreateCell(column, row);
                _grid.SetCell(column, row, cell);
            }
        }
    }

    private Block CreateCell(int column, int row)
    {
        Block cell = Instantiate(_blockPrefab, _cellRoot);
        cell.name = $"Cell_{column}_{row}";
        cell.transform.localPosition = GetCellLocalPosition(column, row);
        cell.transform.localScale = Vector3.one * BlockWorldSize;
        cell.Clear();

        cell.SplitRequested += OnSplitRequested;
        cell.Broken += OnCellBroken;

        return cell;
    }

    private void BuildCurrentStage()
    {
        StageModel stageModel = ModelManager.Instance.StageModel;
        StageInfo stageInfo = DataManager.Instance.GetStageInfo(stageModel.Chapter, stageModel.Stage);

        Build(stageInfo);
    }

    private void Build(StageInfo stageInfo)
    {
        _layout = GetLayout(stageInfo);

        if (_layout == null)
        {
            Clear();
            return;
        }

        _isAllBlocksBrokenRaised = false;

        MoveCellRoot(stageInfo.FieldPosition);
        _stageBlocks = StageBlockSet.Create(DataManager.Instance.Blocks, stageInfo);

        _grid.ResetAlive();

        for (int row = 0; row < _grid.Size; row++)
        {
            for (int column = 0; column < _grid.Size; column++)
            {
                Block cell = _grid.GetCell(column, row);

                int blockKey = _layout.GetBlockKey(column, row);
                StageBlock stageBlock = _stageBlocks.Get(blockKey);

                if (stageBlock == null)
                {
                    cell.Clear();
                    continue;
                }

                _grid.PlaceBlock(cell, stageBlock);
            }
        }

        if (AliveCount == 0)
        {
            DebugUtility.LogError($"[{Tag}] '{_layout.Id}' 레이아웃에 놓을 수 있는 블록이 없습니다. 블록 데이터를 확인하세요.", this);
            Clear();
        }
    }

    private static BlockLayout GetLayout(StageInfo stageInfo)
    {
        if (stageInfo == null) { return null; }

        string layoutId = stageInfo.LayoutId;

#if UNITY_EDITOR
        if (DebugStageOverride.IsSingleBlockEnabled)
        {
            layoutId = DebugStageOverride.SingleBlockLayoutId;
        }
#endif

        return DataManager.Instance.BlockLayouts.GetById(layoutId);
    }

    private void CheckAllBlocksBroken()
    {
        if (_layout == null || AliveCount > 0 || _isAllBlocksBrokenRaised) { return; }

        _isAllBlocksBrokenRaised = true;
        AllBlocksBroken?.Invoke();
    }

    private void Clear()
    {
        _layout = null;
        _grid.ClearAlive();
    }

    private void MoveCellRoot(Vector2 position)
    {
        _cellRoot.position = new Vector3(position.x, position.y, _cellRoot.position.z);
    }

    private Vector2 ToGridPosition(Vector2 worldPosition)
    {
        Vector2 local = _cellRoot.InverseTransformPoint(worldPosition);

        return local / BlockWorldSize + Vector2.one * (_grid.Size * 0.5f);
    }

    private Vector3 GetCellLocalPosition(int column, int row)
    {
        float x = column + 0.5f - _grid.Size * 0.5f;
        float y = row + 0.5f - _grid.Size * 0.5f;

        return new Vector3(x, y, 0f) * BlockWorldSize;
    }

    private float GetCellWorldSize()
    {
        return BlockWorldSize * _cellRoot.lossyScale.x;
    }
}
