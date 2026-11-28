using System.Collections.Generic;
using UnityEngine;

public class StageData : IValidatable
{
    public int Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string LayoutId { get; set; } = string.Empty;
    public FieldPositionData FieldPosition { get; set; }
    public string Background { get; set; } = string.Empty;

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (Stage < 1)
        {
            errors.Add($"Stage는 1 이상이어야 합니다. ({Stage})");
        }

        if (string.IsNullOrWhiteSpace(StageName))
        {
            errors.Add("StageName이 비어 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(LayoutId))
        {
            errors.Add("LayoutId가 비어 있습니다.");
        }

        ValidateFieldPosition(errors);

        return errors;
    }

    private void ValidateFieldPosition(List<string> errors)
    {
        if (FieldPosition == null)
        {
            errors.Add("FieldPosition이 없습니다.");
            return;
        }

        if (!FieldPosition.X.HasValue)
        {
            errors.Add("FieldPosition의 x가 없습니다.");
        }

        if (!FieldPosition.Y.HasValue)
        {
            errors.Add("FieldPosition의 y가 없습니다.");
        }
    }

    public class FieldPositionData
    {
        public float? X { get; set; }
        public float? Y { get; set; }

        public Vector2 ToVector2()
        {
            return new Vector2(X.Value, Y.Value);
        }
    }
}
