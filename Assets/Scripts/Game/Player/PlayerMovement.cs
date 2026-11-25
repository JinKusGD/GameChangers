using UnityEngine;

public class PlayerMovement
{
    private const float MinFlipDeltaX = 0.01f;

    private readonly Transform _owner;
    private readonly Transform _model;
    private readonly BlockField _blockField;
    private readonly float _arrivalDistance;
    private readonly float _modelBaseScaleX;

    private Vector2 _targetPosition;
    private float _moveSpeed;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = Mathf.Max(0f, value);
    }

    public Block Target { get; private set; }
    public bool HasTarget => Target != null;
    public bool HasArrived { get; private set; }
    public int FacingDirection { get; private set; } = -1;

    public PlayerMovement(Transform owner, Transform model, BlockField blockField, float moveSpeed, float arrivalDistance)
    {
        _owner = owner;
        _model = model;
        _blockField = blockField;
        MoveSpeed = moveSpeed;
        _arrivalDistance = arrivalDistance;
        _modelBaseScaleX = model != null ? Mathf.Abs(model.localScale.x) : 1f;
    }

    public void RefreshTarget()
    {
        if (Target == null || !Target.IsAlive)
        {
            Target = _blockField.FindNearest(_owner.position);
        }

        if (Target == null)
        {
            HasArrived = false;
            return;
        }

        _targetPosition = Target.transform.position;
        Vector2 currentPosition = _owner.position;
        HasArrived = (_targetPosition - currentPosition).sqrMagnitude <= _arrivalDistance * _arrivalDistance;
    }

    public void MoveTowardTarget()
    {
        if (!HasTarget) { return; }

        Vector2 currentPosition = _owner.position;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, _targetPosition, _moveSpeed * Time.deltaTime);
        _owner.position = new Vector3(nextPosition.x, nextPosition.y, _owner.position.z);
    }

    public void FaceTarget()
    {
        if (!HasTarget || _model == null) { return; }

        float deltaX = _targetPosition.x - _owner.position.x;
        if (Mathf.Abs(deltaX) < MinFlipDeltaX) { return; }

        FacingDirection = deltaX > 0f ? 1 : -1;

        Vector3 modelScale = _model.localScale;
        modelScale.x = -_modelBaseScaleX * FacingDirection;
        _model.localScale = modelScale;
    }
}
