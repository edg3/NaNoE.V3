namespace NaNoE.V3.Data;

public class HelperItems
{
    /// <summary>
    /// Helper Item ID
    /// </summary>
    private int _id;
    public int ID
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    /// Helper Id this in an item for
    /// </summary>
    private int _helperID;
    public int HelperID
    {
        get => _helperID;
        set => _helperID = value; 
    }

    /// <summary>
    /// Text withing the helper, i.e. note
    /// </summary>
    private string _text;
    public string Text
    {
        get => _text;
        set => _text = value;
    }

    public override string ToString()
    {
        return Text;
    }
}