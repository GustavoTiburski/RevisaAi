using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class LembretesPage : ContentPage
{
    private readonly CLembretesViewModel _vm = new();

    public LembretesPage()
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
        if (e.CurrentSelection.FirstOrDefault() is CLembrete item)
        {
            ((CollectionView)sender).SelectedItem = null;
            await _vm.AbrirFormulario(item);
        }
    }
}
