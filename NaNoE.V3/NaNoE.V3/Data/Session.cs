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

    internal bool ValChange()
    {
        // What if they leave the app open over night?
        // Figured it out - if last time on it was "too long ago" it doesnt update; it created new!
        return ParagraphStart != ParagraphEnd || ChapterStart != ChapterEnd || NoteStart != NoteEnd || BookmarkStart != BookmarkEnd || WordsStart != WordsEnd;
    }
}
