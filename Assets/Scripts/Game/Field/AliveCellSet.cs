public class AliveCellSet
{
    private const int NotInSet = -1;

    private readonly int[] _cellIndices;
    private readonly int[] _slotByCell;

    public int Count { get; private set; }

    public int this[int slot] => _cellIndices[slot];

    public AliveCellSet(int capacity)
    {
        _cellIndices = new int[capacity];
        _slotByCell = new int[capacity];

        for (int i = 0; i < _slotByCell.Length; i++)
        {
            _slotByCell[i] = NotInSet;
        }
    }

    public bool Contains(int cellIndex)
    {
        return _slotByCell[cellIndex] != NotInSet;
    }

    public bool Add(int cellIndex)
    {
        if (Contains(cellIndex)) { return false; }

        _slotByCell[cellIndex] = Count;
        _cellIndices[Count] = cellIndex;
        Count++;

        return true;
    }

    public bool Remove(int cellIndex)
    {
        int slot = _slotByCell[cellIndex];

        if (slot == NotInSet) { return false; }

        int lastCellIndex = _cellIndices[Count - 1];

        _cellIndices[slot] = lastCellIndex;
        _slotByCell[lastCellIndex] = slot;

        _slotByCell[cellIndex] = NotInSet;
        Count--;

        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < Count; i++)
        {
            _slotByCell[_cellIndices[i]] = NotInSet;
        }

        Count = 0;
    }
}
