using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private readonly float _arrivalDistance = 1.2f;

    [Header("Move")]
    [SerializeField, Min(0f)] private float _baseMoveSpeed = 5f;

    [Header("Visual")]
    [SerializeField] private Transform _model;
    [SerializeField] private Animator _animator;

    private PlayerContext _context;

    public Block Target => _context.Movement.Target;
    public float MoveSpeed => _context.Movement.MoveSpeed;

    public PlayerState CurrentState => _context.StateMachine.CurrentState;

    private void Awake()
    {
        if (_animator == null)
        {
            Debug.LogError($"[{nameof(PlayerController)}] animator 참조가 비어 있습니다.", this);
        }

        if (_model == null)
        {
            Debug.LogError($"[{nameof(PlayerController)}] model 참조가 비어 있습니다.", this);
        }

        PlayerMovement movement = new PlayerMovement(transform, _model, _baseMoveSpeed, _arrivalDistance);
        PlayerAnimation animation = new PlayerAnimation(_animator);

        _context = new PlayerContext(movement, animation);
    }

    private void Start()
    {
        _context.StateMachine.ChangeState(_context.IdleState);
    }

    private void Update()
    {
        _context.Movement.RefreshTarget();
        _context.StateMachine.OnUpdate();
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        _context.Movement.MoveSpeed = moveSpeed;
    }

    public void ResetMoveSpeed()
    {
        _context.Movement.MoveSpeed = _baseMoveSpeed;
    }

    public void StartAttack()
    {
        CurrentState?.OnAttackRequested();
    }
}
