using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class RelatoriosPage : ContentPage
{
    private readonly CRelatoriosViewModel _vm = new();

    public RelatoriosPage()
    {
        InitializeComponent();
        BindingContext = _vm;
    }

    // Recalcula o dashboard toda vez que a tela aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Carregar();
    }
}
