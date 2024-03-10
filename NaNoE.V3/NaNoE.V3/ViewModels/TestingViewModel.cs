using NaNoE.V3.Data;
using System.ComponentModel;

namespace NaNoE.V3.ViewModels;

public class TestingViewModel : IViewModel, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Command TestImportCommand => new(async () =>
    {
        var file = await PickFile();
        if (file != null)
        {
            using var fo = File.OpenRead(file.FullPath);
            using var sr = new StreamReader(fo);
            var text = await sr.ReadToEndAsync();
            NDb.I.ImportRaw($"test novel {DateTime.Now.ToShortDateString()}", $"{DateTime.Now.ToShortTimeString()}", "", "test", text);
        }
    });

    private async Task<FileResult> PickFile()
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Please select a 'nne' file"
            };
            var result = await FilePicker.PickAsync(options);
            if (result != null)
            {
                // Process the selected file (e.g., display its path)
                Console.WriteLine($"Selected file: {result.FullPath}");
            }
            return result;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., user canceled)
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    private string _rows = "Rows Latest Novel?";
    public string RowsInDBMsg
    {
        get => _rows;
        set
        {
            _rows = value;
            PropertyChanged?.Invoke(this, new(nameof(RowsInDBMsg)));
        }
    }
    public Command RowsInDB => new(async () =>
    {
        try
        {
            var novel = NDb.I.Novel.OrderBy(it => it.ID).LastOrDefault();
            if (novel is not null)
            {
                var cItems = NDb.I.Item.Where(it => it.NovelID == novel.ID).Count();
                var cNoteName = NDb.I.NoteName.Where(it => it.NovelID == novel.ID).Count();

                RowsInDBMsg = $"Novel: {novel.Title}\t\t{cItems} items; {cNoteName} note names";
            }
        }
        catch
        {

        }
    });

    public void Load()
    {
    }

    public void Save()
    {
    }
}
