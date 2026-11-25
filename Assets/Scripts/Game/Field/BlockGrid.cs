using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockGrid : IDisposable
{
    private const int RingSearchLimit = 5;
    private const int NotAlive = -1;

    public static readonly BlockGrid Empty = new BlockGrid(0);

    private readonly Block[] _cells;
    private readonly Dictionary<Block, int> _cellIndexByBlock = new Dictionary<Block, int>();

    private readonly int[] _aliveCellIndices;
    private readonly int[] _aliveSlotByCell;
    private int _aliveCount;

    public int Size { get; }
    public int AliveCount => _aliveCount;

    public BlockGrid(int size)
    {
        Size = size;

        _cells = new Block[size * size];
        _aliveCellIndices = new int[_cells.Length];
        _aliveSlotByCell = new int[_cells.Length];

        for (int i = 0; i < _aliveSlotByCell.Length; i++)
        {
            _aliveSlotByCell[i] = NotAlive;
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            if (_cells[i] != null)
            {
                _cells[i].Broken -= OnCellBroken;
            }
        }

        _cellIndexByBlock.Clear();
    }

    public void SetCell(int column, int row, Block cell)
    {
        int cellIndex = ToIndex(column, row);

        _cells[cellIndex] = cell;
        _cellIndexByBlock.Add(cell, cellIndex);

        cell.Broken += OnCellBroken;
    }

    public Block GetCell(int column, int row)
    {
        return _cells[ToIndex(column, row)];
    }

    public void MarkAlive(int column, int row)
    {
        int cellIndex = ToIndex(column, row);

        if (_aliveSlotByCell[cellIndex] != NotAlive) { return; }

        _aliveSlotByCell[cellIndex] = _aliveCount;
        _aliveCellIndices[_aliveCount] = cellIndex;
        _aliveCount++;
    }

    public void ClearAlive()
    {
        for (int i = 0; i < _aliveCount; i++)
        {
            _cells[_aliveCellIndices[i]].Clear();
        }

        ResetAlive();
    }

    public void ResetAlive()
    {
        for (int i = 0; i < _aliveCount; i++)
        {
            _aliveSlotByCell[_aliveCellIndices[i]] = NotAlive;
        }

        _aliveCount = 0;
    }

    public Block FindNearest(Vector2 gridPosition, float maxDistance)
    {
        if (_aliveCount == 0) { return null; }

        Block nearest = null;
        float nearestSqrDistance = maxDistance * maxDistance;

        bool isComplete = SearchRings(gridPosition, ref nearest, ref nearestSqrDistance);

        if (!isComplete)
        {
            SearchAlive(gridPosition, ref nearest, ref nearestSqrDistance);
        }

        return nearest;
    }

    public void FindInRadius(Vector2 gridCenter, float radius, List<Block> results)
    {
        results.Clear();

        if (_aliveCount == 0) { return; }

        float radiusSqr = radius * radius;

        int minColumn = Mathf.Max(0, Mathf.FloorToInt(gridCenter.x - radius));
        int maxColumn = Mathf.Min(Size - 1, Mathf.FloorToInt(gridCenter.x + radius));
        int minRow = Mathf.Max(0, Mathf.FloorToInt(gridCenter.y - radius));
        int maxRow = Mathf.Min(Size - 1, Mathf.FloorToInt(gridCenter.y + radius));

        int areaCellCount = (maxColumn - minColumn + 1) * (maxRow - minRow + 1);

        if (_aliveCount < areaCellCount)
        {
            CollectAliveInRadius(gridCenter, radiusSqr, results);
            return;
        }

        for (int row = minRow; row <= maxRow; row++)
        {
            for (int column = minColumn; column <= maxColumn; column++)
            {
                Block cell = GetCell(column, row);

                if (cell.IsAlive && SqrDistanceToCell(column, row, gridCenter) <= radiusSqr)
                {
                    results.Add(cell);
                }
            }
        }
    }

    private bool SearchRings(Vector2 gridPosition, ref Block nearest, ref float nearestSqrDistance)
    {
        int originColumn = Mathf.FloorToInt(gridPosition.x);
        int originRow = Mathf.FloorToInt(gridPosition.y);
        int maxRing = GetMaxRing(originColumn, originRow);

        for (int ring = 0; ring <= maxRing; ring++)
        {
            float ringMinDistance = Mathf.Max(0f, ring - 0.5f);

            if (ringMinDistance * ringMinDistance > nearestSqrDistance) { return true; }

            if (ring > RingSearchLimit) { return false; }

            SearchRing(originColumn, originRow, ring, gridPosition, ref nearest, ref nearestSqrDistance);
        }

        return true;
    }

    private void SearchRing(int originColumn, int originRow, int ring, Vector2 gridPosition, ref Block nearest, ref float nearestSqrDistance)
    {
        int minColumn = originColumn - ring;
        int maxColumn = originColumn + ring;
        int minRow = originRow - ring;
        int maxRow = originRow + ring;

        int firstColumn = Mathf.Max(0, minColumn);
        int lastColumn = Mathf.Min(Size - 1, maxColumn);

        for (int column = firstColumn; column <= lastColumn; column++)
        {
            CheckNearer(column, minRow, gridPosition, ref nearest, ref nearestSqrDistance);

            if (maxRow != minRow)
            {
                CheckNearer(column, maxRow, gridPosition, ref nearest, ref nearestSqrDistance);
            }
        }

        int firstRow = Mathf.Max(0, minRow + 1);
        int lastRow = Mathf.Min(Size - 1, maxRow - 1);

        for (int row = firstRow; row <= lastRow; row++)
        {
            CheckNearer(minColumn, row, gridPosition, ref nearest, ref nearestSqrDistance);
            CheckNearer(maxColumn, row, gridPosition, ref nearest, ref nearestSqrDistance);
        }
    }

    private void SearchAlive(Vector2 gridPosition, ref Block nearest, ref float nearestSqrDistance)
    {
        for (int i = 0; i < _aliveCount; i++)
        {
            int cellIndex = _aliveCellIndices[i];
            float sqrDistance = SqrDistanceToCell(cellIndex % Size, cellIndex / Size, gridPosition);

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = _cells[cellIndex];
            }
        }
    }

    private void CollectAliveInRadius(Vector2 gridCenter, float radiusSqr, List<Block> results)
    {
        for (int i = 0; i < _aliveCount; i++)
        {
            int cellIndex = _aliveCellIndices[i];

            if (SqrDistanceToCell(cellIndex % Size, cellIndex / Size, gridCenter) <= radiusSqr)
            {
                results.Add(_cells[cellIndex]);
            }
        }
    }

    private void CheckNearer(int column, int row, Vector2 gridPosition, ref Block nearest, ref float nearestSqrDistance)
    {
        if (!IsInside(column, row)) { return; }

        Block cell = GetCell(column, row);

        if (!cell.IsAlive) { return; }

        float sqrDistance = SqrDistanceToCell(column, row, gridPosition);

        if (sqrDistance < nearestSqrDistance)
        {
            nearestSqrDistance = sqrDistance;
            nearest = cell;
        }
    }

    private void OnCellBroken(Block cell)
    {
        if (_cellIndexByBlock.TryGetValue(cell, out int cellIndex))
        {
            RemoveAlive(cellIndex);
        }
    }

    private void RemoveAlive(int cellIndex)
    {
        int slot = _aliveSlotByCell[cellIndex];

        if (slot == NotAlive) { return; }

        int lastCellIndex = _aliveCellIndices[_aliveCount - 1];

        _aliveCellIndices[slot] = lastCellIndex;
        _aliveSlotByCell[lastCellIndex] = slot;

        _aliveSlotByCell[cellIndex] = NotAlive;
        _aliveCount--;
    }

    private int GetMaxRing(int column, int row)
    {
        int horizontal = Mathf.Max(column, Size - 1 - column);
        int vertical = Mathf.Max(row, Size - 1 - row);

        return Mathf.Max(horizontal, vertical);
    }

    private bool IsInside(int column, int row)
    {
        return column >= 0 && column < Size && row >= 0 && row < Size;
    }

    private int ToIndex(int column, int row)
    {
        return row * Size + column;
    }

    private static float SqrDistanceToCell(int column, int row, Vector2 gridPosition)
    {
        float dx = column + 0.5f - gridPosition.x;
        float dy = row + 0.5f - gridPosition.y;

        return dx * dx + dy * dy;
    }
}
