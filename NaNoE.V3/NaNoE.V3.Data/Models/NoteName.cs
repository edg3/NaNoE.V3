namespace NaNoE.V3.Data.Models;

public class NoteName : IModel
{
    public int NovelID { get; set; }
    public int NoteCategoryID { get; set; }
    public string Name { get; set; }
}
