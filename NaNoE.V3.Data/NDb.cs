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
        _connection.CreateTable<Elements>();
    }

    public TableQuery<Elements> Elements => _connection.Table<Elements>();

    public void AddElement(Data.Elements element)
    {
        _connection.Insert(element);
        _connection.SaveTransactionPoint();
    }

    public void RemoveElement(Data.Elements elemenet)
    {
        _connection.Delete(elemenet);
        _connection.SaveTransactionPoint();
    }
}
