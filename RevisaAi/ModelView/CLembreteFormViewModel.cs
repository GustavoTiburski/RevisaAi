using System.Windows.Input;
using RevisaAi.DAO;
using RevisaAi.Model;
using RevisaAi.Util;

namespace RevisaAi.ModelView
{
    // ViewModel do formulário de lembrete (inclusão, alteração e exclusão)
    public class CLembreteFormViewModel : CBaseViewModel
    {
        private readonly CLembreteDAO _dao = new();

        private readonly Page _pagina;
        private readonly Func<Task> _aoConcluir;
        private readonly CLembrete _lembrete;

        public bool EmEdicao { get; }

        public string Titulo => EmEdicao ? "Editar lembrete" : "Novo lembrete";

        public List<CVeiculo> Veiculos { get; }

        // ----- Campos do formulário -----

        private CVeiculo? _veiculoSelecionado;
        public CVeiculo? VeiculoSelecionado
        {
            get => _veiculoSelecionado;
            set => Set(ref _veiculoSelecionado, value);
        }

        private string _descricao = string.Empty;
        public string Descricao
        {
            get => _descricao;
            set => Set(ref _descricao, value);
        }

        private string _kmAlvo = string.Empty;
        public string KmAlvo
        {
            get => _kmAlvo;
            set => Set(ref _kmAlvo, value);
        }

        private DateTime? _dataAlvo = DateTime.Today.AddMonths(6);
        public DateTime? DataAlvo
        {
            get => _dataAlvo;
            set => Set(ref _dataAlvo, value);
        }

        private bool _concluido;
        public bool Concluido
        {
            get => _concluido;
            set => Set(ref _concluido, value);
        }

        public ICommand SalvarCommand { get; }
        public ICommand ExcluirCommand { get; }
        public ICommand CancelarCommand { get; }

        public CLembreteFormViewModel(Page pagina, CLembrete? lembrete,
            List<CVeiculo> veiculos, Func<Task> aoConcluir)
        {
            _pagina = pagina;
            _aoConcluir = aoConcluir;
            Veiculos = veiculos;

            EmEdicao = lembrete != null;
            _lembrete = lembrete ?? new CLembrete();

            if (EmEdicao)
            {
                _veiculoSelecionado = veiculos.FirstOrDefault(v => v.Id == _lembrete.VeiculoId);
                _descricao = _lembrete.Descricao;
                _kmAlvo = _lembrete.KmAlvo > 0 ? _lembrete.KmAlvo.ToString() : string.Empty;
                _dataAlvo = _lembrete.DataAlvo;
                _concluido = _lembrete.Concluido;
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
        private string Validar(out int kmAlvo)
        {
            kmAlvo = 0;

            if (VeiculoSelecionado == null)
                return "Selecione o veículo.";

            if (string.IsNullOrWhiteSpace(Descricao))
                return "Informe a descrição do lembrete.";

            if (DataAlvo == null)
                return "Informe a data limite.";

            // Km alvo é opcional; se preenchido precisa ser um número válido
            if (!string.IsNullOrWhiteSpace(KmAlvo) && (!int.TryParse(KmAlvo, out kmAlvo) || kmAlvo < 0))
                return "Km alvo inválido (somente números).";

            return string.Empty;
        }

        // CREATE / UPDATE
        private async Task Salvar()
        {
            try
            {
                string erro = Validar(out int kmAlvo);
                if (erro.Length > 0)
                {
                    await _pagina.DisplayAlertAsync("Atenção", erro, "OK");
                    return;
                }

                _lembrete.VeiculoId = VeiculoSelecionado!.Id;
                _lembrete.Descricao = Descricao.Trim();
                _lembrete.KmAlvo = kmAlvo;
                _lembrete.DataAlvo = DataAlvo!.Value.Date;
                _lembrete.Concluido = Concluido;

                if (EmEdicao)
                    await _dao.Alterar(_lembrete);
                else
                    await _dao.Inserir(_lembrete);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar(EmEdicao ? "Lembrete alterado com sucesso!" : "Lembrete criado com sucesso!");
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
                    $"Deseja excluir o lembrete \"{_lembrete.Descricao}\"?",
                    "Excluir", "Cancelar");

                if (!confirmou)
                    return;

                await _dao.Excluir(_lembrete);

                await Fechar();
                await _aoConcluir();

                CMensagem.Mostrar("Lembrete excluído.");
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
