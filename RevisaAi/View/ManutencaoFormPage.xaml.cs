using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class ManutencaoFormPage : ContentPage
{
    // manutencao = null abre o formulário para um novo registro
    public ManutencaoFormPage(CManutencao? manutencao, List<CVeiculo> veiculos, Func<Task> aoConcluir)
    {
        InitializeComponent();
        BindingContext = new CManutencaoFormViewModel(this, manutencao, veiculos, aoConcluir);
    }
}
