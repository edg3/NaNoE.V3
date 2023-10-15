using SQLite;

namespace NaNoE.V3.Data;

public class NDb
{
    private string _filePath { get; init; }
    private SQLiteConnection _connection { get; init; }
    public NDb(string filePath)
    {
        _filePath = filePath; 
        if (!File.Exists($"{_filePath}"))
        {
            using (var stream = File.Create($"{_filePath}"))
            {
                // Silence is golden
            }
        }
        _connection = new SQLiteConnection(filePath);
        _connection.CreateTable<Element>();
    }

    public TableQuery<Element> Elements => _connection.Table<Element>();

    public void AddElement(Data.Element element)
    {
        _connection.Insert(element);
        _connection.SaveTransactionPoint();
    }

    public void RemoveElement(Data.Element elemenet)
    {
        _connection.Delete(elemenet);
        _connection.SaveTransactionPoint();
    }
}
