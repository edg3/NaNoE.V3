namespace NaNoE.V3.Interact;

internal enum Loc
{
    Main
}

internal enum Platform
{
    Android,
    Windows
}

internal static class Nav
{
    internal static Navigator I { get; set; }

    internal static void GoTo(Loc loc) => I.GoTo(loc);
}

internal class Navigator
{
    private Frame _frame;
    private Platform _platform;
    public Navigator(Frame frame, Platform platform)
    {
        if (Nav.I is not null) throw new Exception("Can't have multiple navigators");
        Nav.I = this;

        _frame = frame;
        _platform = platform;
    }

    private bool _swappedLoc = false;
    public void GoTo(Loc loc)
    {
        _swappedLoc = false;

        // Load View Models
        switch (loc)
        {
            case Loc.Main:
                VML.Main.Load();
                break;
        }

        // Set the frame to the new view
        switch (_platform)
        {
            case Platform.Android:
                AndroidNav(loc);
                break;
            case Platform.Windows:
                WindowsNav(loc);
                break;
        }

        // Unload previous
        if (_swappedLoc)
        {
            switch (loc)
            {
                case Loc.Main:
                    VML.Main.Save();
                    break;
            }
        }
    }

    private void WindowsNav(Loc loc)
    {
        ContentView content = null;

        switch (loc)
        {
            case Loc.Main:
                content = new Views.Windows.MainView();
                break;
        }

        if (null != content)
        {
            _frame.Content = content.Content;
            _swappedLoc = true;
        }
    }

    private void AndroidNav(Loc loc)
    {
        ContentView content = null;

        switch (loc)
        {
            case Loc.Main:
                content = new Views.Android.MainView();
                break;
        }

        if (null != content)
        {
            _frame.Content = content.Content;
            _swappedLoc = true;
        }
    }
}
