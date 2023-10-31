using NaNoE.V3.Interact.ViewModels;

namespace NaNoE.V3.Interact;

internal static class VML
{
    internal static ViewModelLocator I { get; set; }

    internal static MainViewModel Main => I.MainVM;
}

internal class ViewModelLocator
{
    public ViewModelLocator()
    {
        if (VML.I is not null) throw new Exception("Can't make multiple VMLs");
        VML.I = this;
    }

    public MainViewModel MainVM { get; init; } = new();
}
