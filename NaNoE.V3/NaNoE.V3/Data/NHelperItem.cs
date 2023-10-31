namespace NaNoE.V2.Data;

public class NHelperItem
{
    /// <summary>
    /// Helper Item ID
    /// </summary>
    private int _id;
    public int ID
    {
        get => _id;
    }

    /// <summary>
    /// Helper Id this in an item for
    /// </summary>
    private int _helperID;
    public int HelperID
    {
        get => _helperID;
    }

    /// <summary>
    /// Text withing the helper, i.e. note
    /// </summary>
    private string _text;
    public string Text
    {
        get => _text;
    }

    /// <summary>
    /// Create a HelperItem
    /// </summary>
    /// <param name="id">ID for this helper item</param>
    /// <param name="helperID">ID of the helper this is for</param>
    /// <param name="text">Note we added to this helper</param>
    public NHelperItem(int id, int helperID, string text)
    {
        _id = id;
        _helperID = helperID;
        _text = text;
    }

    public override string ToString()
    {
        return Text;
    }
}
