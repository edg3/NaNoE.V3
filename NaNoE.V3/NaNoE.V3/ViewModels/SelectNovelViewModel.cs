using NaNoE.V3.Data;
using NaNoE.V3.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NaNoE.V3.ViewModels;

public class SelectNovelViewModel : IViewModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Novel> Novels { get; } = new();
    private Novel _selectedNovel;
    public Novel SelectedNovel
    {
        get => _selectedNovel;
        set
        {
            _selectedNovel = value;
            PropertyChanged?.Invoke(this, new(nameof(SelectedNovel)));

            if (value is not null)
            {
                PropertyChanged?.Invoke(this, new(nameof(Genre)));
                PropertyChanged?.Invoke(this, new(nameof(DescriptionShort)));
                PropertyChanged?.Invoke(this, new(nameof(DescriptionBack)));
                var items = NDb.Items.Where(it => it.NovelID == SelectedNovel.ID).ToList();
                var words = 0;
                items.Where(it => it.Type == EItemType.Paragraph).ToList().ForEach(it => words += it.Content.Split(' ').Length);
                _novelCounts = $"{items.Count} items; {(items.Where(it => it.Type == EItemType.Chapter)).Count()} chapters; {(items.Where(it => it.Type == EItemType.Paragraph)).Count()}; {words} words";
                PropertyChanged?.Invoke(this, new(nameof(Counts)));
            }
        }
    }

    public string Genre => SelectedNovel?.Genre ?? string.Empty;
    public string DescriptionShort => SelectedNovel?.DescriptionShort ?? string.Empty;
    public string DescriptionBack => SelectedNovel?.DescriptionBack ?? string.Empty;
    private string _novelCounts = string.Empty;
    public string Counts => _novelCounts;

    // ...

    private Command _importText;
    public Command ImportText => _importText ??= new(async () =>
    {
        try
        {
            FileResult fileResult = await FilePicker.PickAsync();

            if (fileResult != null)
            {
                using var fr = File.OpenRead(fileResult.FullPath);
                using var sr = new StreamReader(fr);
                var content = await sr.ReadToEndAsync();

                VML.CreateImportedNovel.Content = content;
                VML.CreateImportedNovel.Title = fileResult.FileName;
                Nav.GoTo(Loc.CreateImportedNovel);
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during file picking
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to pick a file: {ex.Message}", "OK");
        }
    });

    public void Load()
    {
        Novels.Clear();
        NDb.Novels
            .OrderBy(n => n.Title)
            .ToList()
            .ForEach(n => Novels.Add(n));
    }

    public void Save()
    {
        Novels.Clear();
    }
}
