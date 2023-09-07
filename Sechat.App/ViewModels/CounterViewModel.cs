using CommunityToolkit.Mvvm.ComponentModel;

namespace Sechat.App.ViewModels;
public partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]

    private int _number;
}
