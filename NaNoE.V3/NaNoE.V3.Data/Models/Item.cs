namespace NaNoE.V3.Data.Models;

public class Item : IModel
{
    public int NovelID { get; set; }
    public EItemType Type { get; set; }
    public string Content { get; set; }
    // This is "before" this item of BeforeID, said item states this ID is "after" it; and like wise
    public int BeforeID { get; set; }
    public int AfterID { get; set; }
}
