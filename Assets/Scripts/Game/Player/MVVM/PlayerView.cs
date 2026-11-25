using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerView : MonoBehaviour
{
    private const float ArrivalDistance = 1.2f;

    [Header("Visual")]
    [SerializeField] private Transform _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerAnimationEvents _animationEvents;

    [Header("Field")]
    [SerializeField, FormerlySerializedAs("_stageField")] private BlockField _blockField;

    private readonly SkillRunner _skillRunner = new SkillRunner();

    private PlayerContext _context;
    private PlayerViewModel _playerViewModel;

    public event Action AttackHit;

    public bool CanPlayAttackAnimation => _animator != null;

    public Block Target => _context?.Movement.Target;

    public BlockField BlockField => _blockField;

    public PlayerState CurrentState => _context?.StateMachine.CurrentState;

    private void Awake()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_model, nameof(_model), this);
        hasAllReferences &= UnityUtility.CheckReference(_animator, nameof(_animator), this);
        hasAllReferences &= UnityUtility.CheckReference(_animationEvents, nameof(_animationEvents), this);
        hasAllReferences &= UnityUtility.CheckReference(_blockField, nameof(_blockField), this);

        enabled = hasAllReferences;

        if (!hasAllReferences)
        {
            return;
        }

        _playerViewModel = new PlayerViewModel();

        PlayerMovement movement = new PlayerMovement(transform, _model, _blockField, _playerViewModel.MoveSpeed, ArrivalDistance);
        PlayerAnimation animation = new PlayerAnimation(_animator);

        _context = new PlayerContext(movement, animation);

        _animationEvents.AttackHit += OnAttackHit;
    }

    private void OnEnable()
    {
        _playerViewModel.PropertyChanged += OnPropertyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (_playerViewModel == null) { return; }

        _playerViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void Start()
    {
        _context.StateMachine.ChangeState(_context.IdleState);
    }

    private void OnDestroy()
    {
        _playerViewModel?.Dispose();
        _playerViewModel = null;

        if (_animationEvents != null)
        {
            _animationEvents.AttackHit -= OnAttackHit;
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        _context.Movement.RefreshTarget();
        _context.StateMachine.OnUpdate();
        _skillRunner.Tick(deltaTime);
    }

    public void StartAttack(float attackInterval)
    {
        CurrentState?.OnAttackRequested(attackInterval);
    }

    public void RegisterSkill(Skill skill)
    {
        _skillRunner.Register(skill);
    }

    public void UnregisterSkill(Skill skill)
    {
        _skillRunner.Unregister(skill);
    }

    private void Refresh()
    {
        ApplyMoveSpeed();
    }

    private void ApplyMoveSpeed()
    {
        _context.Movement.MoveSpeed = _playerViewModel.MoveSpeed;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_playerViewModel.MoveSpeed):
                ApplyMoveSpeed();
                break;
        }
    }

    private void OnAttackHit()
    {
        AttackHit?.Invoke();
    }
}
