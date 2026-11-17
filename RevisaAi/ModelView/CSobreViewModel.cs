using RevisaAi.Util;

namespace RevisaAi.ModelView
{
    // ViewModel da tela "Ajustes e sobre": escolha do tema do aplicativo
    public class CSobreViewModel : CBaseViewModel
    {
        private bool _ehAutomatico;
        public bool EhAutomatico
        {
            get => _ehAutomatico;
            set
            {
                if (Set(ref _ehAutomatico, value) && value)
                    CTema.Aplicar(CTema.Automatico);
            }
        }

        private bool _ehClaro;
        public bool EhClaro
        {
            get => _ehClaro;
            set
            {
                if (Set(ref _ehClaro, value) && value)
                    CTema.Aplicar(CTema.Claro);
            }
        }

        private bool _ehEscuro;
        public bool EhEscuro
        {
            get => _ehEscuro;
            set
            {
                if (Set(ref _ehEscuro, value) && value)
                    CTema.Aplicar(CTema.Escuro);
            }
        }

        public CSobreViewModel()
        {
            // Marca o RadioButton do tema que está salvo
            string tema = CTema.Atual();

            _ehClaro = tema == CTema.Claro;
            _ehEscuro = tema == CTema.Escuro;
            _ehAutomatico = !_ehClaro && !_ehEscuro;
        }
    }
}
