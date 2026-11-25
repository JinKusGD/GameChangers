using System.Collections.Generic;
using UnityEngine;

public class BlockField : MonoBehaviour
{
    private const float BlockWorldSize = 1f;
    private const int BlocksPerLayoutCell = 3;

    [SerializeField] private Block _blockPrefab;
    [SerializeField] private Transform _cellRoot;

    private readonly BlockData[] _blockDataByKey = new BlockData[BlockLayout.MaxBlockKey + 1];

    private BlockGrid _grid = BlockGrid.Empty;
    private GameManager _gameManager;

    public int AliveCount => _grid.AliveCount;

    private void Awake()
    {
        bool hasAllReferences = true;
        hasAllReferences &= UnityUtility.CheckReference(_blockPrefab, nameof(_blockPrefab), this);
        hasAllReferences &= UnityUtility.CheckReference(_cellRoot, nameof(_cellRoot), this);

        if (!hasAllReferences) { return; }

        CreateGrid();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

        if (_gameManager == null)
        {
            DebugUtility.LogError("[BlockField] GameManager가 없습니다.", this);
            return;
        }

        _gameManager.StageChanged += OnStageChanged;
        OnStageChanged();
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
        {
            _gameManager.StageChanged -= OnStageChanged;
        }

        _grid.Dispose();
    }

    public void Build(StageInfo stageInfo)
    {
        BlockLayout layout = null;

        if (stageInfo != null)
        {
            layout = DataManager.Instance.BlockLayouts.GetById(stageInfo.LayoutId);
        }

        if (layout == null)
        {
            Clear();
            return;
        }

        MoveCellRoot(stageInfo.FieldPosition);
        ResolveBlockData(stageInfo, layout);

        _grid.ResetAlive();

        for (int row = 0; row < _grid.Size; row++)
        {
            for (int column = 0; column < _grid.Size; column++)
            {
                Block cell = _grid.GetCell(column, row);
                int blockKey = layout.GetBlockKey(column / BlocksPerLayoutCell, row / BlocksPerLayoutCell);
                BlockData data = _blockDataByKey[blockKey];

                if (data == null)
                {
                    cell.Clear();
                    continue;
                }

                cell.Setup(data);
                _grid.MarkAlive(column, row);
            }
        }
    }

    public void Clear()
    {
        _grid.ClearAlive();
    }

    public Block FindNearest(Vector2 position, float maxDistance = float.PositiveInfinity)
    {
        return _grid.FindNearest(ToGridPosition(position), maxDistance / GetCellWorldSize());
    }

    public void FindInRadius(Vector2 center, float radius, List<Block> results)
    {
        _grid.FindInRadius(ToGridPosition(center), radius / GetCellWorldSize(), results);
    }

    private void OnStageChanged()
    {
        StageInfo stageInfo = DataManager.Instance.Stages.Get(_gameManager.Chapter, _gameManager.Stage);

        Build(stageInfo);
    }

    private void CreateGrid()
    {
        _grid = new BlockGrid(BlockLayout.Size * BlocksPerLayoutCell);

        for (int row = 0; row < _grid.Size; row++)
        {
            for (int column = 0; column < _grid.Size; column++)
            {
                Block cell = Instantiate(_blockPrefab, _cellRoot);
                cell.name = $"Cell_{column}_{row}";
                cell.transform.localPosition = GetCellLocalPosition(column, row);
                cell.transform.localScale = Vector3.one * BlockWorldSize;
                cell.Clear();

                _grid.SetCell(column, row, cell);
            }
        }
    }

    private void MoveCellRoot(Vector2 position)
    {
        _cellRoot.position = new Vector3(position.x, position.y, _cellRoot.position.z);
    }

    private void ResolveBlockData(StageInfo stageInfo, BlockLayout layout)
    {
        _blockDataByKey[BlockLayout.EmptyBlockKey] = null;

        for (int blockKey = 1; blockKey <= BlockLayout.MaxBlockKey; blockKey++)
        {
            _blockDataByKey[blockKey] = null;

            if (!layout.ContainsBlockKey(blockKey)) { continue; }

            string blockId = stageInfo.GetBlockId(blockKey);

            if (blockId == null)
            {
                DebugUtility.LogError($"[BlockField] '{layout.Id}' 레이아웃의 {blockKey}번 블록이 스테이지 정보에 연결되지 않았습니다.", this);
                continue;
            }

            _blockDataByKey[blockKey] = DataManager.Instance.Blocks.GetById(blockId);
        }
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
