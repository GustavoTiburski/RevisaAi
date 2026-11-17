using System.Collections.ObjectModel;
using RevisaAi.DAO;
using RevisaAi.Model;

namespace RevisaAi.ModelView
{
    // ViewModel do dashboard: gastos por período, veículo e categoria
    public class CRelatoriosViewModel : CBaseViewModel
    {
        private static readonly string[] NomesMeses =
        {
            "Jan", "Fev", "Mar", "Abr", "Mai", "Jun",
            "Jul", "Ago", "Set", "Out", "Nov", "Dez"
        };

        private readonly CVeiculoDAO _veiculoDAO = new();
        private readonly CManutencaoDAO _manutencaoDAO = new();
        private readonly CAbastecimentoDAO _abastecimentoDAO = new();
        private readonly COutroGastoDAO _outroGastoDAO = new();

        private List<CVeiculo> _veiculos = new();
        private List<CManutencao> _manutencoes = new();
        private List<CAbastecimento> _abastecimentos = new();
        private List<COutroGasto> _outrosGastos = new();

        // ----- Filtros (Pickers) -----

        public List<int> Anos { get; } = new();

        public List<string> Meses { get; } = new()
        {
            "Ano inteiro", "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
            "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
        };

        private List<string> _veiculosFiltro = new() { "Todos os veículos" };
        public List<string> VeiculosFiltro
        {
            get => _veiculosFiltro;
            set => Set(ref _veiculosFiltro, value);
        }

        private int _anoIndice;
        public int AnoIndice
        {
            get => _anoIndice;
            set
            {
                if (Set(ref _anoIndice, value))
                    Recalcular();
            }
        }

        // 0 = ano inteiro, 1 = janeiro ... 12 = dezembro
        private int _mesIndice;
        public int MesIndice
        {
            get => _mesIndice;
            set
            {
                if (Set(ref _mesIndice, value))
                    Recalcular();
            }
        }

        // 0 = todos os veículos
        private int _veiculoIndice;
        public int VeiculoIndice
        {
            get => _veiculoIndice;
            set
            {
                if (Set(ref _veiculoIndice, value))
                    Recalcular();
            }
        }

        // ----- Resultados exibidos na tela -----

        private string _tituloPeriodo = string.Empty;
        public string TituloPeriodo
        {
            get => _tituloPeriodo;
            set => Set(ref _tituloPeriodo, value);
        }

        private string _totalPeriodo = "R$ 0,00";
        public string TotalPeriodo
        {
            get => _totalPeriodo;
            set => Set(ref _totalPeriodo, value);
        }

        private string _quantidadeRegistros = "0";
        public string QuantidadeRegistros
        {
            get => _quantidadeRegistros;
            set => Set(ref _quantidadeRegistros, value);
        }

        private string _litros = "0 L";
        public string Litros
        {
            get => _litros;
            set => Set(ref _litros, value);
        }

        private string _precoMedioLitro = "-";
        public string PrecoMedioLitro
        {
            get => _precoMedioLitro;
            set => Set(ref _precoMedioLitro, value);
        }

        private string _tituloEvolucao = string.Empty;
        public string TituloEvolucao
        {
            get => _tituloEvolucao;
            set => Set(ref _tituloEvolucao, value);
        }

        // Barras por categoria (combustível, manutenção, acessórios...)
        public ObservableCollection<CItemRelatorio> Categorias { get; } = new();

        // Barras dos 12 meses do ano selecionado
        public ObservableCollection<CItemRelatorio> MesesDoAno { get; } = new();

        public CRelatoriosViewModel()
        {
            // Ano atual e os 5 anteriores
            int anoAtual = DateTime.Today.Year;
            for (int ano = anoAtual; ano >= anoAtual - 5; ano--)
                Anos.Add(ano);

            // Começa mostrando o mês atual
            _mesIndice = DateTime.Today.Month;
        }

        // READ: carrega tudo do banco e refaz os cálculos
        public async Task Carregar()
        {
            try
            {
                _veiculos = await _veiculoDAO.ListarOrdenado();
                _manutencoes = await _manutencaoDAO.Listar();
                _abastecimentos = await _abastecimentoDAO.Listar();
                _outrosGastos = await _outroGastoDAO.Listar();

                // O Picker pode devolver -1 antes de receber a lista; volta para o padrão
                if (_anoIndice < 0)
                    AnoIndice = 0;
                if (_mesIndice < 0)
                    MesIndice = DateTime.Today.Month;

                // Monta a lista do filtro de veículos mantendo a seleção atual
                int selecionado = _veiculoIndice;

                var nomes = new List<string> { "Todos os veículos" };
                foreach (var v in _veiculos)
                    nomes.Add(v.ToString());

                VeiculosFiltro = nomes;
                VeiculoIndice = selecionado > 0 && selecionado < nomes.Count ? selecionado : 0;

                Recalcular();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Refaz todos os números do dashboard conforme os filtros
        private void Recalcular()
        {
            int ano = Anos[_anoIndice >= 0 && _anoIndice < Anos.Count ? _anoIndice : 0];
            int mes = _mesIndice > 0 && _mesIndice <= 12 ? _mesIndice : 0;

            int veiculoId = 0;
            if (_veiculoIndice > 0 && _veiculoIndice <= _veiculos.Count)
                veiculoId = _veiculos[_veiculoIndice - 1].Id;

            // Registros do ano e do veículo escolhidos
            var manutencoes = _manutencoes
                .Where(m => m.Data.Year == ano && (veiculoId == 0 || m.VeiculoId == veiculoId)).ToList();
            var abastecimentos = _abastecimentos
                .Where(a => a.Data.Year == ano && (veiculoId == 0 || a.VeiculoId == veiculoId)).ToList();
            var outros = _outrosGastos
                .Where(g => g.Pago && g.Data.Year == ano && (veiculoId == 0 || g.VeiculoId == veiculoId)).ToList();

            // ----- Evolução mês a mês (sempre o ano inteiro) -----
            var totaisMes = new double[12];

            foreach (var m in manutencoes)
                totaisMes[m.Data.Month - 1] += m.Valor;
            foreach (var a in abastecimentos)
                totaisMes[a.Data.Month - 1] += a.ValorTotal;
            foreach (var g in outros)
                totaisMes[g.Data.Month - 1] += g.Valor;

            double maiorMes = totaisMes.Max();

            MesesDoAno.Clear();
            for (int i = 0; i < 12; i++)
            {
                MesesDoAno.Add(new CItemRelatorio
                {
                    Nome = NomesMeses[i],
                    ValorTexto = totaisMes[i].ToString("C2", Brasil),
                    Proporcao = maiorMes > 0 ? totaisMes[i] / maiorMes : 0,
                    Cor = i + 1 == mes ? Color.FromArgb("#F5A524") : Color.FromArgb("#8A8C90")
                });
            }

            TituloEvolucao = $"Gastos por mês em {ano}";

            // ----- Período escolhido (mês ou ano inteiro) -----
            if (mes > 0)
            {
                manutencoes = manutencoes.Where(m => m.Data.Month == mes).ToList();
                abastecimentos = abastecimentos.Where(a => a.Data.Month == mes).ToList();
                outros = outros.Where(g => g.Data.Month == mes).ToList();
            }

            double totalCombustivel = abastecimentos.Sum(a => a.ValorTotal);
            double totalManutencao = manutencoes.Sum(m => m.Valor);
            double totalOutros = outros.Sum(g => g.Valor);
            double total = totalCombustivel + totalManutencao + totalOutros;

            TituloPeriodo = mes > 0 ? $"Total em {Meses[mes]} de {ano}" : $"Total em {ano}";
            TotalPeriodo = total.ToString("C2", Brasil);
            QuantidadeRegistros = (manutencoes.Count + abastecimentos.Count + outros.Count).ToString();

            double litros = abastecimentos.Sum(a => a.Litros);
            Litros = $"{litros.ToString("N1", Brasil)} L";
            PrecoMedioLitro = litros > 0 ? (totalCombustivel / litros).ToString("C2", Brasil) : "-";

            // ----- Barras por categoria -----
            Categorias.Clear();
            AdicionarCategoria("Combustível", totalCombustivel, total, "#F5A524");
            AdicionarCategoria("Manutenção", totalManutencao, total, "#F5A524");

            // Uma barra para cada categoria de "outros gastos" que tiver valor
            foreach (string categoria in COutroGastoFormViewModel.ListaCategorias)
            {
                double valor = outros.Where(g => g.Categoria == categoria).Sum(g => g.Valor);

                if (valor > 0)
                    AdicionarCategoria(categoria, valor, total, "#8A8C90");
            }
        }

        private void AdicionarCategoria(string nome, double valor, double total, string cor)
        {
            double proporcao = total > 0 ? valor / total : 0;

            Categorias.Add(new CItemRelatorio
            {
                Nome = nome,
                ValorTexto = valor.ToString("C2", Brasil),
                PercentualTexto = proporcao.ToString("P0", Brasil),
                Proporcao = proporcao,
                Cor = Color.FromArgb(cor)
            });
        }
    }
}
