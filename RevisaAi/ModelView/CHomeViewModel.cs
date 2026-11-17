using System.Collections.ObjectModel;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;

namespace RevisaAi.ModelView
{
    // ViewModel da tela inicial (Painel): mostra a situação do veículo escolhido
    public class CHomeViewModel : CBaseViewModel
    {
        // Preferência onde fica guardado o último veículo escolhido no painel
        private const string ChaveVeiculo = "veiculoPainel";

        // Quantidade de segmentos da barra até a próxima manutenção
        public const int TotalSegmentos = 20;

        private readonly CVeiculoDAO _veiculoDAO = new();
        private readonly CManutencaoDAO _manutencaoDAO = new();
        private readonly CAbastecimentoDAO _abastecimentoDAO = new();
        private readonly COutroGastoDAO _outroGastoDAO = new();
        private readonly CLembreteDAO _lembreteDAO = new();

        private List<CVeiculo> _veiculos = new();
        private bool _carregando;

        // ----- Veículo exibido -----

        private bool _temVeiculo;
        public bool TemVeiculo
        {
            get => _temVeiculo;
            set
            {
                if (Set(ref _temVeiculo, value))
                    OnPropertyChanged(nameof(SemVeiculo));
            }
        }

        public bool SemVeiculo => !_temVeiculo;

        private List<string> _veiculosNomes = new();
        public List<string> VeiculosNomes
        {
            get => _veiculosNomes;
            set => Set(ref _veiculosNomes, value);
        }

        private int _veiculoIndice = -1;
        public int VeiculoIndice
        {
            get => _veiculoIndice;
            set
            {
                if (!Set(ref _veiculoIndice, value) || _carregando)
                    return;

                // Trocou o veículo no seletor: guarda a escolha e refaz o painel
                if (value >= 0 && value < _veiculos.Count)
                {
                    Preferences.Set(ChaveVeiculo, _veiculos[value].Id);
                    _ = AtualizarPainel();
                }
            }
        }

        // ----- Hodômetro -----

        // Um item para cada dígito do km atual
        public ObservableCollection<string> Digitos { get; } = new();

        // ----- Próxima manutenção -----

        private string _proximaTitulo = string.Empty;
        public string ProximaTitulo
        {
            get => _proximaTitulo;
            set => Set(ref _proximaTitulo, value);
        }

        private string _proximaInfo = string.Empty;
        public string ProximaInfo
        {
            get => _proximaInfo;
            set => Set(ref _proximaInfo, value);
        }

        // A barra só aparece quando o lembrete tem km alvo
        private bool _temMedidor;
        public bool TemMedidor
        {
            get => _temMedidor;
            set => Set(ref _temMedidor, value);
        }

        private string _medidorInicio = string.Empty;
        public string MedidorInicio
        {
            get => _medidorInicio;
            set => Set(ref _medidorInicio, value);
        }

        private string _medidorFim = string.Empty;
        public string MedidorFim
        {
            get => _medidorFim;
            set => Set(ref _medidorFim, value);
        }

        // Quantos segmentos da barra ficam acesos (0 a 20)
        private int _segmentosAcesos = -1;
        public int SegmentosAcesos
        {
            get => _segmentosAcesos;
            set => Set(ref _segmentosAcesos, value);
        }

        // ----- Mostradores -----

        private string _gastoMes = "-";
        public string GastoMes
        {
            get => _gastoMes;
            set => Set(ref _gastoMes, value);
        }

        private string _consumoMedio = "-";
        public string ConsumoMedio
        {
            get => _consumoMedio;
            set => Set(ref _consumoMedio, value);
        }

        private string _precoLitro = "-";
        public string PrecoLitro
        {
            get => _precoLitro;
            set => Set(ref _precoLitro, value);
        }

        // ----- Avisos e últimos lançamentos -----

        public ObservableCollection<CItemAviso> Avisos { get; } = new();

        private bool _semAvisos = true;
        public bool SemAvisos
        {
            get => _semAvisos;
            set => Set(ref _semAvisos, value);
        }

        public ObservableCollection<CItemHistorico> Lancamentos { get; } = new();

        private bool _semLancamentos = true;
        public bool SemLancamentos
        {
            get => _semLancamentos;
            set => Set(ref _semLancamentos, value);
        }

        // ----- Navegação -----

        public ICommand IrVeiculosCommand { get; }
        public ICommand IrRegistrosCommand { get; }
        public ICommand IrLembretesCommand { get; }
        public ICommand IrRelatoriosCommand { get; }

        public CHomeViewModel()
        {
            IrVeiculosCommand = new Command(async () => await Shell.Current.GoToAsync("//veiculos"));
            IrRegistrosCommand = new Command(async () => await Shell.Current.GoToAsync("//manutencoes"));
            IrLembretesCommand = new Command(async () => await Shell.Current.GoToAsync("//lembretes"));
            IrRelatoriosCommand = new Command(async () => await Shell.Current.GoToAsync("//relatorios"));
        }

        // READ: carrega os veículos e monta o painel do veículo escolhido
        public async Task Carregar()
        {
            try
            {
                _carregando = true;

                _veiculos = await _veiculoDAO.ListarOrdenado();
                TemVeiculo = _veiculos.Count > 0;

                if (!TemVeiculo)
                    return;

                var nomes = new List<string>();
                foreach (var v in _veiculos)
                    nomes.Add(v.ToString());

                VeiculosNomes = nomes;

                // Volta para o último veículo escolhido (ou o primeiro da lista)
                int idSalvo = Preferences.Get(ChaveVeiculo, 0);
                int indice = _veiculos.FindIndex(v => v.Id == idSalvo);

                VeiculoIndice = indice >= 0 ? indice : 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
            finally
            {
                _carregando = false;
            }

            if (TemVeiculo)
                await AtualizarPainel();
        }

        // Refaz todos os números do painel para o veículo selecionado
        private async Task AtualizarPainel()
        {
            try
            {
                if (_veiculoIndice < 0 || _veiculoIndice >= _veiculos.Count)
                    return;

                var veiculo = _veiculos[_veiculoIndice];

                var manutencoes = await _manutencaoDAO.ListarPorVeiculo(veiculo.Id);
                var abastecimentos = await _abastecimentoDAO.ListarPorVeiculo(veiculo.Id);
                var outros = await _outroGastoDAO.ListarPorVeiculo(veiculo.Id);
                var lembretes = await _lembreteDAO.ListarPorVeiculo(veiculo.Id);

                var pendentes = lembretes.Where(l => !l.Concluido).ToList();

                MontarHodometro(veiculo);
                MontarProximaManutencao(veiculo, pendentes);
                MontarMostradores(manutencoes, abastecimentos, outros);
                MontarAvisos(veiculo, pendentes);
                MontarLancamentos(manutencoes, abastecimentos, outros);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Hodômetro: um dígito por casa, com zeros à esquerda
        private void MontarHodometro(CVeiculo veiculo)
        {
            string km = veiculo.KmAtual.ToString("000000");

            Digitos.Clear();
            foreach (char digito in km)
                Digitos.Add(digito.ToString());
        }

        // Próxima manutenção: o lembrete pendente com o menor km alvo
        private void MontarProximaManutencao(CVeiculo veiculo, List<CLembrete> pendentes)
        {
            var porKm = pendentes.Where(l => l.KmAlvo > 0).OrderBy(l => l.KmAlvo).FirstOrDefault();

            if (porKm != null)
            {
                int falta = porKm.KmAlvo - veiculo.KmAtual;

                ProximaTitulo = porKm.Descricao;
                ProximaInfo = falta >= 0
                    ? $"faltam {falta.ToString("N0", Brasil)} km"
                    : $"passou {(-falta).ToString("N0", Brasil)} km";

                // A barra representa os últimos 10.000 km antes do km alvo
                int inicio = Math.Max(0, porKm.KmAlvo - 10000);
                double percorrido = (double)(veiculo.KmAtual - inicio) / (porKm.KmAlvo - inicio);

                MedidorInicio = inicio.ToString("N0", Brasil);
                MedidorFim = $"{porKm.KmAlvo.ToString("N0", Brasil)} km";
                TemMedidor = true;
                SegmentosAcesos = (int)Math.Clamp(Math.Round(percorrido * TotalSegmentos), 0, TotalSegmentos);
                return;
            }

            TemMedidor = false;

            // Sem km alvo: mostra o lembrete pendente com a data mais próxima
            var porData = pendentes.OrderBy(l => l.DataAlvo).FirstOrDefault();

            if (porData != null)
            {
                ProximaTitulo = porData.Descricao;
                ProximaInfo = $"até {porData.DataAlvo:dd/MM/yyyy}";
            }
            else
            {
                ProximaTitulo = "Nenhum lembrete pendente";
                ProximaInfo = string.Empty;
            }
        }

        // Gasto do mês atual, consumo médio e preço do último litro
        private void MontarMostradores(List<CManutencao> manutencoes,
            List<CAbastecimento> abastecimentos, List<COutroGasto> outros)
        {
            DateTime hoje = DateTime.Today;

            double gasto =
                manutencoes.Where(m => m.Data.Year == hoje.Year && m.Data.Month == hoje.Month).Sum(m => m.Valor)
                + abastecimentos.Where(a => a.Data.Year == hoje.Year && a.Data.Month == hoje.Month).Sum(a => a.ValorTotal)
                + outros.Where(g => g.Pago && g.Data.Year == hoje.Year && g.Data.Month == hoje.Month).Sum(g => g.Valor);

            GastoMes = gasto.ToString("C2", Brasil);

            // Consumo médio: km rodados entre o primeiro e o último abastecimento,
            // dividido pelos litros colocados depois do primeiro
            var porKm = abastecimentos.OrderBy(a => a.Km).ToList();

            ConsumoMedio = "-";
            if (porKm.Count >= 2)
            {
                int kmRodados = porKm[porKm.Count - 1].Km - porKm[0].Km;
                double litros = porKm.Skip(1).Sum(a => a.Litros);

                if (kmRodados > 0 && litros > 0)
                    ConsumoMedio = $"{(kmRodados / litros).ToString("N1", Brasil)} km/L";
            }

            // Preço do litro no abastecimento mais recente
            var ultimo = abastecimentos.OrderByDescending(a => a.Data).FirstOrDefault();

            PrecoLitro = ultimo != null && ultimo.Litros > 0
                ? (ultimo.ValorTotal / ultimo.Litros).ToString("C2", Brasil)
                : "-";
        }

        // Avisos: até 3 lembretes pendentes, os atrasados primeiro
        private void MontarAvisos(CVeiculo veiculo, List<CLembrete> pendentes)
        {
            var lista = new List<CItemAviso>();

            foreach (var l in pendentes)
            {
                bool atrasado = l.DataAlvo.Date < DateTime.Today
                    || (l.KmAlvo > 0 && veiculo.KmAtual >= l.KmAlvo);

                string info;
                if (atrasado)
                    info = "atrasado";
                else if (l.KmAlvo > 0)
                    info = $"aos {l.KmAlvo.ToString("N0", Brasil)} km";
                else
                    info = $"até {l.DataAlvo:dd/MM}";

                lista.Add(new CItemAviso { Descricao = l.Descricao, Info = info, Atrasado = atrasado });
            }

            Avisos.Clear();
            foreach (var aviso in lista.OrderByDescending(a => a.Atrasado).Take(3))
                Avisos.Add(aviso);

            SemAvisos = Avisos.Count == 0;
        }

        // Últimos lançamentos: os 3 registros mais recentes do veículo
        private void MontarLancamentos(List<CManutencao> manutencoes,
            List<CAbastecimento> abastecimentos, List<COutroGasto> outros)
        {
            var lista = new List<CItemHistorico>();

            foreach (var m in manutencoes)
                lista.Add(new CItemHistorico { Data = m.Data, Titulo = m.TipoServico, ValorTexto = m.Valor.ToString("N2", Brasil) });

            foreach (var a in abastecimentos)
                lista.Add(new CItemHistorico { Data = a.Data, Titulo = a.Titulo, ValorTexto = a.ValorTotal.ToString("N2", Brasil) });

            foreach (var g in outros)
                lista.Add(new CItemHistorico { Data = g.Data, Titulo = g.Titulo, ValorTexto = g.Valor.ToString("N2", Brasil) });

            Lancamentos.Clear();
            foreach (var item in lista.OrderByDescending(i => i.Data).Take(3))
            {
                item.Subtitulo = item.Data.ToString("dd/MM");
                Lancamentos.Add(item);
            }

            SemLancamentos = Lancamentos.Count == 0;
        }
    }
}
