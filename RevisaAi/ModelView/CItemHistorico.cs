namespace RevisaAi.ModelView
{
    // Uma linha do histórico do veículo (manutenção, abastecimento ou outro gasto)
    public class CItemHistorico
    {
        public DateTime Data { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Subtitulo { get; set; } = string.Empty;

        public string ValorTexto { get; set; } = string.Empty;
    }
}
