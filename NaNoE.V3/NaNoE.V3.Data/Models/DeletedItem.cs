namespace NaNoE.V3.Data.Models;

public class DeletedItem : IModel
{
    public int NovelID { get; set; }
    public int RoughItemID { get; set; }
    public int RoughChapterID { get; set; }
    public int RoughItemType { get; set; }
    public string Content { get; set; }
}
