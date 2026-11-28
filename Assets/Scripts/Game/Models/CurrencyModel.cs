using System;
using System.ComponentModel;

public class CurrencyModel : ObservableModel
{
    private static readonly PropertyChangedEventArgs GoldChangedArgs = new PropertyChangedEventArgs(nameof(Gold));

    private double _gold;

    public double Gold
    {
        get => _gold;
        private set => SetProperty(ref _gold, value, GoldChangedArgs);
    }

    public void AddGold(double amount)
    {
        if (amount <= 0) { return; }

        Gold += amount;
    }

    public bool TrySpendGold(double amount)
    {
        if (amount < 0 || Gold < amount) { return false; }

        Gold -= amount;
        return true;
    }

    public void Load(double gold)
    {
        Gold = Math.Max(0, gold);
    }
}
