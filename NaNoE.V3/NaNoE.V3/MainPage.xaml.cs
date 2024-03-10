using NaNoE.V3.Data;

namespace NaNoE.V3;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        new NDb();

        new ViewModelLocator();
        Nav.I.Content = frmContent;
        Nav.GoTo(Loc.Testing);
    }
}
