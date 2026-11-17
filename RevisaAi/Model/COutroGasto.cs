using System.Globalization;
using SQLite;

namespace RevisaAi.Model
{
    // Tabela de outros gastos do veículo (acessórios, seguro, IPVA, lavagem, multa...)
    public class COutroGasto
    {
        private static readonly CultureInfo Brasil = new("pt-BR");

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Chave estrangeira para CVeiculo
        [Indexed]
        public int VeiculoId { get; set; }

        // Acessórios, Seguro, IPVA/Licenciamento, Lavagem, Multa ou Outro
        public string Categoria { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public DateTime Data { get; set; } = DateTime.Today;

        public double Valor { get; set; }

        // false = conta que ainda vai ser paga (não entra nos totais)
        public bool Pago { get; set; } = true;

        // ----- Propriedades só para exibição (não vão para o banco) -----

        // Nome do veículo, preenchido pelo ViewModel ao carregar a lista
        [Ignore]
        public string VeiculoNome { get; set; } = string.Empty;

        [Ignore]
        public string Titulo => $"{Categoria}: {Descricao}";

        [Ignore]
        public string Detalhe => $"{VeiculoNome}  •  {Data:dd/MM/yyyy}";

        [Ignore]
        public string Situacao => Pago ? "Pago" : "A pagar";

        [Ignore]
        public string ValorTexto => Valor.ToString("C2", Brasil);
    }
}
