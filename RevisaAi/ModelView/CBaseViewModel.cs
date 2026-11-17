using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RevisaAi.ModelView
{
    // Base de todos os ViewModels: avisa a tela quando uma propriedade muda
    public class CBaseViewModel : INotifyPropertyChanged
    {
        protected static readonly CultureInfo Brasil = new("pt-BR");

        // Converte texto em número decimal aceitando vírgula ou ponto (ex.: 150,50)
        protected static bool LerDecimal(string? texto, out double valor)
        {
            string limpo = (texto ?? string.Empty).Trim().Replace(',', '.');
            return double.TryParse(limpo, NumberStyles.Float, CultureInfo.InvariantCulture, out valor);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
        }

        // Atualiza o campo e notifica a tela; retorna false se o valor não mudou
        protected bool Set<T>(ref T campo, T valor, [CallerMemberName] string? nome = null)
        {
            if (EqualityComparer<T>.Default.Equals(campo, valor))
                return false;

            campo = valor;
            OnPropertyChanged(nome);
            return true;
        }
    }
}
