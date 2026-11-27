using System;
using System.Collections.Generic;
using UnityEngine;

public class RegenTrait : BlockTrait
{
    private const string IntervalKey = "Interval";
    private const string RateKey = "Rate";
    private const int NotRegistered = -1;
    private const double MinInterval = 0.1;

    private static readonly List<RegenTrait> ActiveTraits = new List<RegenTrait>();

    private double _interval;
    private double _rate;
    private double _elapsed;
    private int _activeIndex = NotRegistered;

    public override BlockTraitType Type => BlockTraitType.Regen;

    public static void TickAll(float deltaTime)
    {
        for (int i = 0; i < ActiveTraits.Count; i++)
        {
            ActiveTraits[i].Tick(deltaTime);
        }
    }

    public override double MergeParam(string key, double current, double added)
    {
        if (IsKey(key, IntervalKey)) { return Math.Min(current, added); }

        return base.MergeParam(key, current, added);
    }

    protected override void OnInitialize(BlockTraitData data)
    {
        double interval = data.GetParam(IntervalKey, 3);
        _interval = interval > 0 ? Math.Max(interval, MinInterval) : 0;
        _rate = data.GetParam(RateKey, 0.05);
        _elapsed = 0;

        _activeIndex = ActiveTraits.Count;
        ActiveTraits.Add(this);
    }

    protected override void OnReset()
    {
        if (_activeIndex == NotRegistered) { return; }

        int lastIndex = ActiveTraits.Count - 1;
        RegenTrait last = ActiveTraits[lastIndex];

        ActiveTraits[_activeIndex] = last;
        last._activeIndex = _activeIndex;
        ActiveTraits.RemoveAt(lastIndex);

        _activeIndex = NotRegistered;
    }

    private void Tick(float deltaTime)
    {
        if (_interval <= 0) { return; }

        if (Owner.Hp >= Owner.MaxHp)
        {
            _elapsed = 0;
            return;
        }

        _elapsed += deltaTime;

        while (_elapsed >= _interval)
        {
            _elapsed -= _interval;
            Owner.Heal(Owner.MaxHp * _rate);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveTraits()
    {
        ActiveTraits.Clear();
    }
}
