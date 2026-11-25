using System.Collections.Generic;

public class BlockLayout : IValidatable
{
    public const int Size = 16;
    public const int EmptyBlockKey = 0;
    public const int MaxBlockKey = 9;

    private const char EmptySymbol = '.';

    public string Id { get; set; } = string.Empty;

    public List<string> Grid { get; set; } = new List<string>();

    public int GetBlockKey(int column, int row)
    {
        char symbol = Grid[Size - 1 - row][column];

        if (TryParseBlockKey(symbol, out int blockKey))
        {
            return blockKey;
        }

        return EmptyBlockKey;
    }

    public bool ContainsBlockKey(int blockKey)
    {
        for (int row = 0; row < Size; row++)
        {
            foreach (char symbol in Grid[row])
            {
                if (TryParseBlockKey(symbol, out int key) && key == blockKey)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Id가 비어 있음");
        }

        if (Grid == null || Grid.Count != Size)
        {
            int rowCount = 0;

            if (Grid != null)
            {
                rowCount = Grid.Count;
            }

            errors.Add($"줄 수 {rowCount} (반드시 {Size})");
            return errors;
        }

        int blockCount = 0;

        for (int row = 0; row < Size; row++)
        {
            blockCount += ValidateLine(row, errors);
        }

        if (blockCount == 0)
        {
            errors.Add("블록이 하나도 없음");
        }

        return errors;
    }

    private int ValidateLine(int row, List<string> errors)
    {
        string line = Grid[row];
        int lineNumber = row + 1;

        if (line == null)
        {
            errors.Add($"{lineNumber}번째 줄이 null");
            return 0;
        }

        if (line.Length != Size)
        {
            errors.Add($"{lineNumber}번째 줄 길이 {line.Length} (반드시 {Size})");
        }

        int blockCount = 0;

        foreach (char symbol in line)
        {
            if (symbol == EmptySymbol) { continue; }

            if (TryParseBlockKey(symbol, out _))
            {
                blockCount++;
                continue;
            }

            errors.Add($"{lineNumber}번째 줄에 알 수 없는 문자 '{symbol}'");
        }

        return blockCount;
    }

    private static bool TryParseBlockKey(char symbol, out int blockKey)
    {
        if (symbol >= '1' && symbol <= '9')
        {
            blockKey = symbol - '0';
            return true;
        }

        blockKey = EmptyBlockKey;
        return false;
    }
}
