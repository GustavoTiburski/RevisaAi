using RevisaAi.ModelView;

namespace RevisaAi.View;

public partial class SobrePage : ContentPage
{
    public SobrePage()
    {
        InitializeComponent();
        BindingContext = new CSobreViewModel();
    }
}
