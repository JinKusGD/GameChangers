using System.Collections.Generic;
using UnityEngine;

public class StageTemplate
{
    public const char Empty = '.';

    public string Id { get; set; } = string.Empty;

    public List<string> Grid { get; set; } = new List<string>();

    public int Columns => Grid.Count > 0 ? Grid[0].Length : 0;
    public int Rows => Grid.Count;

    public Vector2 CellSize => Columns > 0 && Rows > 0 ? new Vector2(1f / Columns, 1f / Rows) : Vector2.zero;

    public List<StageTemplateSlot> GetSlots()
    {
        List<StageTemplateSlot> slots = new List<StageTemplateSlot>();

        int rows = Grid.Count;

        for (int row = 0; row < rows; row++)
        {
            string line = Grid[row];

            for (int col = 0; col < line.Length; col++)
            {
                char c = line[col];
                if (c < '1' || c > '9') continue;

                Vector2 pos = new Vector2((col + 0.5f) / line.Length, 1f - (row + 0.5f) / rows);

                slots.Add(new StageTemplateSlot(c - '0', pos));
            }
        }

        return slots;
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("Id가 비어 있음");
        }

        if (Grid == null || Grid.Count == 0)
        {
            errors.Add("Grid가 비어 있음");
            return errors;
        }

        int width = Grid[0]?.Length ?? 0;
        int objectCount = 0;

        for (int row = 0; row < Grid.Count; row++)
        {
            string line = Grid[row];

            if (line == null)
            {
                errors.Add($"{row + 1}번째 줄이 null");
                continue;
            }

            if (line.Length != width)
            {
                errors.Add($"{row + 1}번째 줄 길이 {line.Length} (첫 줄은 {width})");
            }

            foreach (char c in line)
            {
                if (c == Empty) { continue; }

                if (c >= '1' && c <= '9')
                {
                    objectCount++;
                }
                else
                {
                    errors.Add($"{row + 1}번째 줄에 알 수 없는 문자 '{c}'");
                }
            }
        }

        if (objectCount == 0)
        {
            errors.Add("오브젝트가 하나도 없음");
        }

        return errors;
    }
}

public readonly struct StageTemplateSlot
{
    public readonly int Tier;
    public readonly Vector2 NormalizedPos;

    public StageTemplateSlot(int tier, Vector2 normalizedPos)
    {
        Tier = tier;
        NormalizedPos = normalizedPos;
    }
}
