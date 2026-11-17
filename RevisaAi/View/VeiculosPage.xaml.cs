using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class VeiculosPage : ContentPage
{
    private readonly CVeiculosViewModel _vm = new();

    public VeiculosPage()
    {
        InitializeComponent();
        BindingContext = _vm;
    }

    // Recarrega a lista toda vez que a tela aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Carregar();
    }

    // Ao tocar em um veículo, abre a tela de histórico dele
    private async void cvVeiculos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CVeiculo veiculo)
        {
            ((CollectionView)sender).SelectedItem = null;
            await _vm.AbrirHistorico(veiculo);
        }
    }
}
