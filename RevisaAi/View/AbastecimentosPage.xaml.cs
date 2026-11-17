using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class AbastecimentosPage : ContentPage
{
    private readonly CAbastecimentosViewModel _vm = new();

    public AbastecimentosPage()
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
        if (e.CurrentSelection.FirstOrDefault() is CAbastecimento item)
        {
            ((CollectionView)sender).SelectedItem = null;
            await _vm.AbrirFormulario(item);
        }
    }
}
