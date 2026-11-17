using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.View;

namespace RevisaAi.ModelView
{
    // ViewModel da tela de histórico: tudo o que foi gasto com um veículo
    public class CVeiculoHistoricoViewModel : CBaseViewModel
    {
        private readonly CVeiculoDAO _veiculoDAO = new();
        private readonly CManutencaoDAO _manutencaoDAO = new();
        private readonly CAbastecimentoDAO _abastecimentoDAO = new();
        private readonly COutroGastoDAO _outroGastoDAO = new();

        private readonly CVeiculo _veiculo;

        // Evita voltar duas vezes quando o veículo é excluído
        private bool _fechando;

        // ----- Dados do veículo -----

        public string Imagem => _veiculo.Imagem;
        public string Descricao => _veiculo.Descricao;
        public string Detalhe => _veiculo.Detalhe;
        public string Situacao => _veiculo.Situacao;

        // ----- Totais -----

        private string _totalGeral = "R$ 0,00";
        public string TotalGeral
        {
            get => _totalGeral;
            set => Set(ref _totalGeral, value);
        }

        private string _totalCombustivel = "R$ 0,00";
        public string TotalCombustivel
        {
            get => _totalCombustivel;
            set => Set(ref _totalCombustivel, value);
        }

        private string _totalManutencao = "R$ 0,00";
        public string TotalManutencao
        {
            get => _totalManutencao;
            set => Set(ref _totalManutencao, value);
        }

        private string _totalOutros = "R$ 0,00";
        public string TotalOutros
        {
            get => _totalOutros;
            set => Set(ref _totalOutros, value);
        }

        // Registros do veículo, do mais recente para o mais antigo
        public ObservableCollection<CItemHistorico> Itens { get; } = new();

        public ICommand EditarCommand { get; }

        public CVeiculoHistoricoViewModel(CVeiculo veiculo)
        {
            _veiculo = veiculo;

            // Abre o formulário do veículo em modo modal; ao fechar, recarrega esta tela
            EditarCommand = new Command(async () =>
                await Shell.Current.Navigation.PushModalAsync(new VeiculoFormPage(_veiculo, Carregar)));
        }

        // READ: busca todos os registros do veículo
        public async Task Carregar()
        {
            try
            {
                // Se o veículo foi excluído no formulário, volta para a lista
                if (_fechando)
                    return;

                if (await _veiculoDAO.BuscarPorId(_veiculo.Id) == null)
                {
                    if (_fechando)
                        return;

                    _fechando = true;
                    await Shell.Current.Navigation.PopAsync();
                    return;
                }

                // Atualiza o cabeçalho (os dados podem ter sido editados)
                OnPropertyChanged(nameof(Imagem));
                OnPropertyChanged(nameof(Descricao));
                OnPropertyChanged(nameof(Detalhe));
                OnPropertyChanged(nameof(Situacao));

                var manutencoes = await _manutencaoDAO.ListarPorVeiculo(_veiculo.Id);
                var abastecimentos = await _abastecimentoDAO.ListarPorVeiculo(_veiculo.Id);
                var outros = await _outroGastoDAO.ListarPorVeiculo(_veiculo.Id);

                var lista = new List<CItemHistorico>();

                foreach (var m in manutencoes)
                {
                    lista.Add(new CItemHistorico
                    {
                        Data = m.Data,
                        Titulo = m.TipoServico,
                        Subtitulo = $"Manutenção  •  {m.Data:dd/MM/yyyy}  •  {m.Km.ToString("N0", Brasil)} km",
                        ValorTexto = m.ValorTexto
                    });
                }

                foreach (var a in abastecimentos)
                {
                    lista.Add(new CItemHistorico
                    {
                        Data = a.Data,
                        Titulo = a.Titulo,
                        Subtitulo = $"Abastecimento  •  {a.Data:dd/MM/yyyy}  •  {a.Km.ToString("N0", Brasil)} km",
                        ValorTexto = a.ValorTexto
                    });
                }

                foreach (var g in outros)
                {
                    lista.Add(new CItemHistorico
                    {
                        Data = g.Data,
                        Titulo = g.Titulo,
                        Subtitulo = $"{g.Situacao}  •  {g.Data:dd/MM/yyyy}",
                        ValorTexto = g.ValorTexto
                    });
                }

                Itens.Clear();
                foreach (var item in lista.OrderByDescending(i => i.Data))
                    Itens.Add(item);

                // Totais (gastos ainda não pagos ficam de fora)
                double combustivel = abastecimentos.Sum(a => a.ValorTotal);
                double manutencao = manutencoes.Sum(m => m.Valor);
                double outrosPagos = outros.Where(g => g.Pago).Sum(g => g.Valor);

                TotalCombustivel = combustivel.ToString("C2", Brasil);
                TotalManutencao = manutencao.ToString("C2", Brasil);
                TotalOutros = outrosPagos.ToString("C2", Brasil);
                TotalGeral = (combustivel + manutencao + outrosPagos).ToString("C2", Brasil);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }
    }
}
