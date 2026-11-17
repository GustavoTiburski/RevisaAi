using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class ManutencoesPage : ContentPage
{
    private readonly CManutencoesViewModel _vm = new();

    public ManutencoesPage()
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

    // Ao tocar em um item, abre o formulário de edição
    private async void cvLista_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CManutencao item)
        {
            ((CollectionView)sender).SelectedItem = null;
            await _vm.AbrirFormulario(item);
        }
    }
}
