using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.View;

namespace RevisaAi.ModelView
{
    // ViewModel da lista de lembretes (consulta + filtro em tempo real)
    public class CLembretesViewModel : CBaseViewModel
    {
        private readonly CLembreteDAO _dao = new();
        private readonly CVeiculoDAO _veiculoDAO = new();

        private List<CLembrete> _todos = new();
        private List<CVeiculo> _veiculos = new();

        // Lembretes exibidos na tela (depois do filtro)
        public ObservableCollection<CLembrete> Lembretes { get; } = new();

        private string _filtro = string.Empty;
        public string Filtro
        {
            get => _filtro;
            set
            {
                if (Set(ref _filtro, value ?? string.Empty))
                    AplicarFiltro();
            }
        }

        // CheckBox "mostrar somente pendentes"
        private bool _somentePendentes;
        public bool SomentePendentes
        {
            get => _somentePendentes;
            set
            {
                if (Set(ref _somentePendentes, value))
                    AplicarFiltro();
            }
        }

        private string _resumo = string.Empty;
        public string Resumo
        {
            get => _resumo;
            set => Set(ref _resumo, value);
        }

        public ICommand NovoCommand { get; }

        public CLembretesViewModel()
        {
            NovoCommand = new Command(async () => await AbrirFormulario(null));
        }

        // READ: carrega os lembretes e o nome do veículo de cada um
        public async Task Carregar()
        {
            try
            {
                _veiculos = await _veiculoDAO.ListarOrdenado();
                _todos = await _dao.ListarOrdenado();

                foreach (var l in _todos)
                {
                    var veiculo = _veiculos.FirstOrDefault(v => v.Id == l.VeiculoId);
                    l.VeiculoNome = veiculo?.Descricao ?? "Veículo removido";
                }

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Filtra por descrição ou veículo
        private void AplicarFiltro()
        {
            string texto = _filtro.Trim().ToLower();

            Lembretes.Clear();

            foreach (var l in _todos)
            {
                if (_somentePendentes && l.Concluido)
                    continue;

                bool encontrou = texto.Length == 0
                    || l.Descricao.ToLower().Contains(texto)
                    || l.VeiculoNome.ToLower().Contains(texto);

                if (encontrou)
                    Lembretes.Add(l);
            }

            Resumo = $"{Lembretes.Count} de {_todos.Count}";
        }

        // Abre o formulário em modo modal (null = novo lembrete)
        public async Task AbrirFormulario(CLembrete? lembrete)
        {
            if (_veiculos.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("Atenção",
                    "Cadastre um veículo antes de criar um lembrete.", "OK");
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(
                new LembreteFormPage(lembrete, _veiculos, Carregar));
        }
    }
}
