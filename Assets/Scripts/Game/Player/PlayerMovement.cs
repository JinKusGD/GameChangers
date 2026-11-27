using UnityEngine;

public class PlayerMovement
{
    private const float MinFlipDeltaX = 0.01f;
    private const float ArrivalDistance = 1.2f;

    private readonly Transform _owner;
    private readonly Transform _model;

    private readonly float _modelBaseScaleX;

    private Vector2 _targetPosition;
    private float _moveSpeed;
    private int _facingDirection = -1;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = Mathf.Max(0f, value);
    }

    public Block Target { get; private set; }
    public bool HasTarget => Target != null;
    public bool HasArrived { get; private set; }
    public bool NeedsTarget => Target == null || !Target.IsAlive;

    private Vector2 CurrentPosition => _owner.position;

    public PlayerMovement(Transform owner, Transform model, float moveSpeed)
    {
        _owner = owner;
        _model = model;
        MoveSpeed = moveSpeed;
        _modelBaseScaleX = model != null ? Mathf.Abs(model.localScale.x) : 1f;
    }

    public void ClearTarget()
    {
        Target = null;
        HasArrived = false;
    }

    public void SetTarget(Block target)
    {
        Target = target;
        UpdateArrival();
    }

    public void UpdateArrival()
    {
        if (Target == null)
        {
            HasArrived = false;
            return;
        }

        _targetPosition = Target.transform.position;
        HasArrived = (_targetPosition - CurrentPosition).sqrMagnitude <= ArrivalDistance * ArrivalDistance;
    }

    public void MoveTowardTarget()
    {
        if (!HasTarget) { return; }

        Vector2 nextPosition = Vector2.MoveTowards(CurrentPosition, _targetPosition, _moveSpeed * Time.deltaTime);
        SetPosition(nextPosition);
    }

    public void FaceTarget()
    {
        if (!HasTarget || _model == null) { return; }

        float deltaX = _targetPosition.x - CurrentPosition.x;
        if (Mathf.Abs(deltaX) < MinFlipDeltaX) { return; }

        _facingDirection = deltaX > 0f ? 1 : -1;

        Vector3 modelScale = _model.localScale;
        modelScale.x = -_modelBaseScaleX * _facingDirection;
        _model.localScale = modelScale;
    }

    private void SetPosition(Vector2 position)
    {
        _owner.position = new Vector3(position.x, position.y, _owner.position.z);
    }
}
