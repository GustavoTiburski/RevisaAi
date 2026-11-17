using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class VeiculoHistoricoPage : ContentPage
{
    private readonly CVeiculoHistoricoViewModel _vm;

    public VeiculoHistoricoPage(CVeiculo veiculo)
    {
        InitializeComponent();
        BindingContext = _vm = new CVeiculoHistoricoViewModel(veiculo);
    }

    // Recarrega os registros toda vez que a tela aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Carregar();
    }
}
