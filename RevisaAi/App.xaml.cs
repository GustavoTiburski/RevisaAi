using System.Globalization;
using RevisaAi.Util;

namespace RevisaAi
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Números, datas e moeda sempre no padrão brasileiro (ex.: 165.000 km, R$ 280,00)
            var brasil = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = brasil;
            CultureInfo.DefaultThreadCurrentUICulture = brasil;
            CultureInfo.CurrentCulture = brasil;
            CultureInfo.CurrentUICulture = brasil;

            // Aplica o tema escolhido pelo usuário (automático, claro ou escuro)
            CTema.AplicarSalvo();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var janela = new Window(new AppShell());

#if WINDOWS
            // No Windows, abre a janela com proporção de celular
            janela.Width = 420;
            janela.Height = 780;
            janela.X = 120;
            janela.Y = 10;
#endif

            return janela;
        }
    }
}
