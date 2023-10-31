using NaNoE.V2.Data;

namespace NaNoE.V3
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var tester = new NItem(ControlType.Bookmark, 0, "Cake", true);
        }
    }
}