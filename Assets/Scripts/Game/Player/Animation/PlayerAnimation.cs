using UnityEngine;

public class PlayerAnimation
{
    private const float FadeDuration = 0.05f;
    private const string AttackClipName = "Attack";

    private readonly Animator _animator;
    private readonly float _attackClipLength;

    public bool HasAnimator => _animator != null;

    public PlayerAnimation(Animator animator)
    {
        _animator = animator;
        _attackClipLength = FindClipLength(AttackClipName);
    }

    public void Play(int stateHash)
    {
        if (!HasAnimator) { return; }

        _animator.Play(stateHash, 0, 0f);
    }

    public void CrossFade(int stateHash)
    {
        if (!HasAnimator || IsInOrEntering(stateHash)) { return; }

        _animator.CrossFade(stateHash, FadeDuration, 0);
    }

    public bool IsInOrEntering(int stateHash)
    {
        if (!HasAnimator) { return false; }

        if (_animator.IsInTransition(0))
        {
            return _animator.GetNextAnimatorStateInfo(0).shortNameHash == stateHash;
        }

        return _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
    }

    public void FitAttackSpeedToInterval(float attackInterval)
    {
        if (!HasAnimator) { return; }

        float speed = 1f;

        if (attackInterval > 0f && attackInterval < _attackClipLength)
        {
            speed = _attackClipLength / attackInterval;
        }

        _animator.SetFloat(PlayerAnimationHash.AttackSpeedParameter, speed);
    }

    private float FindClipLength(string clipName)
    {
        if (!HasAnimator || _animator.runtimeAnimatorController == null) { return 0f; }

        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }

        DebugUtility.LogWarning($"[{nameof(PlayerAnimation)}] '{clipName}' 클립을 찾지 못했습니다.", _animator);
        return 0f;
    }
}
