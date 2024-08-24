using NaNoE.V3.Data;

namespace NaNoE.V3;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        CheckAndRequestStoragePermissions();

        try
        {
            new NDb();

            new ViewModelLocator();
            Nav.I.Content = frmContent;
#if WINDOWS
            Nav.GoTo(Loc.Testing);
#else
            Nav.GoTo(Loc.SelectNovel);
#endif
        }
        catch { }
    }

    private async void CheckAndRequestStoragePermissions()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                // Handle permission denied scenario
                return;
            }
        }

        status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                // Handle permission denied scenario
                return;
            }
        }
    }
}
