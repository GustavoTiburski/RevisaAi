using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.Util;

namespace RevisaAi.ModelView
{
    // ViewModel do formulário de outros gastos (inclusão, alteração e exclusão)
    public class COutroGastoFormViewModel : CBaseViewModel
    {
        // Categorias disponíveis (também usadas na tela de relatórios)
        public static readonly List<string> ListaCategorias = new()
        {
            "Acessórios", "Seguro", "IPVA/Licenciamento", "Lavagem", "Multa", "Outro"
        };

        private readonly COutroGastoDAO _dao = new();

        private readonly Page _pagina;
        private readonly Func<Task> _aoConcluir;
        private readonly COutroGasto _gasto;

        public bool EmEdicao { get; }

        public string Titulo => EmEdicao ? "Editar gasto" : "Novo gasto";

        public List<CVeiculo> Veiculos { get; }

        public List<string> Categorias => ListaCategorias;

        // ----- Campos do formulário -----

        private CVeiculo? _veiculoSelecionado;
        public CVeiculo? VeiculoSelecionado
        {
            get => _veiculoSelecionado;
            set => Set(ref _veiculoSelecionado, value);
        }

        private string? _categoria;
        public string? Categoria
        {
            get => _categoria;
            set => Set(ref _categoria, value);
        }

        private string _descricao = string.Empty;
        public string Descricao
        {
            get => _descricao;
            set => Set(ref _descricao, value);
        }

        private DateTime? _data = DateTime.Today;
        public DateTime? Data
        {
            get => _data;
            set => Set(ref _data, value);
        }

        private string _valor = string.Empty;
        public string Valor
        {
            get => _valor;
            set => Set(ref _valor, value);
        }

        private bool _pago = true;
        public bool Pago
        {
            get => _pago;
            set => Set(ref _pago, value);
        }

        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }

        public COutroGastoFormViewModel(Page pagina, COutroGasto? gasto,
            List<CVeiculo> veiculos, Func<Task> aoConcluir)
        {
            _pagina = pagina;
            _aoConcluir = aoConcluir;
            Veiculos = veiculos;

            EmEdicao = gasto != null;
            _gasto = gasto ?? new COutroGasto();

            if (EmEdicao)
            {
                _veiculoSelecionado = veiculos.FirstOrDefault(v => v.Id == _gasto.VeiculoId);
                _categoria = _gasto.Categoria;
                _descricao = _gasto.Descricao;
                _data = _gasto.Data;
                _valor = _gasto.Valor.ToString("0.00", Brasil);
                _pago = _gasto.Pago;
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
        private string Validar(out double valor)
        {
            valor = 0;

            if (VeiculoSelecionado == null)
                return "Selecione o veículo.";

            if (string.IsNullOrEmpty(Categoria))
                return "Selecione a categoria.";

            if (string.IsNullOrWhiteSpace(Descricao))
                return "Informe a descrição do gasto.";

            if (Data == null)
                return "Informe a data.";

            if (Pago && Data.Value.Date > DateTime.Today)
                return "Um gasto já pago não pode ter data futura.";

            if (!LerDecimal(Valor, out valor) || valor <= 0)
                return "Informe um valor maior que zero. Ex.: 120,00";

            return string.Empty;
        }

        // CREATE / UPDATE
        private async Task Salvar()
        {
            try
            {
                string erro = Validar(out double valor);
                if (erro.Length > 0)
                {
                    await _pagina.DisplayAlertAsync("Atenção", erro, "OK");
                    return;
                }

                _gasto.VeiculoId = VeiculoSelecionado!.Id;
                _gasto.Categoria = Categoria ?? string.Empty;
                _gasto.Descricao = Descricao.Trim();
                _gasto.Data = Data!.Value.Date;
                _gasto.Valor = valor;
                _gasto.Pago = Pago;

                if (EmEdicao)
                    await _dao.Alterar(_gasto);
                else
                    await _dao.Inserir(_gasto);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar(EmEdicao ? "Gasto alterado com sucesso!" : "Gasto registrado com sucesso!");
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
                    $"Deseja excluir o gasto \"{_gasto.Descricao}\"?",
                    "Excluir", "Cancelar");

                if (!confirmou)
                    return;

                await _dao.Excluir(_gasto);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar("Gasto excluído.");
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
