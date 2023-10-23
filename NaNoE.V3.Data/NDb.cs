using SQLite;
using System.ComponentModel;

namespace NaNoE.V3.Data;

public class NDb : INotifyPropertyChanged
{
    public static NDb Instance { get; private set; }

    private string _filePath { get; init; }
    private SQLiteConnection _connection { get; init; }
    public NDb(string filePath)
    {
        Instance = this;
        _filePath = filePath; 
        if (!File.Exists($"{_filePath}"))
        {
            using (var stream = File.Create($"{_filePath}"))
            {
                // Silence is golden
            }
        }
        _connection = new SQLiteConnection(filePath);
        _connection.CreateTable<Elements>();
        _connection.CreateTable<Helpers>();
        _connection.CreateTable<HelperItems>();
        _connection.CreateTable<ChangeLog>();
        _connection.CreateTable<TimeTracking>();
    }

    public static TableQuery<Elements> Element => NDb.Instance.Elements;
    public static TableQuery<Helpers> Helper => NDb.Instance.Helpers;
    public static TableQuery<HelperItems> HelperItem => NDb.Instance.HelperItems;
    public static TableQuery<ChangeLog> ChangeLog => NDb.Instance.ChangeLogs;
    public static TableQuery<TimeTracking> TimeTracking => NDb.Instance.TimeTrackings;

    public TableQuery<Elements> Elements => _connection.Table<Elements>();
    public TableQuery<Helpers> Helpers => _connection.Table<Helpers>();
    public TableQuery<HelperItems> HelperItems => _connection.Table<HelperItems>();
    public TableQuery<ChangeLog> ChangeLogs => _connection.Table<ChangeLog>();
    public TableQuery<TimeTracking> TimeTrackings => _connection.Table<TimeTracking>();
    

    public void AddElement(Data.Elements element)
    {
        // TODO: log
        _connection.Insert(element);
        _connection.SaveTransactionPoint();
    }

    public void RemoveElement(Data.Elements elemenet)
    {
        // TODO: log
        _connection.Delete(elemenet);
        _connection.SaveTransactionPoint();
    }

#region review
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// The Position we are in the map
    /// </summary>
    private int _position = 0;
    public int Position
    {
        get { return _position; }
        set { _position = value; }
    }

    /// <summary>
    /// If we are connected to a novel DB
    /// </summary>
    private bool _connected;
    public bool Connected
    {
        get { return _connected; }
    }

    /// <summary>
    /// Private data
    /// </summary>
    private List<int> _map;

    /// <summary>
    /// Find position of element ID in map.
    /// </summary>
    /// <param name="splt">Element ID to find in map.</param>
    /// <returns>Position of ID, if found in map</returns>
    public int MapPosition(int splt)
    {
        return _map.IndexOf(splt) + 1;
    }

    /// <summary>
    /// Create the map of the order of elements
    /// </summary>
    private void GenerateMap()
    {
        _map = new List<int>();

        //var list = _sqlConnection.CreateCommand();
        //list.CommandText = "SELECT id, idbefore, idafter FROM elements";
        //var answer = list.ExecuteReader();
        List<(long, long, long)> powerList = new List<(long, long, long)>();
        var items = NDb.Element.ToList();
        foreach (var item in items)
        {
            var a = item.Id;
            var b = item.IdBefore;
            var c = item.IdAfter;
            powerList.Add((a, b, c));
        }

        var first = (from item in powerList
                     where item.Item2 == -1 /* Starts at -1 */
                     select item).FirstOrDefault();
        if (first.ToString() != "(0, 0, 0)")
        {
            _map.Add((int)first.Item1);
            int next = (int)first.Item3;
            powerList.Remove(first);

            while (next != -2) /* Ends at -2 */
            {
                first = (from item in powerList
                         where item.Item1 == next
                         select item).First();
                _map.Add((int)first.Item1);
                next = (int)first.Item3;
                powerList.Remove(first);
            }
        }

        _position = _map.Count;
        UpdateNItems();
        RefreshWordsBefore();
    }

    /// <summary>
    /// Insert an element 
    /// Item Types
    ///  - 0: para
    ///  - 1: chap
    ///  - 2: note
    ///  - 3: bookmark
    /// </summary>
    /// <param name="nitem">See summary</param>
    /// <param name="data">Data to put in to that element</param>
    /// <param name="ignored">If the element is ignored in the editing</param>
    public void InsertElement(int nitem, string data, bool ignored)
    {
        int idbefore = -1;
        int idafter = -2;
        if (!(_position < 1))
        {
            idbefore = _map[_position - 1];
            if (_map.Count > _position)
            {
                idafter = _map[_position];
            }
        }
        else
        {
            try { idafter = _map[0]; }
            catch { }
        }

        var newElements = new Elements()
        {
            IdBefore = idbefore,
            IdAfter = idafter,
            NItem = nitem,
            SData = data,
            Ignored = ignored
        };
        NDb.Instance.AddElement(newElements);


        if (nitem == 0)
        {
            _wordCount += data.Split(' ').Length;
        }

        // update before if not -1
        if (-1 != idbefore)
        {
            var idBeforeItem = NDb.Element.Where(it => it.Id == idbefore).First();
            idBeforeItem.IdAfter = idafter;
        }

        // update after if not -2
        if (-2 != idafter)
        {
            var idAfterItem = NDb.Element.Where(it => it.Id == idafter).First();
            idAfterItem.IdBefore = idbefore;
        }

        _connection.SaveTransactionPoint();

        // update position
        _map.Insert(_position, newElements.Id);
        ++_position;

        UpdateNItems();
    }

    private Helpers _selectedHelper = null;
    public Helpers SelectedHelper
    {
        get => _selectedHelper;
        set
        {
            _selectedHelper = value;
            var args = new PropertyChangedEventArgs("SelectedHelper");
            PropertyChanged?.Invoke(this, args);
        }
    }

    /// <summary>
    /// The style of helper, e.g. '[C:M]' => main character
    /// </summary>
    /// <param name="name">Name of helper, e.g. 'Joe Soap'</param>
    /// <param name="hStyle">Style of helper, e.g. '[C:M]' character main</param>
    public void InsertHelper(string name, string hStyle)
    {
        if (hStyle.Contains("[A:"))
        {
            // work out position
            var splt = hStyle.Split(':');
            var id = int.Parse(splt[1].Substring(0, splt[1].Length - 1));

            //// Update all positions after position
            //var cmd2 = _sqlConnection.CreateCommand();
            //cmd2.CommandText = "SELECT count(id) FROM helpers WHERE name LIKE '[A:%';";
            //var ans2 = cmd2.ExecuteScalar();

            //for (int i = int.Parse(ans2.ToString()); i >= id; --i)
            //{
            //    var cmd3 = _sqlConnection.CreateCommand();
            //    var padder = i.ToString();
            //    if (padder.Length < 3)
            //    {
            //        while (padder.Length < 3) padder = "0" + padder;
            //    }
            //    cmd3.CommandText = "SELECT name, id FROM helpers WHERE name LIKE '[A:" + padder + "]%'";
            //    var ans3 = cmd3.ExecuteReader();
            //    ans3.Read();
            //    var line1 = ans3.GetString(0);
            //    var id1 = ans3.GetInt32(1);
            //    var secondPad = (i + 1).ToString();
            //    if (secondPad.Length < 3)
            //    {
            //        while (secondPad.Length < 3) secondPad = "0" + secondPad;
            //    }
            //    var newName = "[A:" + secondPad + "]" + line1.Substring(7);

            //    var cmd5 = _sqlConnection.CreateCommand();
            //    cmd5.CommandText = "UPDATE helpers SET name = '" + newName + "' WHERE id = " + id1 + ";";
            //    cmd5.ExecuteNonQuery();
            //}

            //// Insert position
            //var cmd4 = _sqlConnection.CreateCommand();
            //cmd4.CommandText = "INSERT INTO helpers (name) VALUES ('" + hStyle + " " + name + "');";
            //cmd4.ExecuteNonQuery();
        }
        else
        {
            //var cmd1 = _sqlConnection.CreateCommand();
            //cmd1.CommandText = "INSERT INTO helpers (name) VALUES ('" + StringFormat(hStyle) + " " + StringFormat(name) + "');";
            //cmd1.ExecuteNonQuery();
        }
        RefreshHelpers();
    }

    public void ImportHelper(string in_name, string helper)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "INSERT INTO helpers (name) VALUES ('" + helper + " " + in_name + "');";
        //cmd.ExecuteNonQuery();

        RefreshHelpers();
    }

    public List<HelperItems> GetHelperItems(int id)
    {
        var answer = new List<HelperItems>();

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id, data FROM helperitems WHERE helperid = " + id + ";";
        //var ans = cmd.ExecuteReader();

        //if (ans.HasRows)
        //{
        //    while (ans.Read())
        //    {
        //        answer.Add(new NHelperItem(ans.GetInt32(0), id, ans.GetString(1)));
        //    }
        //}

        return answer;
    }

    public void InsertHelperNote(Helpers helper, string text)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "INSERT INTO helperitems (helperid, data) VALUES (" + helper.ID + ", '" + StringFormat(text) + "');";
        //cmd.ExecuteNonQuery();

        //var cmd1 = _sqlConnection.CreateCommand();
        //cmd1.CommandText = "SELECT max(id) FROM helperitems;";
        //var ans = cmd1.ExecuteScalar();

        //helper.Items.Add(new NHelperItem(int.Parse(ans.ToString()), helper.ID, text));
    }

    private void RefreshHelpers()
    {
        SelectedHelper = null;
        //Helpers.Clear();

        //if (_connected)
        //{
        //    var cmd = _sqlConnection.CreateCommand();
        //    cmd.CommandText = "SELECT id, name FROM helpers;";
        //    var ans = cmd.ExecuteReader();
        //    var unsorted = new List<NHelper>();
        //    if (ans.HasRows)
        //    {
        //        while (ans.Read())
        //        {
        //            unsorted.Add(new NHelper(
        //                ans.GetInt32(0),
        //                ans.GetString(1))
        //                );
        //        }
        //    }
        //    if (unsorted.Count > 0)
        //    {
        //        Helpers.AddRange(unsorted.OrderBy(a => a).ToList());
        //    }
        //}
    }


    List<char> _numeric = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    public void DeleteHelper(int iD)
    {
        //var helper = Helpers.Find(a => a.ID == iD);

        //if (helper.Name.Contains("[A:"))
        //{
        //    var num = int.Parse(helper.Name.Substring(3, 3));
        //    foreach (var hi in Helpers)
        //    {
        //        if (hi.Name.Contains("[A:"))
        //        {
        //            var numeri = int.Parse(hi.Name.Substring(3, 3));
        //            if (numeri > num)
        //            {
        //                string newname = (numeri - 1).ToString();
        //                while (newname.Length < 3) newname = "0" + newname;
        //                newname = "[A:" + newname + "]" + hi.Name.Substring(7);

        //                var cmd3 = _sqlConnection.CreateCommand();
        //                cmd3.CommandText = "UPDATE helpers SET name = '" + newname + "' WHERE id = " + hi.ID + ";";
        //                cmd3.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //}

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "DELETE FROM helpers WHERE id = " + iD + ";";
        //cmd.ExecuteNonQuery();

        //var cmd2 = _sqlConnection.CreateCommand();
        //cmd.CommandText = "DELETE FROM helperitems WHERE helperid = " + iD + ";";
        //cmd.ExecuteNonQuery();

        RefreshHelpers();
    }

    public void DeleteHelperItem(int iD)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "DELETE FROM helperitems WHERE id = " + iD + ";";
        //cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Export entire novel (chapters and paragraphs only) as a list of string
    /// </summary>
    /// <returns>List of Novel elements</returns>
    public List<string> ExportGetNovel() // NItem?
    {
        var ans = new List<string>();
        //for (int i = 0; i < _map.Count; ++i)
        //{
        //    if (CheckElement(_map[i], "01"))
        //    {
        //        ans.Add(GetElement(_map[i]));
        //    }
        //}
        return ans;
    }

    /// <summary>
    /// Get element by ID
    /// </summary>
    /// <param name="v">The ID to get</param>
    /// <returns>NItem of the element at that ID</returns>
    private NItem GetElement(int v)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id, nitem, sdata FROM elements WHERE id = " + v;
        //var ans = cmd.ExecuteReader();
        //ans.Read();
        //var item = ans.GetInt32(1);
        //var what = ControlType.Paragraph;
        //bool flagging = true;
        //switch (item)
        //{
        //    case 0: flagging = false; break;
        //    case 1: what = ControlType.Chapter; break;
        //    case 2: what = ControlType.Note; break;
        //    case 3: what = ControlType.Bookmark; break;
        //}

        //return new NItem(what, ans.GetInt32(0), ans.GetString(2), flagging);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve RAW element data from the novel DB, this is just in case something goes wrong and all the data is needed back.
    /// </summary>
    /// <returns>List of all elements in DB</returns>
    public List<string> GetRaw()
    {
        List<string> answer = new List<string>();
        answer.Add(
            "id" + "; \t" +
            "idbefore" + "; \t" +
            "idafter" + "; \t" +
            "nitem" + "; \t" +
            "sdata"
            );

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id, idbefore, idafter, nitem, sdata FROM elements";
        //var reader = cmd.ExecuteReader();
        //while (reader.Read())
        //{
        //    answer.Add(
        //        reader.GetInt64(0) + "; \t" +
        //        reader.GetInt64(1) + "; \t" +
        //        reader.GetInt64(2) + "; \t" +
        //        reader.GetInt64(3) + "; \t" +
        //        reader.GetString(4)
        //        );
        //}

        //answer.Add("\n\n[Helpers]\n");

        //var cmd2 = _sqlConnection.CreateCommand();
        //cmd2.CommandText = "SELECT id, name FROM helpers;";
        //var answer2 = cmd2.ExecuteReader();

        //if (answer2.HasRows)
        //{
        //    while (answer2.Read())
        //    {
        //        var a_id = answer2.GetInt32(0);
        //        var a_name = answer2.GetString(1);

        //        answer.Add(" - " + a_name);

        //        var cmd3 = _sqlConnection.CreateCommand();
        //        cmd3.CommandText = "SELECT data FROM helperitems WHERE helperid = " + a_id + ";";
        //        var answer3 = cmd3.ExecuteReader();

        //        if (answer3.HasRows)
        //        {
        //            while (answer3.Read())
        //            {
        //                answer.Add("\t - " + answer3.GetString(0));
        //            }
        //        }

        //        answer.Add("\n");
        //    }
        //}

        return answer;
    }

    /// <summary>
    /// Check if an Element fits in a category
    ///  - e.g. "01" implies chapter and paragraph only
    /// </summary>
    /// <param name="v">The ID</param>
    /// <param name="controlTypes">The control types to check</param>
    /// <returns>Bool</returns>
    private bool CheckElement(int v, string controlTypes)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT nitem FROM elements WHERE id = " + v;
        //var ans = cmd.ExecuteScalar();

        //return controlTypes.Contains(ans.ToString());

        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the RAW list of NItems out the novel DB
    /// </summary>
    /// <returns>List of Elements in DB</returns>
    public List<NItem> ExportGetRaw()
    {
        var ans = new List<NItem>();
        //for (int i = 0; i < _map.Count; ++i)
        //{
        //    if (CheckElement(_map[i], "0123"))
        //    {
        //        ans.Add(GetElement(_map[i]));
        //    }
        //}
        return ans;
    }

    /// <summary>
    /// Formats string for sqlite syntax
    ///  - i.e. replaces ['] with ['']
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string StringFormat(string data)
    {
        string answer = data.Replace("'", "''");
        data = data.Replace("\r", String.Empty).Replace("\n", String.Empty);
        data = data.Trim();
        return answer;
    }

    /// <summary>
    /// Get the element at the current position in the map
    /// </summary>
    /// <returns>Element at the current position</returns>
    public NItem GetPosition()
    {
        //if (_position < 1) return null;

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id, nitem, sdata, ignored FROM elements WHERE id = " + _map[_position - 1];
        //var ans = cmd.ExecuteReader();
        //ans.Read();
        //var item = ans.GetInt32(1);
        //var what = ControlType.Paragraph;
        //bool flagging = true;
        //switch (item)
        //{
        //    case 0: flagging = ans.GetBoolean(3); break;
        //    case 1: what = ControlType.Chapter; break;
        //    case 2: what = ControlType.Note; break;
        //    case 3: what = ControlType.Bookmark; break;
        //}

        //return new NItem(what, ans.GetInt32(0), ans.GetString(2), flagging);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the element for the previous item in the map
    /// </summary>
    /// <returns>The element for the previous item in the map</returns>
    private NItem GetPrevious()
    {
        //if (_position < 2) return null;

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id, nitem, sdata, ignored FROM elements WHERE id = " + _map[_position - 2];
        //var ans = cmd.ExecuteReader();
        //ans.Read();
        //var item = ans.GetInt32(1);
        //var what = ControlType.Paragraph;
        //bool flagging = true;
        //switch (item)
        //{
        //    case 0: flagging = ans.GetBoolean(3); break;
        //    case 1: what = ControlType.Chapter; break;
        //    case 2: what = ControlType.Note; break;
        //    case 3: what = ControlType.Bookmark; break;
        //}

        //return new NItem(what, ans.GetInt32(0), ans.GetString(2), flagging);
        throw new NotImplementedException();
    }

    /// <summary>
    /// The number to increase or decrease the number of words in the novel
    /// </summary>
    /// <param name="incremental">The number up or down used</param>
    public void AdjustWordCount(int incremental)
    {
        if (incremental != 0) _wordCount += incremental;
    }

    /// <summary>
    /// Gets the next element in the map, if it exists
    /// </summary>
    /// <returns>The next element in the map</returns>
    public NItem GetNext()
    {
        //if (_position < _map.Count)
        //{
        //    var cmd = _sqlConnection.CreateCommand();
        //    cmd.CommandText = "SELECT id, nitem, sdata, ignored FROM elements WHERE id = " + _map[_position];
        //    var ans = cmd.ExecuteReader();
        //    ans.Read();
        //    var item = ans.GetInt32(1);
        //    var what = ControlType.Paragraph;
        //    bool flagging = true;
        //    switch (item)
        //    {
        //        case 0: flagging = ans.GetBoolean(3); break;
        //        case 1: what = ControlType.Chapter; break;
        //        case 2: what = ControlType.Note; break;
        //        case 3: what = ControlType.Bookmark; break;
        //    }

        //    return new NItem(what, ans.GetInt32(0), ans.GetString(2), flagging);
        //}

        return null;
    }

    /// <summary>
    /// Current position NItem
    /// </summary>
    private NItem _nPosition;
    public NItem NPosition
    {
        get { return _nPosition; }
    }

    /// <summary>
    /// Next position NItem
    /// </summary>
    private NItem _nNext;
    public NItem NNext
    {
        get { return _nNext; }
    }

    /// <summary>
    /// Previous position NItem
    /// </summary>
    private NItem _nPrevious;
    public NItem NPrevious
    {
        get { return _nPrevious; }
    }

    /// <summary>
    /// Refresh the NItems for the current Position
    /// </summary>
    public void UpdateNItems()
    {
        _nPosition = GetPosition();
        _nNext = GetNext();
        _nPrevious = GetPrevious();

        RefreshWordsBefore();
    }

    /// <summary>
    /// The number of NItems in the DB in the map
    /// </summary>
    public int MapSize
    {
        get
        {
            if (_map == null) return 0;
            return _map.Count;
        }
    }

    /// <summary>
    /// Count of words
    /// </summary>
    private int _wordCount;
    public int WordCount
    {
        get { return _wordCount; }
    }

    /// <summary>
    /// Count of words above current position
    /// </summary>
    private int _wordsBeforePosition;
    public int WordsBeforePosition
    {
        get => _wordsBeforePosition;
    }

    /// <summary>
    /// Get the words in paragraphs inside the DB
    /// </summary>
    private void GetWordCount()
    {
        //var countcmd = _sqlConnection.CreateCommand();
        //countcmd.CommandText = "SELECT length(sdata) - length(replace(sdata, ' ', '')) + 1 FROM elements WHERE nitem = 0;";
        //var reader = countcmd.ExecuteReader();
        //_wordCount = 0;
        //while (reader.Read())
        //{
        //    _wordCount += reader.GetInt32(0);
        //}

        RefreshWordsBefore();
    }

    /// <summary>
    /// Get the IDs of Chapter elements
    /// </summary>
    /// <returns>IDs of Chapters</returns>
    public List<int> GetChapterIDs()
    {
        List<int> answer = new List<int>();

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id FROM elements WHERE nitem = 1;";
        //var reader = cmd.ExecuteReader();

        //while (reader.Read())
        //{
        //    answer.Add(_map.FindIndex(a => a == reader.GetInt32(0)));
        //}

        return answer;
    }

    /// <summary>
    /// Get the IDs and titles of the bookmarks
    /// </summary>
    /// <returns>List of strings of IDs and names of bookmarks</returns>
    public List<string> GetBookmarks()
    {
        List<string> answer = new List<string>();

        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "SELECT id, sdata FROM elements WHERE nitem = 3;";
        //var reader = cmd.ExecuteReader();

        //while (reader.Read())
        //{
        //    answer.Add(reader.GetInt32(0) + " " + reader.GetString(1));
        //}

        return answer;
    }

    /// <summary>
    /// Update contents of an element
    /// </summary>
    /// <param name="iD">The ID to update</param>
    /// <param name="data">The data to set it to</param>
    public void UpdateData(int iD, string data)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "UPDATE elements SET sdata = '" + StringFormat(data) + "' WHERE id = " + iD.ToString() + ";";
        //var ans = cmd.ExecuteNonQuery();

        UpdateNItems();
    }

    /// <summary>
    /// Delete the current position element from the DB
    /// </summary>
    public void DeletePosition()
    {
        if (Position == 0) return; // Not in map location at all, so theres nothing here

        NItem item = GetPosition();

        int idbefore = -1;
        int idafter = -2;

        var pos = MapPosition(item.ID) - 1;
        if (pos > 0)
        {
            idbefore = _map[pos - 1];
        }
        if (pos < _map.Count - 1)
        {
            idafter = _map[pos + 1];
        }

        //if (idbefore == -1 && idafter == -2)
        //{
        //    // There is 1 element total
        //    var cmd1 = _sqlConnection.CreateCommand();
        //    cmd1.CommandText = "DELETE FROM elements WHERE id = " + item.ID;
        //    cmd1.ExecuteNonQuery();

        //    _position = 0;
        //    GetWordCount();
        //    GenerateMap();
        //}
        //else if (idbefore >= 0 && idafter == -2)
        //{
        //    // Theres only an element before
        //    var cmd2 = _sqlConnection.CreateCommand();
        //    cmd2.CommandText = "UPDATE elements SET idafter = -2 WHERE id = " + idbefore;
        //    cmd2.ExecuteNonQuery();

        //    var cmd3 = _sqlConnection.CreateCommand();
        //    cmd3.CommandText = "DELETE FROM elements WHERE id = " + item.ID;
        //    cmd3.ExecuteNonQuery();

        //    _position -= 1;
        //    GetWordCount();
        //    GenerateMap();
        //}
        //else if (idbefore == -1 && idafter >= 0)
        //{
        //    // Right at the start
        //    var cmd4 = _sqlConnection.CreateCommand();
        //    cmd4.CommandText = "UPDATE elements SET idbefore = -1 WHERE id = " + idafter;
        //    cmd4.ExecuteNonQuery();

        //    var cmd5 = _sqlConnection.CreateCommand();
        //    cmd5.CommandText = "DELETE FROM elements WHERE id = " + item.ID;
        //    cmd5.ExecuteNonQuery();

        //    GetWordCount();
        //    GenerateMap();
        //    _position = 1;
        //}
        //else
        //{
        //    // Somewhere in the middle
        //    var cmd6 = _sqlConnection.CreateCommand();
        //    cmd6.CommandText = "UPDATE elements SET idbefore = " + idbefore + " WHERE id = " + idafter;
        //    cmd6.ExecuteNonQuery();

        //    var cmd7 = _sqlConnection.CreateCommand();
        //    cmd7.CommandText = "UPDATE elements SET idafter = " + idafter + " WHERE id = " + idbefore;
        //    cmd7.ExecuteNonQuery();

        //    var cmd8 = _sqlConnection.CreateCommand();
        //    cmd8.CommandText = "DELETE FROM elements WHERE id = " + item.ID;
        //    cmd8.ExecuteNonQuery();

        //    var where = _position;
        //    GetWordCount();
        //    GenerateMap();
        //    _position = where;
        //}
    }

    /// <summary>
    /// Move along the map until we find the next element.
    /// </summary>
    public void EditJump()
    {
        //if (Position < _map.Count)
        //{
        //    bool found = false;
        //    while (!found)
        //    {
        //        Position++;
        //        if (Position < _map.Count)
        //        {
        //            var item = GetPosition();
        //            if (!item.Flagged)
        //            {

        //                if (item.CType == ControlType.Paragraph)
        //                {
        //                    var editSuggestions = EditProcessor.Instance.Process(item.Data);
        //                    if (editSuggestions.Count > 0)
        //                    {
        //                        found = true;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            found = true;
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Swap flagged on paragraphs
    /// </summary>
    /// <param name="id">ID to swap</param>
    internal void SwitchFlagged(int id, bool to)
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "UPDATE elements SET ignored = " + (to ? "true" : "false") + " WHERE id = " + id + ";";
        //cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Unflag all paragraphs
    /// </summary>
    public void ResetFlags()
    {
        //var cmd = _sqlConnection.CreateCommand();
        //cmd.CommandText = "UPDATE elements SET ignored = false WHERE nitem = 0;";
        //cmd.ExecuteNonQuery();
    }

    public Helpers GetMaxHelperID()
    {
        return Helpers.OrderByDescending(a => a.ID).First();
    }

    /// <summary>
    /// Retrieve every paragraph text
    /// </summary>
    /// <returns></returns>
    public List<string> GetWordData()
    {
        List<string> answer = new List<string>();
        for (int i = 0; i < _map.Count; ++i)
        {
            var e = GetElement(_map[i]);
            if (e.CType == ControlType.Paragraph)
            {
                answer.Add(e.Data);
            }
        }
        return answer;
    }

    /// <summary>
    /// Update word count before element we are looking at
    /// </summary>
    public void RefreshWordsBefore()
    {
        if (null == _map) return;


        int final = 0;
        int id = -1;
        // If too large => Max
        if (Position > 0 && Position == _map.Count)
        {
            id = Position - 1;
            final = FindWordCount(_map[id]);
        }
        // If too small => 0, we're at the start completely
        else if (Position <= 0)
        {
            _wordsBeforePosition = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WordsBefore"));
            return;
        }
        else
        {
            id = Position;
        }

        // Map all above position
        var idWhereWeAre = _map[id];
        var tmpMap = _map.TakeWhile(a => a != idWhereWeAre).ToList();
        foreach (var item in tmpMap)
        {
            final += FindWordCount(item);
        }
        _wordsBeforePosition = final;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WordsBefore"));

    }

    /// <summary>
    /// Get count of item
    /// </summary>
    /// <param name="id">ID of item to count</param>
    /// <returns>Count of words in item, or 0 for Chapters/Bookmarks/Notes</returns>
    private int FindWordCount(int id)
    {
        //var countcmd = _sqlConnection.CreateCommand();
        //countcmd.CommandText = "SELECT length(sdata) - length(replace(sdata, ' ', '')) + 1 FROM elements WHERE nitem = 0 AND id = " + id + ";";
        //var reader = countcmd.ExecuteReader();
        //if (reader.HasRows)
        //{
        //    reader.Read();
        //    return reader.GetInt32(0);
        //}

        return 0;
    }
#endregion
}
