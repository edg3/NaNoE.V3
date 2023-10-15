using SQLite;

namespace NaNoE.V3.Data;

public class NDb
{
    public static NDb Instance { get; private set; }

    private string _filePath { get; init; }
    private SQLiteConnection _connection { get; init; }
    public NDb(string filePath)
    {
        Instance = this;
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
        _connection.CreateTable<Helpers>();
        _connection.CreateTable<HelperItems>();
    }

    public TableQuery<Elements> Elements => _connection.Table<Elements>();
    public TableQuery<Helpers> Helpers => _connection.Table<Helpers>();
    public TableQuery<HelperItems> HelperItems => _connection.Table<HelperItems>();
    

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
