namespace RevisaAi.ModelView
{
    // Um aviso do painel: lembrete pendente do veículo
    public class CItemAviso
    {
        public string Descricao { get; set; } = string.Empty;

        // Ex.: "atrasado", "até 12/12" ou "aos 160.000 km"
        public string Info { get; set; } = string.Empty;

        // Luz de aviso acesa (atrasado) ou apagada (no prazo)
        public bool Atrasado { get; set; }

        public bool NoPrazo => !Atrasado;
    }
}
