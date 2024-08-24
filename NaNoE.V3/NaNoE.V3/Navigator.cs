namespace NaNoE.V3;

public enum Loc
{
    Testing,
    SelectNovel,
    CreateImportedNovel,
}

public static class Nav
{
    private static Navigator? _navigator;
    public static Navigator I => _navigator ??= new Navigator();

    public static void GoTo(Loc loc) => I.GoTo(loc);
}

public class Navigator
{
    public Frame? Content { get; set; }

    private Loc _lastLoc = Loc.Testing;
    public void GoTo(Loc loc)
    {
        View content = null;
        switch (loc)
        {
            case Loc.Testing:
                VML.Testing.Load();
                content = new Views.Windows.TestingView();
                break;
            case Loc.SelectNovel:
                VML.SelectNovel.Load();
                content = new Views.Mobile.SelectNovelView();
                break;
            case Loc.CreateImportedNovel:
                VML.CreateImportedNovel.Load();
                content = new Views.Mobile.CreateImportedNovelView();
                break;
        }

        if (content is not null)
        {
            Content.Content = null;
            Content.Content = content;

            switch (_lastLoc)
            {
                case Loc.Testing:
                    VML.Testing.Save();
                    break;
                case Loc.SelectNovel:
                    if (loc is not Loc.SelectNovel) VML.SelectNovel.Save();
                    break;
                case Loc.CreateImportedNovel:
                    VML.CreateImportedNovel.Save();
                    break;
            }
            _lastLoc = loc;
        }
    }
}
