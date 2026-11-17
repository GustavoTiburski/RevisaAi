using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.Util;

namespace RevisaAi.ModelView
{
    // ViewModel do formulário de abastecimento (inclusão, alteração e exclusão)
    public class CAbastecimentoFormViewModel : CBaseViewModel
    {
        private readonly CAbastecimentoDAO _dao = new();
        private readonly CVeiculoDAO _veiculoDAO = new();

        private readonly Page _pagina;
        private readonly Func<Task> _aoConcluir;
        private readonly CAbastecimento _abastecimento;

        public bool EmEdicao { get; }

        public string Titulo => EmEdicao ? "Editar abastecimento" : "Novo abastecimento";

        public List<CVeiculo> Veiculos { get; }

        // ----- Campos do formulário -----

        private CVeiculo? _veiculoSelecionado;
        public CVeiculo? VeiculoSelecionado
        {
            get => _veiculoSelecionado;
            set => Set(ref _veiculoSelecionado, value);
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

        private string _litros = string.Empty;
        public string Litros
        {
            get => _litros;
            set => Set(ref _litros, value);
        }

        private string _valorTotal = string.Empty;
        public string ValorTotal
        {
            get => _valorTotal;
            set => Set(ref _valorTotal, value);
        }

        private bool _tanqueCheio;
        public bool TanqueCheio
        {
            get => _tanqueCheio;
            set => Set(ref _tanqueCheio, value);
        }

        // ----- Combustível (RadioButtons) -----

        private string _combustivel = "Gasolina";

        private bool _ehGasolina;
        public bool EhGasolina
        {
            get => _ehGasolina;
            set
            {
                if (Set(ref _ehGasolina, value) && value)
                    _combustivel = "Gasolina";
            }
        }

        private bool _ehEtanol;
        public bool EhEtanol
        {
            get => _ehEtanol;
            set
            {
                if (Set(ref _ehEtanol, value) && value)
                    _combustivel = "Etanol";
            }
        }

        private bool _ehDiesel;
        public bool EhDiesel
        {
            get => _ehDiesel;
            set
            {
                if (Set(ref _ehDiesel, value) && value)
                    _combustivel = "Diesel";
            }
        }

        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }

        public CAbastecimentoFormViewModel(Page pagina, CAbastecimento? abastecimento,
            List<CVeiculo> veiculos, Func<Task> aoConcluir)
        {
            _pagina = pagina;
            _aoConcluir = aoConcluir;
            Veiculos = veiculos;

            EmEdicao = abastecimento != null;
            _abastecimento = abastecimento ?? new CAbastecimento();

            if (EmEdicao)
            {
                _veiculoSelecionado = veiculos.FirstOrDefault(v => v.Id == _abastecimento.VeiculoId);
                _data = _abastecimento.Data;
                _km = _abastecimento.Km.ToString();
                _litros = _abastecimento.Litros.ToString("0.00", Brasil);
                _valorTotal = _abastecimento.ValorTotal.ToString("0.00", Brasil);
                _tanqueCheio = _abastecimento.TanqueCheio;
                _combustivel = _abastecimento.Combustivel;
            }
            else if (veiculos.Count == 1)
            {
                // Só existe um veículo: já deixa selecionado
                _veiculoSelecionado = veiculos[0];
            }

            _ehEtanol = _combustivel == "Etanol";
            _ehDiesel = _combustivel == "Diesel";
            _ehGasolina = !_ehEtanol && !_ehDiesel;

            SalvarCommand = new Command(async () => await Salvar());
            ExcluirCommand = new Command(async () => await Excluir());
            CancelarCommand = new Command(async () => await Fechar());
        }

        // Validação dos dados antes de acessar o banco
        private string Validar(out int km, out double litros, out double valor)
        {
            km = 0;
            litros = 0;
            valor = 0;

            if (VeiculoSelecionado == null)
                return "Selecione o veículo.";

            if (Data == null || Data.Value.Date > DateTime.Today)
                return "A data não pode ser futura.";

            if (!int.TryParse(Km, out km) || km < 0)
                return "Informe a quilometragem (somente números).";

            if (!LerDecimal(Litros, out litros) || litros <= 0)
                return "Informe a quantidade de litros. Ex.: 35,5";

            if (!LerDecimal(ValorTotal, out valor) || valor <= 0)
                return "Informe o valor total pago. Ex.: 200,00";

            return string.Empty;
        }

        // CREATE / UPDATE
        private async Task Salvar()
        {
            try
            {
                string erro = Validar(out int km, out double litros, out double valor);
                if (erro.Length > 0)
                {
                    await _pagina.DisplayAlertAsync("Atenção", erro, "OK");
                    return;
                }

                var veiculo = VeiculoSelecionado!;

                _abastecimento.VeiculoId = veiculo.Id;
                _abastecimento.Data = Data!.Value.Date;
                _abastecimento.Km = km;
                _abastecimento.Litros = litros;
                _abastecimento.ValorTotal = valor;
                _abastecimento.Combustivel = _combustivel;
                _abastecimento.TanqueCheio = TanqueCheio;

                if (EmEdicao)
                    await _dao.Alterar(_abastecimento);
                else
                    await _dao.Inserir(_abastecimento);

                // Se o km informado for maior, atualiza o km atual do veículo
                if (km > veiculo.KmAtual)
                {
                    veiculo.KmAtual = km;
                    await _veiculoDAO.Alterar(veiculo);
                }

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar(EmEdicao ? "Abastecimento alterado com sucesso!" : "Abastecimento registrado com sucesso!");
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
                    $"Deseja excluir o abastecimento de {_abastecimento.Data:dd/MM/yyyy}?",
                    "Excluir", "Cancelar");

                if (!confirmou)
                    return;

                await _dao.Excluir(_abastecimento);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar("Abastecimento excluído.");
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
