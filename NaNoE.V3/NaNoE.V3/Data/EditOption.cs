namespace NaNoE.V2.Data;

/// <summary>
/// Data style for Edit suggestions
/// </summary>
public class EditOption
{
    /// <summary>
    /// The option to look for
    ///  - e.g.1: 'this' will find instances of the word 'this' in paragraphs
    ///  - e.g.2: '-ly' will find 'eventually', i.e. the word that ends in 'ly' in paragraphs
    /// </summary>
    public string Opt { get; private set; }

    /// <summary>
    /// The detail of the idea behind the suggestions
    ///  - e.g. 'minimise -ly'
    /// </summary>
    public string Detail { get; private set; }

    /// <summary>
    /// The better message for the suggestion
    ///  - e.g. 1 line: 'We should stop using words that in with -ly, they sound repetitive.'
    ///  - e.g. 2 lines: 'This is line 1.|This is line 2' - simple text formats '|' adds '\r\n\r\n'
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Create and instantiate the edit helper
    /// </summary>
    /// <param name="opt">Option</param>
    /// <param name="detail">Short detail</param>
    /// <param name="message">Edit message</param>
    public EditOption(string opt, string detail, string message)
    {
        Opt = opt;
        Detail = detail;
        Message = message;
    }

    /// <summary>
    /// Used for checking the edit options first character for '-'
    /// </summary>
    public string SubOptimal
    {
        get { return Opt.Substring(1); }
    }

    /// <summary>
    /// Simplified ToString for use in Edit Settings Window
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Opt + "; " + Detail + "; " + Message;
    }
}
