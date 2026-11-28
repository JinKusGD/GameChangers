using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    private const string Tag = "Block";

    private SpriteRenderer _spriteRenderer;
    private readonly List<BlockTrait> _traits = new List<BlockTrait>();

    private Sprite _defaultSprite;

    private double _damageTakenMultiplier = 1;

    private int _splitBlockKey;

    public double GuardPercent { get; private set; }
    public bool SharesDamage { get; private set; }

    public int Key { get; private set; }
    public Color Color { get; private set; }
    public double MaxHp { get; private set; }
    public double Hp { get; private set; }
    public double Gold { get; private set; }

    public int Column { get; private set; }
    public int Row { get; private set; }

    public bool IsAlive => Hp > 0;

    public event Action<Block, double> Damaged;
    public event Action<Block> Broken;
    public event Action<Block> Cleared;
    public event Action<Block, int> SplitRequested;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultSprite = _spriteRenderer.sprite;
    }

    private void OnDestroy()
    {
        ReleaseTraits();
    }

    public void Bind(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public void Setup(StageBlock stageBlock)
    {
        Clear();

        if (stageBlock == null)
        {
            DebugUtility.LogError($"[{Tag}] {name}: StageBlock이 비어 있습니다.", this);
            return;
        }

        Key = stageBlock.Data.Key;
        MaxHp = stageBlock.MaxHp;
        Hp = MaxHp;

        ApplyVisual(stageBlock.Data);

        double goldBonus = SetupTraits(stageBlock.Traits);
        Gold = stageBlock.Gold * (1 + goldBonus / 100);
    }

    public void Clear()
    {
        ReleaseTraits();

        Key = BlockKey.Empty;
        Color = default;
        MaxHp = 0;
        Hp = 0;
        Gold = 0;

        _spriteRenderer.enabled = false;

        Cleared?.Invoke(this);
    }

    public void LoadHp(double hp)
    {
        if (!IsAlive || hp <= 0) { return; }

        Hp = Math.Min(hp, MaxHp);
    }

    public bool TryGetShield(out ShieldTrait shield)
    {
        for (int i = 0; i < _traits.Count; i++)
        {
            if (_traits[i] is ShieldTrait found)
            {
                shield = found;
                return true;
            }
        }

        shield = null;
        return false;
    }

    public void Heal(double amount)
    {
        if (!IsAlive || amount <= 0) { return; }

        Hp = Math.Min(MaxHp, Hp + amount);
    }

    public void ApplyDamage(double damage, double guardPercent)
    {
        if (!IsAlive) { return; }

        double multiplier = GetDamageTakenMultiplier(guardPercent);
        damage = ModifyDamage(damage * multiplier);

        if (damage <= 0) { return; }

        Hp = Math.Max(0, Hp - damage);
        Damaged?.Invoke(this, damage);

        if (!IsAlive)
        {
            Break();
        }
    }

    private double GetDamageTakenMultiplier(double guardPercent)
    {
        double multiplier = _damageTakenMultiplier * (1 - guardPercent / 100);

        return Math.Max(multiplier, DataManager.Instance.Config.Block.MinDamageTakenMultiplier);
    }

    private double ModifyDamage(double damage)
    {
        for (int i = 0; i < _traits.Count; i++)
        {
            damage = _traits[i].ModifyDamage(damage);
        }

        return damage;
    }

    private void Break()
    {
        int splitBlockKey = _splitBlockKey;

        Broken?.Invoke(this);
        Clear();

        if (splitBlockKey != BlockKey.Empty)
        {
            SplitRequested?.Invoke(this, splitBlockKey);
        }
    }

    private void ApplyVisual(BlockData data)
    {
        Color = data.Color;

        _spriteRenderer.enabled = true;

        if (data.Sprite != null)
        {
            _spriteRenderer.sprite = data.Sprite;
            _spriteRenderer.color = Color.white;
        }
        else
        {
            _spriteRenderer.sprite = _defaultSprite;
            _spriteRenderer.color = data.Color;
        }
    }

    private double SetupTraits(IReadOnlyList<BlockTraitData> traits)
    {
        double goldBonus = 0;

        if (traits == null) { return goldBonus; }

        foreach (BlockTraitData traitData in traits)
        {
            BlockTrait trait = BlockTraitPool.Get(this, traitData);

            if (trait == null) { continue; }

            _traits.Add(trait);

            _damageTakenMultiplier *= trait.DamageTakenMultiplier;
            GuardPercent += trait.GuardPercent;
            SharesDamage |= trait.SharesDamage;

            if (trait.SplitBlockKey != BlockKey.Empty)
            {
                _splitBlockKey = trait.SplitBlockKey;
            }
            goldBonus += trait.GoldBonus;
        }

        return goldBonus;
    }

    private void ReleaseTraits()
    {
        _damageTakenMultiplier = 1;
        GuardPercent = 0;
        SharesDamage = false;
        _splitBlockKey = BlockKey.Empty;

        for (int i = 0; i < _traits.Count; i++)
        {
            BlockTraitPool.Release(_traits[i]);
        }

        _traits.Clear();
    }
}
