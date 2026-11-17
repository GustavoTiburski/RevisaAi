using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.View;

namespace RevisaAi.ModelView
{
    // ViewModel da lista de outros gastos (consulta + filtro em tempo real)
    public class COutrosGastosViewModel : CBaseViewModel
    {
        private readonly COutroGastoDAO _dao = new();
        private readonly CVeiculoDAO _veiculoDAO = new();

        private List<COutroGasto> _todos = new();
        private List<CVeiculo> _veiculos = new();

        // Gastos exibidos na tela (depois do filtro)
        public ObservableCollection<COutroGasto> Gastos { get; } = new();

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

        public COutrosGastosViewModel()
        {
            NovoCommand = new Command(async () => await AbrirFormulario(null));
        }

        // READ: carrega os gastos e o nome do veículo de cada um
        public async Task Carregar()
        {
            try
            {
                _veiculos = await _veiculoDAO.ListarOrdenado();
                _todos = await _dao.ListarOrdenado();

                foreach (var g in _todos)
                {
                    var veiculo = _veiculos.FirstOrDefault(v => v.Id == g.VeiculoId);
                    g.VeiculoNome = veiculo?.Descricao ?? "Veículo removido";
                }

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Filtra por categoria, descrição ou veículo
        private void AplicarFiltro()
        {
            string texto = _filtro.Trim().ToLower();
            double total = 0;

            Gastos.Clear();

            foreach (var g in _todos)
            {
                bool encontrou = texto.Length == 0
                    || g.Categoria.ToLower().Contains(texto)
                    || g.Descricao.ToLower().Contains(texto)
                    || g.VeiculoNome.ToLower().Contains(texto);

                if (encontrou)
                {
                    Gastos.Add(g);
                    total += g.Valor;
                }
            }

            Resumo = $"{Gastos.Count} registro(s)  •  Total: {total.ToString("C2", Brasil)}";
        }

        // Abre o formulário em modo modal (null = novo gasto)
        public async Task AbrirFormulario(COutroGasto? gasto)
        {
            if (_veiculos.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("Atenção",
                    "Cadastre um veículo antes de lançar um gasto.", "OK");
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(
                new OutroGastoFormPage(gasto, _veiculos, Carregar));
        }
    }
}
