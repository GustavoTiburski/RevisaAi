using System.Globalization;
using SQLite;

namespace RevisaAi.Model
{
    // Tabela de lembretes de próximas manutenções
    public class CLembrete
    {
        private static readonly CultureInfo Brasil = new("pt-BR");

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Chave estrangeira para CVeiculo
        [Indexed]
        public int VeiculoId { get; set; }

        public string Descricao { get; set; } = string.Empty;

        // Km em que a manutenção deve ser feita (0 = não usa km)
        public int KmAlvo { get; set; }

        public DateTime DataAlvo { get; set; } = DateTime.Today;

        public bool Concluido { get; set; }

        // ----- Propriedades só para exibição (não vão para o banco) -----

        // Nome do veículo, preenchido pelo ViewModel ao carregar a lista
        [Ignore]
        public string VeiculoNome { get; set; } = string.Empty;

        [Ignore]
        public string Detalhe
        {
            get
            {
                string texto = $"{VeiculoNome}  •  até {DataAlvo:dd/MM/yyyy}";

                if (KmAlvo > 0)
                    texto += $"  •  aos {KmAlvo.ToString("N0", Brasil)} km";

                return texto;
            }
        }

        [Ignore]
        public string Situacao
        {
            get
            {
                if (Concluido)
                    return "Concluído";

                return DataAlvo.Date < DateTime.Today ? "Atrasado" : "Pendente";
            }
        }
    }
}
