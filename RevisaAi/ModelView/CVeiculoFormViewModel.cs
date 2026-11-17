using System.Text.RegularExpressions;
using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.Util;

namespace RevisaAi.ModelView
{
    // ViewModel do formulário de veículo (inclusão, alteração e exclusão)
    public class CVeiculoFormViewModel : CBaseViewModel
    {
        private readonly CVeiculoDAO _dao = new();
        private readonly CManutencaoDAO _manutencaoDAO = new();
        private readonly CAbastecimentoDAO _abastecimentoDAO = new();
        private readonly CLembreteDAO _lembreteDAO = new();
        private readonly COutroGastoDAO _outroGastoDAO = new();

        private readonly Page _pagina;           // página do formulário (para alertas e fechar o modal)
        private readonly Func<Task> _aoConcluir; // recarrega a lista depois de salvar/excluir
        private readonly CVeiculo _veiculo;      // registro que está sendo editado

        public bool EmEdicao { get; }

        public string Titulo => EmEdicao ? "Editar veículo" : "Novo veículo";

        public List<string> Combustiveis { get; } =
            new() { "Flex", "Gasolina", "Etanol", "Diesel", "Elétrico" };

        // ----- Campos do formulário -----

        private string _marca = string.Empty;
        public string Marca
        {
            get => _marca;
            set => Set(ref _marca, value);
        }

        private string _modelo = string.Empty;
        public string Modelo
        {
            get => _modelo;
            set => Set(ref _modelo, value);
        }

        private string _placa = string.Empty;
        public string Placa
        {
            get => _placa;
            set => Set(ref _placa, value);
        }

        // Ano e Km ficam como texto para poder validar o que foi digitado
        private string _ano = string.Empty;
        public string Ano
        {
            get => _ano;
            set => Set(ref _ano, value);
        }

        private string _kmAtual = string.Empty;
        public string KmAtual
        {
            get => _kmAtual;
            set => Set(ref _kmAtual, value);
        }

        private string? _combustivel;
        public string? Combustivel
        {
            get => _combustivel;
            set => Set(ref _combustivel, value);
        }

        private bool _ativo = true;
        public bool Ativo
        {
            get => _ativo;
            set => Set(ref _ativo, value);
        }

        // ----- Tipo do veículo (RadioButtons) -----

        private string _tipo = "Carro";

        private bool _ehCarro;
        public bool EhCarro
        {
            get => _ehCarro;
            set
            {
                if (Set(ref _ehCarro, value) && value)
                    DefinirTipo("Carro");
            }
        }

        private bool _ehMoto;
        public bool EhMoto
        {
            get => _ehMoto;
            set
            {
                if (Set(ref _ehMoto, value) && value)
                    DefinirTipo("Moto");
            }
        }

        private bool _ehCaminhonete;
        public bool EhCaminhonete
        {
            get => _ehCaminhonete;
            set
            {
                if (Set(ref _ehCaminhonete, value) && value)
                    DefinirTipo("Caminhonete");
            }
        }

        // Imagem exibida no topo do formulário, muda conforme o tipo escolhido
        public string ImagemTipo => CVeiculo.ImagemDoTipo(_tipo);

        private void DefinirTipo(string tipo)
        {
            _tipo = tipo;
            OnPropertyChanged(nameof(ImagemTipo));
        }

        // ----- Comandos (botões) -----

        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }

        public CVeiculoFormViewModel(Page pagina, CVeiculo? veiculo, Func<Task> aoConcluir)
        {
            _pagina = pagina;
            _aoConcluir = aoConcluir;

            EmEdicao = veiculo != null;
            _veiculo = veiculo ?? new CVeiculo();

            // Em edição, preenche o formulário com os dados do registro
            if (EmEdicao)
            {
                _marca = _veiculo.Marca;
                _modelo = _veiculo.Modelo;
                _placa = _veiculo.Placa;
                _ano = _veiculo.Ano.ToString();
                _kmAtual = _veiculo.KmAtual.ToString();
                _combustivel = _veiculo.Combustivel;
                _ativo = _veiculo.Ativo;
                _tipo = _veiculo.Tipo;
            }

            _ehCarro = _tipo == "Carro";
            _ehMoto = _tipo == "Moto";
            _ehCaminhonete = _tipo == "Caminhonete";

            SalvarCommand = new Command(async () => await Salvar());
            ExcluirCommand = new Command(async () => await Excluir());
            CancelarCommand = new Command(async () => await Fechar());
        }

        // Validação dos dados antes de acessar o banco
        private async Task<string> Validar(string placa)
        {
            if (string.IsNullOrWhiteSpace(Marca))
                return "Informe a marca do veículo.";

            if (string.IsNullOrWhiteSpace(Modelo))
                return "Informe o modelo do veículo.";

            // Aceita placa antiga (ABC1234) e Mercosul (ABC1D23)
            if (!Regex.IsMatch(placa, "^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$"))
                return "Placa inválida. Use o formato ABC1234 ou ABC1D23.";

            int anoMaximo = DateTime.Today.Year + 1;
            if (!int.TryParse(Ano, out int ano) || ano < 1950 || ano > anoMaximo)
                return $"Informe um ano entre 1950 e {anoMaximo}.";

            if (!int.TryParse(KmAtual, out int km) || km < 0)
                return "Informe a quilometragem atual (somente números).";

            if (string.IsNullOrEmpty(Combustivel))
                return "Selecione o combustível.";

            // Não permite dois veículos com a mesma placa
            var existente = await _dao.BuscarPorPlaca(placa);
            if (existente != null && existente.Id != _veiculo.Id)
                return "Já existe um veículo cadastrado com essa placa.";

            return string.Empty;
        }

        // CREATE / UPDATE
        private async Task Salvar()
        {
            try
            {
                string placa = (Placa ?? string.Empty).Replace("-", "").Replace(" ", "").ToUpper();

                string erro = await Validar(placa);
                if (erro.Length > 0)
                {
                    await _pagina.DisplayAlertAsync("Atenção", erro, "OK");
                    return;
                }

                _veiculo.Marca = Marca.Trim();
                _veiculo.Modelo = Modelo.Trim();
                _veiculo.Placa = placa;
                _veiculo.Ano = int.Parse(Ano);
                _veiculo.KmAtual = int.Parse(KmAtual);
                _veiculo.Combustivel = Combustivel ?? string.Empty;
                _veiculo.Tipo = _tipo;
                _veiculo.Ativo = Ativo;

                if (EmEdicao)
                    await _dao.Alterar(_veiculo);
                else
                    await _dao.Inserir(_veiculo);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar(EmEdicao ? "Veículo alterado com sucesso!" : "Veículo cadastrado com sucesso!");
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
                    $"Deseja excluir o veículo {_veiculo.Descricao}?\n" +
                    "As manutenções, abastecimentos, outros gastos e lembretes dele também serão apagados.",
                    "Excluir", "Cancelar");

                if (!confirmou)
                    return;

                // Apaga primeiro os registros ligados ao veículo
                await _manutencaoDAO.ExcluirPorVeiculo(_veiculo.Id);
                await _abastecimentoDAO.ExcluirPorVeiculo(_veiculo.Id);
                await _lembreteDAO.ExcluirPorVeiculo(_veiculo.Id);
                await _outroGastoDAO.ExcluirPorVeiculo(_veiculo.Id);
                await _dao.Excluir(_veiculo);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar("Veículo excluído.");
            }
            catch (Exception ex)
            {
                await _pagina.DisplayAlertAsync("Erro", ex.Message, "OK");
            }
        }

        // Fecha o formulário modal
        private async Task Fechar()
        {
            await _pagina.Navigation.PopModalAsync();
        }
    }
}
