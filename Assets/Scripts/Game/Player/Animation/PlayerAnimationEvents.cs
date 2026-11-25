using System;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public event Action AttackHit;

    private void OnAttackHit()
    {
        AttackHit?.Invoke();
    }
}
