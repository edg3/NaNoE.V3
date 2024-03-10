namespace NaNoE.V3.Views.Windows;

public partial class TestingView : ContentView
{
	public TestingView()
	{
		BindingContext = VML.Testing;
		InitializeComponent();
	}
}