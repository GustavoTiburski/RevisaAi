using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.Util;

namespace RevisaAi.ModelView
{
    // ViewModel do formulário de manutenção (inclusão, alteração e exclusão)
    public class CManutencaoFormViewModel : CBaseViewModel
    {
        private readonly CManutencaoDAO _dao = new();
        private readonly CVeiculoDAO _veiculoDAO = new();

        private readonly Page _pagina;
        private readonly Func<Task> _aoConcluir;
        private readonly CManutencao _manutencao;

        public bool EmEdicao { get; }

        public string Titulo => EmEdicao ? "Editar manutenção" : "Nova manutenção";

        public List<CVeiculo> Veiculos { get; }

        public List<string> TiposServico { get; } = new()
        {
            "Troca de óleo", "Filtros", "Pneus", "Freios", "Suspensão",
            "Elétrica", "Alinhamento e balanceamento", "Revisão geral", "Outro"
        };

        // ----- Campos do formulário -----

        private CVeiculo? _veiculoSelecionado;
        public CVeiculo? VeiculoSelecionado
        {
            get => _veiculoSelecionado;
            set => Set(ref _veiculoSelecionado, value);
        }

        private string? _tipoServico;
        public string? TipoServico
        {
            get => _tipoServico;
            set => Set(ref _tipoServico, value);
        }

        private DateTime? _data = DateTime.Today;
        public DateTime? Data
        {
            get => _data;
            set => Set(ref _data, value);
        }

        private string _km = string.Empty;
        public string Km
        {
            get => _km;
            set => Set(ref _km, value);
        }

        private string _valor = string.Empty;
        public string Valor
        {
            get => _valor;
            set => Set(ref _valor, value);
        }

        private bool _feitaEmOficina = true;
        public bool FeitaEmOficina
        {
            get => _feitaEmOficina;
            set => Set(ref _feitaEmOficina, value);
        }

        private string _oficina = string.Empty;
        public string Oficina
        {
            get => _oficina;
            set => Set(ref _oficina, value);
        }

        private string _observacao = string.Empty;
        public string Observacao
        {
            get => _observacao;
            set => Set(ref _observacao, value);
        }

        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }

        public CManutencaoFormViewModel(Page pagina, CManutencao? manutencao,
            List<CVeiculo> veiculos, Func<Task> aoConcluir)
        {
            _pagina = pagina;
            _aoConcluir = aoConcluir;
            Veiculos = veiculos;

            EmEdicao = manutencao != null;
            _manutencao = manutencao ?? new CManutencao();

            if (EmEdicao)
            {
                _veiculoSelecionado = veiculos.FirstOrDefault(v => v.Id == _manutencao.VeiculoId);
                _tipoServico = _manutencao.TipoServico;
                _data = _manutencao.Data;
                _km = _manutencao.Km.ToString();
                _valor = _manutencao.Valor.ToString("0.00", Brasil);
                _feitaEmOficina = _manutencao.FeitaEmOficina;
                _oficina = _manutencao.Oficina;
                _observacao = _manutencao.Observacao;
            }
            else if (veiculos.Count == 1)
            {
                // Só existe um veículo: já deixa selecionado
                _veiculoSelecionado = veiculos[0];
            }

            SalvarCommand = new Command(async () => await Salvar());
            ExcluirCommand = new Command(async () => await Excluir());
            CancelarCommand = new Command(async () => await Fechar());
        }

        // Validação dos dados antes de acessar o banco
        private string Validar(out int km, out double valor)
        {
            km = 0;
            valor = 0;

            if (VeiculoSelecionado == null)
                return "Selecione o veículo.";

            if (string.IsNullOrEmpty(TipoServico))
                return "Selecione o tipo de serviço.";

            if (Data == null || Data.Value.Date > DateTime.Today)
                return "A data não pode ser futura.";

            if (!int.TryParse(Km, out km) || km < 0)
                return "Informe a quilometragem (somente números).";

            if (!LerDecimal(Valor, out valor) || valor <= 0)
                return "Informe um valor maior que zero. Ex.: 150,00";

            if (FeitaEmOficina && string.IsNullOrWhiteSpace(Oficina))
                return "Informe o nome da oficina.";

            return string.Empty;
        }

        // CREATE / UPDATE
        private async Task Salvar()
        {
            try
            {
                string erro = Validar(out int km, out double valor);
                if (erro.Length > 0)
                {
                    await _pagina.DisplayAlertAsync("Atenção", erro, "OK");
                    return;
                }

                var veiculo = VeiculoSelecionado!;

                _manutencao.VeiculoId = veiculo.Id;
                _manutencao.TipoServico = TipoServico ?? string.Empty;
                _manutencao.Data = Data!.Value.Date;
                _manutencao.Km = km;
                _manutencao.Valor = valor;
                _manutencao.FeitaEmOficina = FeitaEmOficina;
                _manutencao.Oficina = FeitaEmOficina ? Oficina.Trim() : string.Empty;
                _manutencao.Observacao = (Observacao ?? string.Empty).Trim();

                if (EmEdicao)
                    await _dao.Alterar(_manutencao);
                else
                    await _dao.Inserir(_manutencao);

                // Se o km informado for maior, atualiza o km atual do veículo
                if (km > veiculo.KmAtual)
                {
                    veiculo.KmAtual = km;
                    await _veiculoDAO.Alterar(veiculo);
                }

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar(EmEdicao ? "Manutenção alterada com sucesso!" : "Manutenção registrada com sucesso!");
            }
            catch (Exception ex)
            {
                await _pagina.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // DELETE (com confirmação)
        private async Task Excluir()
        {
            try
            {
                bool confirmou = await _pagina.DisplayAlertAsync(
                    "Confirmar exclusão",
                    $"Deseja excluir a manutenção \"{_manutencao.TipoServico}\"?",
                    "Excluir", "Cancelar");

                if (!confirmou)
                    return;

                await _dao.Excluir(_manutencao);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar("Manutenção excluída.");
            }
            catch (Exception ex)
            {
                await _pagina.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        private async Task Fechar()
        {
            await _pagina.Navigation.PopModalAsync();
        }
    }
}
