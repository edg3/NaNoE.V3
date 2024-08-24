namespace NaNoE.V3.Views.Mobile;

public partial class CreateImportedNovelView : ContentView
{
	public CreateImportedNovelView()
	{
		BindingContext = VML.CreateImportedNovel;
		InitializeComponent();
	}
}