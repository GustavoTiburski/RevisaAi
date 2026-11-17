using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.View;

namespace RevisaAi.ModelView
{
    // ViewModel da lista de manutenções (consulta + filtro em tempo real)
    public class CManutencoesViewModel : CBaseViewModel
    {
        private readonly CManutencaoDAO _dao = new();
        private readonly CVeiculoDAO _veiculoDAO = new();

        private List<CManutencao> _todos = new();
        private List<CVeiculo> _veiculos = new();

        // Manutenções exibidas na tela (depois do filtro)
        public ObservableCollection<CManutencao> Manutencoes { get; } = new();

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

        public CManutencoesViewModel()
        {
            NovoCommand = new Command(async () => await AbrirFormulario(null));
        }

        // READ: carrega as manutenções e o nome do veículo de cada uma
        public async Task Carregar()
        {
            try
            {
                _veiculos = await _veiculoDAO.ListarOrdenado();
                _todos = await _dao.ListarOrdenado();

                foreach (var m in _todos)
                {
                    var veiculo = _veiculos.FirstOrDefault(v => v.Id == m.VeiculoId);
                    m.VeiculoNome = veiculo?.Descricao ?? "Veículo removido";
                }

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Filtra por tipo de serviço, veículo ou oficina
        private void AplicarFiltro()
        {
            string texto = _filtro.Trim().ToLower();
            double total = 0;

            Manutencoes.Clear();

            foreach (var m in _todos)
            {
                bool encontrou = texto.Length == 0
                    || m.TipoServico.ToLower().Contains(texto)
                    || m.VeiculoNome.ToLower().Contains(texto)
                    || m.Oficina.ToLower().Contains(texto);

                if (encontrou)
                {
                    Manutencoes.Add(m);
                    total += m.Valor;
                }
            }

            Resumo = $"{Manutencoes.Count} registro(s)  •  Total: {total.ToString("C2", Brasil)}";
        }

        // Abre o formulário em modo modal (null = nova manutenção)
        public async Task AbrirFormulario(CManutencao? manutencao)
        {
            if (_veiculos.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("Atenção",
                    "Cadastre um veículo antes de lançar uma manutenção.", "OK");
                return;
            }

            await Shell.Current.Navigation.PushModalAsync(
                new ManutencaoFormPage(manutencao, _veiculos, Carregar));
        }
    }
}
