using NaNoE.V3.Data;
using System.ComponentModel;

namespace NaNoE.V3.ViewModels;

public class CreateImportedNovelViewModel : IViewModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            PropertyChanged?.Invoke(this, new(nameof(Title)));
        }
    }

    private string _genre = string.Empty;
    public string Genre
    {
        get => _genre;
        set
        {
            _genre = value;
            PropertyChanged?.Invoke(this, new(nameof(Genre)));
        }
    }

    private string _descriptionShort = string.Empty;
    public string DescriptionShort
    {
        get => _descriptionShort;
        set
        {
            _descriptionShort = value;
            PropertyChanged?.Invoke(this, new(nameof(DescriptionShort)));
        }
    }

    private string _descriptionLong = string.Empty;
    public string DescriptionLong
    {
        get => _descriptionLong;
        set
        {
            _descriptionLong = value;
            PropertyChanged?.Invoke(this, new(nameof(DescriptionLong)));
        }
    }

    public string Content { get; set; } = string.Empty;

    public Command ExecImport => new(() =>
    {
        NDb.I.ImportRaw(Title, Genre, DescriptionShort, DescriptionLong, Content);
        Thread.Sleep(10000);
        Nav.GoTo(Loc.SelectNovel);
    });

    public Command Back => new(() =>
    {
        Nav.GoTo(Loc.SelectNovel);
    });

    public void Load()
    {
    }

    public void Save()
    {
    }
}
