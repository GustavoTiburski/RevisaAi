using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class VeiculoFormPage : ContentPage
{
    // veiculo = null abre o formulário para um novo cadastro
    public VeiculoFormPage(CVeiculo? veiculo, Func<Task> aoConcluir)
    {
        InitializeComponent();
        BindingContext = new CVeiculoFormViewModel(this, veiculo, aoConcluir);
    }
}
