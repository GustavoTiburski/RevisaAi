using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.View;

namespace RevisaAi.ModelView
{
    // ViewModel da lista de veículos (consulta + filtro em tempo real)
    public class CVeiculosViewModel : CBaseViewModel
    {
        private readonly CVeiculoDAO _dao = new();

        // Todos os veículos vindos do banco
        private List<CVeiculo> _todos = new();

        // Veículos exibidos na tela (depois do filtro)
        public ObservableCollection<CVeiculo> Veiculos { get; } = new();

        // Texto digitado na barra de pesquisa
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

        // CheckBox "mostrar somente ativos"
        private bool _somenteAtivos;
        public bool SomenteAtivos
        {
            get => _somenteAtivos;
            set
            {
                if (Set(ref _somenteAtivos, value))
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

        public CVeiculosViewModel()
        {
            NovoCommand = new Command(async () => await AbrirFormulario(null));
        }

        // READ: carrega os veículos do banco
        public async Task Carregar()
        {
            try
            {
                _todos = await _dao.ListarOrdenado();
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Filtra a lista por marca, modelo ou placa
        private void AplicarFiltro()
        {
            string texto = _filtro.Trim().ToLower();

            Veiculos.Clear();

            foreach (var v in _todos)
            {
                if (_somenteAtivos && !v.Ativo)
                    continue;

                bool encontrou = texto.Length == 0
                    || v.Marca.ToLower().Contains(texto)
                    || v.Modelo.ToLower().Contains(texto)
                    || v.Placa.ToLower().Contains(texto);

                if (encontrou)
                    Veiculos.Add(v);
            }

            Resumo = $"{Veiculos.Count} de {_todos.Count}";
        }

        // Abre a tela de histórico do veículo (navegação empilhada, com botão de voltar)
        public async Task AbrirHistorico(CVeiculo veiculo)
        {
            await Shell.Current.Navigation.PushAsync(new VeiculoHistoricoPage(veiculo));
        }

        // Abre o formulário em modo modal (null = novo veículo)
        public async Task AbrirFormulario(CVeiculo? veiculo)
        {
            await Shell.Current.Navigation.PushModalAsync(new VeiculoFormPage(veiculo, Carregar));
        }
    }
}
