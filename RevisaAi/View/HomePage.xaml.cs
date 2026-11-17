using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class HomePage : ContentPage
{
    private readonly CHomeViewModel _vm = new();

    // Segmentos da barra "próxima manutenção"
    private readonly List<BoxView> _segmentos = new();

    public HomePage()
    {
        InitializeComponent();
        BindingContext = _vm;

        MontarMedidor();

        // Quando o ViewModel recalcula a barra, repinta os segmentos
        _vm.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(CHomeViewModel.SegmentosAcesos))
                PintarMedidor();
        };
    }

    // Atualiza o painel toda vez que a tela aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Carregar();
        PintarMedidor();
    }

    // Cria os 20 segmentos da barra, um em cada coluna do Grid
    private void MontarMedidor()
    {
        for (int i = 0; i < CHomeViewModel.TotalSegmentos; i++)
        {
            grMedidor.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var segmento = new BoxView();
            Grid.SetColumn(segmento, i);
            grMedidor.Children.Add(segmento);

            _segmentos.Add(segmento);
        }
    }

    // Acende em âmbar os segmentos já percorridos; os demais ficam apagados
    private void PintarMedidor()
    {
        for (int i = 0; i < _segmentos.Count; i++)
        {
            if (i < _vm.SegmentosAcesos)
                _segmentos[i].Color = Color.FromArgb("#F5A524");
            else
                _segmentos[i].SetAppThemeColor(BoxView.ColorProperty,
                    Color.FromArgb("#D6D6D0"), Color.FromArgb("#2A2C30"));
        }
    }
}
