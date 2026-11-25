using System;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private List<BlockTrait> _traits;

    public double MaxHp { get; private set; }
    public double Hp { get; private set; }

    public bool IsAlive => Hp > 0;

    public event Action<Block, double> Damaged;
    public event Action<Block> Broken;
    public event Action<Block> Cleared;

    private void OnDestroy()
    {
        ReleaseTraits();
    }

    public void Setup(BlockData data)
    {
        Clear();

        if (data == null)
        {
            DebugUtility.LogError($"[Block] {name}: BlockData가 비어 있습니다.", this);
            return;
        }

        MaxHp = data.MaxHp;
        Hp = data.MaxHp;

        ApplyVisual(data);
        SetupTraits(data);
    }

    public void Clear()
    {
        ReleaseTraits();

        MaxHp = 0;
        Hp = 0;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }

        Cleared?.Invoke(this);
    }

    public void TakeDamage(double damage)
    {
        if (!IsAlive) { return; }

        damage = ModifyDamage(damage);

        if (damage <= 0) { return; }

        Hp = Math.Max(0, Hp - damage);
        Damaged?.Invoke(this, damage);

        if (_traits != null)
        {
            for (int i = 0; i < _traits.Count; i++)
            {
                _traits[i].OnDamaged(damage);
            }
        }

        if (!IsAlive)
        {
            Break();
        }
    }

    private void ApplyVisual(BlockData data)
    {
        if (_spriteRenderer == null) { return; }

        _spriteRenderer.enabled = true;

        if (data.Sprite != null)
        {
            _spriteRenderer.sprite = data.Sprite;
        }

        _spriteRenderer.color = data.Color;
    }

    private void SetupTraits(BlockData data)
    {
        ReleaseTraits();

        foreach (BlockTraitData traitData in data.Traits)
        {
            BlockTrait trait = BlockTraitPool.Get(this, traitData);

            if (trait == null) { continue; }

            if (_traits == null)
            {
                _traits = new List<BlockTrait>();
            }

            _traits.Add(trait);
        }
    }

    private void ReleaseTraits()
    {
        if (_traits == null) { return; }

        for (int i = 0; i < _traits.Count; i++)
        {
            BlockTraitPool.Release(_traits[i]);
        }

        _traits.Clear();
    }

    private double ModifyDamage(double damage)
    {
        if (_traits == null) { return damage; }

        for (int i = 0; i < _traits.Count; i++)
        {
            damage = _traits[i].ModifyDamage(damage);
        }

        return damage;
    }

    private void Break()
    {
        if (_traits != null)
        {
            for (int i = 0; i < _traits.Count; i++)
            {
                _traits[i].OnBroken();
            }
        }

        Broken?.Invoke(this);
        Clear();
    }
}
