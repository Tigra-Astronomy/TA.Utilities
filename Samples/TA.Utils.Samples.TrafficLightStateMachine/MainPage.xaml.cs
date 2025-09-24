using TA.Utils.Samples.TrafficLightStateMachine.ViewModels;

namespace TA.Utils.Samples.TrafficLightStateMachine;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel vm = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnStartClicked(object? sender, EventArgs e) => vm.Start();
    private void OnStopClicked(object? sender, EventArgs e)  => vm.Stop();
}
