using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockGrid : IDisposable
{
    private const int RingSearchLimit = 5;

    public static readonly BlockGrid Empty = new BlockGrid(0);

    private readonly Block[] _cells;
    private readonly AliveCellSet _alive;

    public int Size { get; }
    public int AliveCount => _alive.Count;

    public event Action Changed;

    public BlockGrid(int size)
    {
        Size = size;

        _cells = new Block[size * size];
        _alive = new AliveCellSet(_cells.Length);
    }

    public void Dispose()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            Block cell = _cells[i];

            if (cell != null)
            {
                cell.Broken -= OnCellBroken;
            }
        }
    }

    public void SetCell(int column, int row, Block cell)
    {
        int cellIndex = ToIndex(column, row);
        _cells[cellIndex] = cell;

        cell.Broken += OnCellBroken;
        cell.Bind(column, row);
    }

    public Block GetCell(int column, int row)
    {
        int cellIndex = ToIndex(column, row);

        return _cells[cellIndex];
    }

    public Block GetCell(int cellIndex)
    {
        return _cells[cellIndex];
    }

    public bool IsValidIndex(int cellIndex)
    {
        return cellIndex >= 0 && cellIndex < _cells.Length;
    }

    public int ToIndex(int column, int row)
    {
        return row * Size + column;
    }

    public bool TryGetAliveCell(int column, int row, out Block cell)
    {
        cell = null;

        if (!IsInside(column, row)) { return false; }

        cell = GetCell(column, row);

        return cell != null && cell.IsAlive;
    }

    public void PlaceBlock(Block cell, StageBlock stageBlock)
    {
        cell.Setup(stageBlock);
        MarkAlive(cell);
        MarkChanged();
    }

    public void RemoveBlock(Block cell)
    {
        MarkDead(cell);
        cell.Clear();
    }

    public void ClearAlive()
    {
        for (int i = 0; i < _alive.Count; i++)
        {
            Block cell = GetAliveCell(i);
            cell.Clear();
        }

        ResetAlive();
    }

    public void ResetAlive()
    {
        _alive.Clear();
        MarkChanged();
    }

    public Block FindNearest(Vector2 gridPosition, float maxDistance)
    {
        if (_alive.Count == 0) { return null; }

        NearestCandidate nearest = new NearestCandidate(gridPosition, maxDistance);

        if (!TrySearchNearbyRings(ref nearest))
        {
            SearchAllAlive(ref nearest);
        }

        return nearest.Block;
    }

    public void FindInRadius(Vector2 gridCenter, float radius, List<Block> results)
    {
        results.Clear();

        if (_alive.Count == 0) { return; }

        float radiusSqr = radius * radius;
        RectInt area = GetClampedArea(gridCenter, radius);

        if (_alive.Count < area.width * area.height)
        {
            CollectAliveInRadius(gridCenter, radiusSqr, results);
            return;
        }

        CollectAliveInArea(area, gridCenter, radiusSqr, results);
    }

    private void MarkAlive(Block cell)
    {
        int cellIndex = GetCellIndex(cell);

        _alive.Add(cellIndex);
    }

    private void MarkDead(Block cell)
    {
        int cellIndex = GetCellIndex(cell);
        bool isRemoved = _alive.Remove(cellIndex);

        if (isRemoved)
        {
            MarkChanged();
        }
    }

    private void MarkChanged()
    {
        Changed?.Invoke();
    }

    private Block GetAliveCell(int slot)
    {
        int cellIndex = _alive[slot];

        return _cells[cellIndex];
    }

    private int GetCellIndex(Block cell)
    {
        return ToIndex(cell.Column, cell.Row);
    }

    private void OnCellBroken(Block cell)
    {
        MarkDead(cell);
    }

    private bool TrySearchNearbyRings(ref NearestCandidate nearest)
    {
        int originColumn = Mathf.FloorToInt(nearest.Position.x);
        int originRow = Mathf.FloorToInt(nearest.Position.y);
        int maxRing = GetMaxRing(originColumn, originRow);

        for (int ring = 0; ring <= maxRing; ring++)
        {
            if (IsRingBeyond(ring, nearest.SqrDistance)) { return true; }

            if (ring > RingSearchLimit) { return false; }

            SearchRing(originColumn, originRow, ring, ref nearest);
        }

        return true;
    }

    private static bool IsRingBeyond(int ring, float sqrDistance)
    {
        float ringMinDistance = Mathf.Max(0f, ring - 0.5f);

        return ringMinDistance * ringMinDistance > sqrDistance;
    }

    private void SearchRing(int originColumn, int originRow, int ring, ref NearestCandidate nearest)
    {
        int minColumn = originColumn - ring;
        int maxColumn = originColumn + ring;
        int minRow = originRow - ring;
        int maxRow = originRow + ring;

        SearchRingTopAndBottom(minColumn, maxColumn, minRow, maxRow, ref nearest);
        SearchRingLeftAndRight(minColumn, maxColumn, minRow, maxRow, ref nearest);
    }

    private void SearchRingTopAndBottom(int minColumn, int maxColumn, int minRow, int maxRow, ref NearestCandidate nearest)
    {
        int firstColumn = Mathf.Max(0, minColumn);
        int lastColumn = Mathf.Min(Size - 1, maxColumn);

        for (int column = firstColumn; column <= lastColumn; column++)
        {
            ConsiderCell(column, minRow, ref nearest);

            if (maxRow != minRow)
            {
                ConsiderCell(column, maxRow, ref nearest);
            }
        }
    }

    private void SearchRingLeftAndRight(int minColumn, int maxColumn, int minRow, int maxRow, ref NearestCandidate nearest)
    {
        int firstRow = Mathf.Max(0, minRow + 1);
        int lastRow = Mathf.Min(Size - 1, maxRow - 1);

        for (int row = firstRow; row <= lastRow; row++)
        {
            ConsiderCell(minColumn, row, ref nearest);
            ConsiderCell(maxColumn, row, ref nearest);
        }
    }

    private void SearchAllAlive(ref NearestCandidate nearest)
    {
        for (int i = 0; i < _alive.Count; i++)
        {
            Block cell = GetAliveCell(i);
            float sqrDistance = SqrDistanceToCell(cell, nearest.Position);

            nearest.Consider(cell, sqrDistance);
        }
    }

    private void ConsiderCell(int column, int row, ref NearestCandidate nearest)
    {
        if (!IsInside(column, row)) { return; }

        Block cell = GetCell(column, row);

        if (!cell.IsAlive) { return; }

        float sqrDistance = SqrDistanceToCell(column, row, nearest.Position);
        nearest.Consider(cell, sqrDistance);
    }

    private RectInt GetClampedArea(Vector2 gridCenter, float radius)
    {
        int minColumn = Mathf.Max(0, Mathf.FloorToInt(gridCenter.x - radius));
        int maxColumn = Mathf.Min(Size - 1, Mathf.FloorToInt(gridCenter.x + radius));
        int minRow = Mathf.Max(0, Mathf.FloorToInt(gridCenter.y - radius));
        int maxRow = Mathf.Min(Size - 1, Mathf.FloorToInt(gridCenter.y + radius));

        return new RectInt(minColumn, minRow, maxColumn - minColumn + 1, maxRow - minRow + 1);
    }

    private void CollectAliveInRadius(Vector2 gridCenter, float radiusSqr, List<Block> results)
    {
        for (int i = 0; i < _alive.Count; i++)
        {
            Block cell = GetAliveCell(i);
            float sqrDistance = SqrDistanceToCell(cell, gridCenter);

            if (sqrDistance <= radiusSqr)
            {
                results.Add(cell);
            }
        }
    }

    private void CollectAliveInArea(RectInt area, Vector2 gridCenter, float radiusSqr, List<Block> results)
    {
        for (int row = area.yMin; row < area.yMax; row++)
        {
            for (int column = area.xMin; column < area.xMax; column++)
            {
                Block cell = GetCell(column, row);

                if (!cell.IsAlive) { continue; }

                float sqrDistance = SqrDistanceToCell(column, row, gridCenter);

                if (sqrDistance <= radiusSqr)
                {
                    results.Add(cell);
                }
            }
        }
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

    private static float SqrDistanceToCell(Block cell, Vector2 gridPosition)
    {
        return SqrDistanceToCell(cell.Column, cell.Row, gridPosition);
    }

    private static float SqrDistanceToCell(int column, int row, Vector2 gridPosition)
    {
        float dx = column + 0.5f - gridPosition.x;
        float dy = row + 0.5f - gridPosition.y;

        return dx * dx + dy * dy;
    }

    private struct NearestCandidate
    {
        public readonly Vector2 Position;

        public Block Block { get; private set; }
        public float SqrDistance { get; private set; }

        public NearestCandidate(Vector2 position, float maxDistance)
        {
            Position = position;
            Block = null;
            SqrDistance = maxDistance * maxDistance;
        }

        public void Consider(Block cell, float sqrDistance)
        {
            if (sqrDistance >= SqrDistance) { return; }

            SqrDistance = sqrDistance;
            Block = cell;
        }
    }
}
