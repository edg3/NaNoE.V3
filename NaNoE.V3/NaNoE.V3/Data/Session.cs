namespace NaNoE.V3.Data;

public class Session
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }

    public long ParagraphStart { get; set; }
    public long ParagraphEnd { get; set; }
    public long ChapterStart { get; set; }
    public long ChapterEnd { get; set; }
    public long NoteStart { get; set; }
    public long NoteEnd { get; set; }
    public long BookmarkStart { get; set; }
    public long BookmarkEnd { get; set; }
    public long WordsStart { get; set; }
    public long WordsEnd { get; set; }
}
