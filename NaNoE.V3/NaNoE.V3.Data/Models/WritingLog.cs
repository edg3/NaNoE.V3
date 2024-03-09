namespace NaNoE.V3.Data.Models;

public class WritingLog : IModel
{
    public int NovelID { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime LastUpdateTime { get; set; }

    public int Start_WordCount { get; set; }
    public int Start_ParagraphCount { get; set; }
    public int Start_ChapterCount { get; set; }
    public int Start_NoteCount { get; set; }
    public int Start_BookmarkCount { get; set; }

    public int End_WordCount { get; set; }
    public int End_ParagraphCount { get; set; }
    public int End_ChapterCount { get; set; }
    public int End_NoteCount { get; set; }
    public int End_BookmarkCount { get; set; }
}
