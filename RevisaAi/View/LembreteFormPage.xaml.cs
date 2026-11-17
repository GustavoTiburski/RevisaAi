using RevisaAi.Model;
using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class LembreteFormPage : ContentPage
{
    // lembrete = null abre o formulário para um novo registro
    public LembreteFormPage(CLembrete? lembrete, List<CVeiculo> veiculos, Func<Task> aoConcluir)
    {
        InitializeComponent();
        BindingContext = new CLembreteFormViewModel(this, lembrete, veiculos, aoConcluir);
    }
}
