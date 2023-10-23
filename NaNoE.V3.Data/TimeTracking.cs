namespace NaNoE.V3.Data;

// TODO: Make every action update time
// TODO: Make every action which changes numbers update this as well
public class TimeTracking
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LatestActionTime { get; set; }
    public int StartParagraphs { get; set; }
    public int EndParagraphs { get; set; }
    public int StartWords { get; set; }
    public int EndWords { get; set; }
    public int StartNotes { get; set; }
    public int EndNotes { get; set; }
    public int Startchapters { get; set; }
    public int Endchapters { get; set; }
}
