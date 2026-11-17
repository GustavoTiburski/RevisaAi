using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class AbastecimentoFormPage : ContentPage
{
    // abastecimento = null abre o formulário para um novo registro
    public AbastecimentoFormPage(CAbastecimento? abastecimento, List<CVeiculo> veiculos, Func<Task> aoConcluir)
    {
        InitializeComponent();
        BindingContext = new CAbastecimentoFormViewModel(this, abastecimento, veiculos, aoConcluir);
    }
}
