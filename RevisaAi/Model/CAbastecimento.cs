using System.Globalization;
using SQLite;

namespace RevisaAi.Model
{
    // Tabela de abastecimentos de um veículo
    public class CAbastecimento
    {
        private static readonly CultureInfo Brasil = new("pt-BR");

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Chave estrangeira para CVeiculo
        [Indexed]
        public int VeiculoId { get; set; }

        public DateTime Data { get; set; } = DateTime.Today;

        public int Km { get; set; }

        public double Litros { get; set; }

        public double ValorTotal { get; set; }

        // Gasolina, Etanol ou Diesel
        public string Combustivel { get; set; } = string.Empty;

        public bool TanqueCheio { get; set; }

        // ----- Propriedades só para exibição (não vão para o banco) -----

        // Nome do veículo, preenchido pelo ViewModel ao carregar a lista
        [Ignore]
        public string VeiculoNome { get; set; } = string.Empty;

        [Ignore]
        public string Titulo => $"{Litros.ToString("N2", Brasil)} L de {Combustivel}";

        [Ignore]
        public string Detalhe => $"{VeiculoNome}  •  {Data:dd/MM/yyyy}  •  {Km.ToString("N0", Brasil)} km";

        [Ignore]
        public string Tanque => TanqueCheio ? "Tanque cheio" : "Abastecimento parcial";

        [Ignore]
        public string ValorTexto => ValorTotal.ToString("C2", Brasil);
    }
}
