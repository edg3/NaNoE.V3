namespace NaNoE.V3.Views.Mobile;

public partial class SelectNovelView : ContentView
{
	public SelectNovelView()
	{
		BindingContext = VML.SelectNovel;
		InitializeComponent();
	}
}