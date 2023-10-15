using NaNoE.V3.Data;

namespace NaNoE.V3;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();

#if ANDROID
        // Android works
        var testFile = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download/test.nne");
#else
        // Windows works
		var testFile = Path.Combine("Z:\\test", "test.db");
#endif

        var testDb = new NDb(testFile);
        if (testDb.Elements.Where(it => true).Count() == 0)
        {
            testDb.AddElement(new Data.Element()
            {
                IdBefore = -1,
                IdAfter = -1,
                NItem = 1,
                SData = "Cake",
                Ignored = false
            });
        }
        else
        {
            var first = testDb.Elements.Where(it => true).First();
            testDb.RemoveElement(first);
        }
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

