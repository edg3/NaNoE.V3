using NaNoE.V3.ViewModels;

namespace NaNoE.V3;

public static class VML
{
    public static ViewModelLocator I => ViewModelLocator.I;

    public static TestingViewModel Testing => I.TestingViewModel;
}

public class ViewModelLocator
{
    public static ViewModelLocator I { get; private set; }
    public ViewModelLocator()
    {
        I = this;
    }

    public TestingViewModel TestingViewModel { get; } = new();
}
