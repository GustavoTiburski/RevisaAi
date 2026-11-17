using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class OutroGastoFormPage : ContentPage
{
    // gasto = null abre o formulário para um novo registro
    public OutroGastoFormPage(COutroGasto? gasto, List<CVeiculo> veiculos, Func<Task> aoConcluir)
    {
        InitializeComponent();
        BindingContext = new COutroGastoFormViewModel(this, gasto, veiculos, aoConcluir);
    }
}
