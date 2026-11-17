using System.Globalization;
using SQLite;

namespace RevisaAi.Model
{
    // Tabela de veículos
    public class CVeiculo
    {
        private static readonly CultureInfo Brasil = new("pt-BR");

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        public int Ano { get; set; }

        public int KmAtual { get; set; }

        // Flex, Gasolina, Etanol, Diesel ou Elétrico
        public string Combustivel { get; set; } = string.Empty;

        // Carro, Moto ou Caminhonete
        public string Tipo { get; set; } = "Carro";

        public bool Ativo { get; set; } = true;

        // ----- Propriedades só para exibição (não vão para o banco) -----

        [Ignore]
        public string Imagem => ImagemDoTipo(Tipo);

        [Ignore]
        public string Descricao => $"{Marca} {Modelo}";

        [Ignore]
        public string Detalhe => $"{Placa}  •  {Ano}  •  {KmAtual.ToString("N0", Brasil)} km  •  {Combustivel}";

        [Ignore]
        public string Situacao => Ativo ? "Ativo" : "Inativo";

        // Texto exibido nas listas de seleção (Picker)
        public override string ToString()
        {
            return $"{Marca} {Modelo} ({Placa})";
        }

        public static string ImagemDoTipo(string tipo)
        {
            if (tipo == "Moto")
                return "moto.png";

            if (tipo == "Caminhonete")
                return "caminhonete.png";

            return "carro.png";
        }
    }
}
