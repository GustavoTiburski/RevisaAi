using Microsoft.Maui.Controls.Shapes;

namespace RevisaAi.Util
{
    // Mensagens rápidas ao usuário (Toast / Snackbar)
    public static class CMensagem
    {
        public static void Mostrar(string texto)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
#if ANDROID
                    // No Android usa o Toast nativo do sistema
                    global::Android.Widget.Toast.MakeText(
                        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity,
                        texto,
                        global::Android.Widget.ToastLength.Short)?.Show();
                    await Task.CompletedTask;
#else
                    // No Windows mostra uma faixa (Snackbar) no rodapé da página atual
                    await MostrarSnackbar(texto);
#endif
                }
                catch
                {
                    // Mensagem é só um aviso: se falhar, o app segue normalmente
                }
            });
        }

        private static async Task MostrarSnackbar(string texto)
        {
            var pagina = Shell.Current?.CurrentPage as ContentPage;

            // Todas as páginas do app têm um Grid como raiz
            if (pagina?.Content is not Grid grid)
                return;

            var faixa = new Border
            {
                BackgroundColor = Color.FromArgb("#F5A524"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                Padding = new Thickness(18, 10),
                Margin = new Thickness(16, 0, 16, 84),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                InputTransparent = true,
                ZIndex = 100,
                Content = new Label
                {
                    Text = texto,
                    TextColor = Color.FromArgb("#0B0C0E"),
                    FontSize = 14
                }
            };

            Grid.SetRowSpan(faixa, Math.Max(1, grid.RowDefinitions.Count));
            Grid.SetColumnSpan(faixa, Math.Max(1, grid.ColumnDefinitions.Count));

            grid.Children.Add(faixa);
            await Task.Delay(2500);
            grid.Children.Remove(faixa);
        }
    }
}
