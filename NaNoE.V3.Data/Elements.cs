using SQLite;

namespace NaNoE.V3.Data;

public class Elements
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int IdBefore { get; set; }
    public int IdAfter { get; set; }
    public int NItem { get; set; }
    public string SData { get; set; }
    public bool Ignored { get; set; }
}