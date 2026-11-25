using System.Collections.Generic;
using System.ComponentModel;

public abstract class ObservableModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, PropertyChangedEventArgs args)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, args);
        return true;
    }
}
