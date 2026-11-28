public class SwordAttackView : Skill
{
    private SwordAttackViewModel _viewModel;
    private Block _pendingTarget;

    protected override string DisplayName => _viewModel?.DisplayName ?? string.Empty;
    protected override SkillTargetMode TargetMode => _viewModel?.TargetMode ?? default;
    protected override float Cooldown => _viewModel?.Cooldown ?? 0f;
    protected override float Range => _viewModel?.Range ?? 0f;

    private double Damage => _viewModel?.Damage ?? 0;

    public override double DamagePerSecond => Cooldown > 0f ? Damage / Cooldown : 0;

    private bool CanWaitForAttackHit => Player != null && Player.CanPlayAttackAnimation;

    protected override void Awake()
    {
        _viewModel = new SwordAttackViewModel();
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Player != null)
        {
            Player.AttackHit += OnAttackHit;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Player != null)
        {
            Player.AttackHit -= OnAttackHit;
        }

        _pendingTarget = null;
    }

    private void OnDestroy()
    {
        _viewModel?.Dispose();
        _viewModel = null;
    }

    protected override void Execute(Block target)
    {
        if (!CanWaitForAttackHit)
        {
            ApplyDamage(target);
            return;
        }

        _pendingTarget = target;
        Player.StartAttack(Cooldown);
    }

    private void OnAttackHit()
    {
        ApplyDamage(_pendingTarget);
        _pendingTarget = null;
    }

    private void ApplyDamage(Block target)
    {
        if (target == null || !target.IsAlive) { return; }

        StageManager stageManager = StageManager.Instance;

        if (stageManager == null) { return; }

        stageManager.Damage(target, Damage);
    }
}
