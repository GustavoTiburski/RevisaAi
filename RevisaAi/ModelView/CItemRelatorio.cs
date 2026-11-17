namespace RevisaAi.ModelView
{
    // Uma linha do dashboard: nome, valor e tamanho da barra
    public class CItemRelatorio
    {
        public string Nome { get; set; } = string.Empty;

        public string ValorTexto { get; set; } = string.Empty;

        public string PercentualTexto { get; set; } = string.Empty;

        // Tamanho da barra, de 0 a 1
        public double Proporcao { get; set; }

        public Color Cor { get; set; } = Colors.Gray;
    }
}
