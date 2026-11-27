using System;
using System.ComponentModel;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerAnimationEvents _animationEvents;

    private readonly SkillRunner _skillRunner = new SkillRunner();

    private PlayerContext _context;
    private PlayerViewModel _playerViewModel;
    private GameManager _gameManager;

    public event Action AttackHit;

    public bool CanPlayAttackAnimation => _animator != null;

    public Block Target => _context?.Movement.Target;

    public PlayerState CurrentState => _context?.StateMachine.CurrentState;

    private void Awake()
    {
        bool hasAllReferences = true;

        hasAllReferences &= UnityUtility.CheckReference(_model, nameof(_model), this);
        hasAllReferences &= UnityUtility.CheckReference(_animator, nameof(_animator), this);
        hasAllReferences &= UnityUtility.CheckReference(_animationEvents, nameof(_animationEvents), this);

        enabled = hasAllReferences;

        if (!hasAllReferences)
        {
            return;
        }

        _playerViewModel = new PlayerViewModel();

        PlayerMovement movement = new PlayerMovement(transform, _model, _playerViewModel.MoveSpeed);
        PlayerAnimation animation = new PlayerAnimation(_animator);

        _context = new PlayerContext(movement, animation);

        _animationEvents.AttackHit += OnAttackHit;
    }

    private void OnEnable()
    {
        _playerViewModel.PropertyChanged += OnPropertyChanged;
        ApplyMoveSpeed();
    }

    private void OnDisable()
    {
        if (_playerViewModel == null) { return; }

        _playerViewModel.PropertyChanged -= OnPropertyChanged;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

        if (_gameManager != null && _gameManager.Data != null)
        {
            _gameManager.Saving += OnSaving;
            _gameManager.StageChanged += OnStageChanged;
            RestorePosition(_gameManager.Data.StageSave);
        }

        _context.StateMachine.ChangeState(_context.IdleState);
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
        {
            _gameManager.Saving -= OnSaving;
            _gameManager.StageChanged -= OnStageChanged;
        }

        _playerViewModel?.Dispose();
        _playerViewModel = null;

        if (_animationEvents != null)
        {
            _animationEvents.AttackHit -= OnAttackHit;
        }
    }

    private void Update()
    {
        RefreshTarget();
        _context.StateMachine.OnUpdate();
        _skillRunner.Tick(Time.deltaTime);
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

    private void RefreshTarget()
    {
        PlayerMovement movement = _context.Movement;

        if (movement.NeedsTarget)
        {
            StageManager stageManager = StageManager.Instance;
            movement.SetTarget(stageManager != null ? stageManager.FindNearest(transform.position) : null);
            return;
        }

        movement.UpdateArrival();
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

    private void RestorePosition(StageSaveData stageSave)
    {
        if (!stageSave.TryGetPlayerPosition(out Vector2 savedPosition)) { return; }

        transform.position = new Vector3(savedPosition.x, savedPosition.y, transform.position.z);
    }

    private void OnStageChanged()
    {
        _context.Movement.ClearTarget();
    }

    private void OnSaving()
    {
        _gameManager.Data.StageSave.SetPlayerPosition(transform.position);
    }
}
