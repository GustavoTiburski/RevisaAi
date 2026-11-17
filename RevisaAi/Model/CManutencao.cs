using System.Globalization;
using SQLite;

namespace RevisaAi.Model
{
    // Tabela de manutenções realizadas em um veículo
    public class CManutencao
    {
        private static readonly CultureInfo Brasil = new("pt-BR");

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Chave estrangeira para CVeiculo
        [Indexed]
        public int VeiculoId { get; set; }

        // Ex.: Troca de óleo, Pneus, Freios...
        public string TipoServico { get; set; } = string.Empty;

        public DateTime Data { get; set; } = DateTime.Today;

        public int Km { get; set; }

        public double Valor { get; set; }

        public string Oficina { get; set; } = string.Empty;

        public bool FeitaEmOficina { get; set; } = true;

        public string Observacao { get; set; } = string.Empty;

        // ----- Propriedades só para exibição (não vão para o banco) -----

        // Nome do veículo, preenchido pelo ViewModel ao carregar a lista
        [Ignore]
        public string VeiculoNome { get; set; } = string.Empty;

        [Ignore]
        public string Detalhe => $"{VeiculoNome}  •  {Data:dd/MM/yyyy}  •  {Km.ToString("N0", Brasil)} km";

        [Ignore]
        public string Local => FeitaEmOficina ? $"Oficina: {Oficina}" : "Feita em casa";

        [Ignore]
        public string ValorTexto => Valor.ToString("C2", Brasil);
    }
}
