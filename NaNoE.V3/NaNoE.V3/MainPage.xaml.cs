using NaNoE.V2.Data;
using NaNoE.V3.Interact;

namespace NaNoE.V3;

public partial class MainPage : ContentPage
{
    private ViewModelLocator _vml;
    private Navigator _nav;
    private DataConnection _con;
    public MainPage()
    {
        InitializeComponent();

        _vml = new();
        _con = DataConnection.Instance;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            _nav = new(frmContent, Interact.Platform.Android);
            frmContent.Margin = new Thickness(-10);
            // Test:
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "test.ndb");
            if (File.Exists(filePath))
            {
                _con.Open(filePath);
            }
            else
            {
                _con.Create(filePath);
            }
        }
        else // Default / Windows
        {
            _nav = new(frmContent, Interact.Platform.Windows);
            // Test:
            if (File.Exists("z:/sample.nne"))
            {
                _con.Open("z:/sample.nne");
            }
            else
            {
                _con.Create("z:/sample.nne");
            }
        }

        // TODO: look up info on this: await Navigation.PushAsync(new HelloFromAndroid());
        _nav.GoTo(Loc.Main);
    }
}