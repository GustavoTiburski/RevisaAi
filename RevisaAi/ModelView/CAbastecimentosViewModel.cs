using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.View;

namespace RevisaAi.ModelView
{
    // ViewModel da lista de abastecimentos (consulta + filtro em tempo real)
    public class CAbastecimentosViewModel : CBaseViewModel
    {
        private readonly CAbastecimentoDAO _dao = new();
        private readonly CVeiculoDAO _veiculoDAO = new();

        private List<CAbastecimento> _todos = new();
        private List<CVeiculo> _veiculos = new();

        // Abastecimentos exibidos na tela (depois do filtro)
        public ObservableCollection<CAbastecimento> Abastecimentos { get; } = new();

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

        private string _resumo = string.Empty;
        public string Resumo
        {
            get => _resumo;
            set => Set(ref _resumo, value);
        }

        public ICommand NovoCommand { get; }

        public CAbastecimentosViewModel()
        {
            NovoCommand = new Command(async () => await AbrirFormulario(null));
        }

        // READ: carrega os abastecimentos e o nome do veículo de cada um
        public async Task Carregar()
        {
            try
            {
                _veiculos = await _veiculoDAO.ListarOrdenado();
                _todos = await _dao.ListarOrdenado();

                foreach (var a in _todos)
                {
                    var veiculo = _veiculos.FirstOrDefault(v => v.Id == a.VeiculoId);
                    a.VeiculoNome = veiculo?.Descricao ?? "Veículo removido";
                }

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Filtra por veículo ou combustível
        private void AplicarFiltro()
        {
            string texto = _filtro.Trim().ToLower();
            double total = 0;

            Abastecimentos.Clear();

            foreach (var a in _todos)
            {
                bool encontrou = texto.Length == 0
                    || a.VeiculoNome.ToLower().Contains(texto)
                    || a.Combustivel.ToLower().Contains(texto);

                if (encontrou)
                {
                    Abastecimentos.Add(a);
                    total += a.ValorTotal;
                }
            }

            Resumo = $"{Abastecimentos.Count} registro(s)  •  Total: {total.ToString("C2", Brasil)}";
        }

        // Abre o formulário em modo modal (null = novo abastecimento)
        public async Task AbrirFormulario(CAbastecimento? abastecimento)
        {
            if (_veiculos.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("Atenção",
                    "Cadastre um veículo antes de lançar um abastecimento.", "OK");
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(
                new AbastecimentoFormPage(abastecimento, _veiculos, Carregar));
        }
    }
}
