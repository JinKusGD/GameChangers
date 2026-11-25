using UnityEngine;

public class PlayerAnimation
{
    private const float FadeDuration = 0.05f;

    private readonly Animator _animator;

    public bool HasAnimator => _animator != null;

    public PlayerAnimation(Animator animator)
    {
        _animator = animator;
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
}
