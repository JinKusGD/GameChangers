using System;
using UnityEngine;

public enum SkillTargetMode
{
    PlayerTarget,
    Nearest
}

public abstract class Skill : MonoBehaviour
{
    private const string Tag = "Skill";

    private float _cooldownRemaining;

    protected abstract string DisplayName { get; }
    protected abstract SkillTargetMode TargetMode { get; }
    protected abstract float Cooldown { get; }
    protected abstract float Range { get; }

    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public float CooldownRatio => Cooldown > 0f ? Mathf.Clamp01(_cooldownRemaining / Cooldown) : 0f;
    public bool IsReady => _cooldownRemaining <= 0f;
    public virtual double DamagePerSecond => 0;

    public event Action<Skill, Block> Used;

    protected PlayerView Player { get; private set; }

    protected virtual void Awake()
    {
        Player = GetComponentInParent<PlayerView>();

        if (Player == null)
        {
            DebugUtility.LogError($"[{Tag}] {DisplayName}: 부모에서 PlayerView를 찾지 못했습니다.", this);
        }
    }

    protected virtual void OnEnable()
    {
        if (Player != null)
        {
            Player.RegisterSkill(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (Player != null)
        {
            Player.UnregisterSkill(this);
        }
    }

    public void Tick(float deltaTime)
    {
        if (!IsReady)
        {
            _cooldownRemaining -= deltaTime;
            return;
        }

        Block target = FindTarget();

        if (target == null) { return; }

        Execute(target);
        _cooldownRemaining = Cooldown;
        Used?.Invoke(this, target);
    }

    protected abstract void Execute(Block target);

    private Block FindTarget()
    {
        switch (TargetMode)
        {
            case SkillTargetMode.PlayerTarget:
                return FindPlayerTarget();

            default:
                return FindNearestTarget();
        }
    }

    private Block FindNearestTarget()
    {
        StageManager stageManager = StageManager.Instance;

        if (stageManager == null) { return null; }

        return stageManager.FindNearest(transform.position, Range);
    }

    private Block FindPlayerTarget()
    {
        if (Player == null) { return null; }

        Block target = Player.Target;

        if (target == null || !target.IsAlive) { return null; }

        return IsInRange(target) ? target : null;
    }

    private bool IsInRange(Block target)
    {
        Vector2 offset = target.transform.position - transform.position;
        return offset.sqrMagnitude <= Range * Range;
    }
}
