using NaNoE.V3.Data;

namespace NaNoE.V3;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();

        Thread picker = new Thread(async () =>
        {
            // Testing POC - can open nne files on Andoid AND Windows with this
            // Delay here is the window needs to exist first before this is called or it gives an error
            //  - Idea is: First page shows details and info like V2; you can "open last novel" if its in the same place
            //  - Note: tested with a OneDrive nne on Windows; got '3398' test rows (var a); Android emulator has issues with my drives - apk built and put on my device didn't crash on same test; but cant use OneDrive directly - would be fine local Downloads, not OneDrive (at this point)
            Thread.Sleep(5000);
            var testFile = "";
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    testFile = result.FullPath;
                }
#if ANDROID
                testFile = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download/" + testFile.Split('/').Last()); // TODO: swap from create in downloads logic if I can
#endif

                var testDb = new NDb(testFile);
                var a = testDb.Elements.Where(it => true).ToList();
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
        });
        picker.Start();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}

