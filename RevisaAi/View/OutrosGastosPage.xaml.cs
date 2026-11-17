using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class OutrosGastosPage : ContentPage
{
    private readonly COutrosGastosViewModel _vm = new();

    public OutrosGastosPage()
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
        if (e.CurrentSelection.FirstOrDefault() is COutroGasto item)
        {
            ((CollectionView)sender).SelectedItem = null;
            await _vm.AbrirFormulario(item);
        }
    }
}
