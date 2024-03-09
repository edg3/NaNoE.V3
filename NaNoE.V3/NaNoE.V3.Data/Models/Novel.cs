namespace NaNoE.V3.Data.Models;

public class Novel : IModel
{
    public string Title { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string DescriptionShort { get; set; } = string.Empty;
    public string DescriptionBack { get; set; } = string.Empty;
}
