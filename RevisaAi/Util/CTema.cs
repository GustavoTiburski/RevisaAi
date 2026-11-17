namespace RevisaAi.Util
{
    // Guarda e aplica o tema escolhido pelo usuário (automático, claro ou escuro)
    public static class CTema
    {
        private const string Chave = "tema";

        public const string Automatico = "automatico";
        public const string Claro = "claro";
        public const string Escuro = "escuro";

        // Tema salvo nas preferências do aparelho
        public static string Atual()
        {
            return Preferences.Get(Chave, Automatico);
        }

        // Aplica o tema salvo (chamado quando o app abre)
        public static void AplicarSalvo()
        {
            Aplicar(Atual());
        }

        // Salva a escolha e troca o tema na hora
        public static void Aplicar(string tema)
        {
            Preferences.Set(Chave, tema);

            if (Application.Current == null)
                return;

            if (tema == Claro)
                Application.Current.UserAppTheme = AppTheme.Light;
            else if (tema == Escuro)
                Application.Current.UserAppTheme = AppTheme.Dark;
            else
                Application.Current.UserAppTheme = Application.Current.PlatformAppTheme; // segue o aparelho
        }
    }
}
