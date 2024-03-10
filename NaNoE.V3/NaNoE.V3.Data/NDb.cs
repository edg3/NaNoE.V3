using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using NaNoE.V3.Data.Models;

namespace NaNoE.V3.Data;

public class NDb : DbContext
{
    public static NDb I { get; private set; }
    public NDb()
    {
        I = this;
        this.Database.EnsureCreated();
    }

    public DbSet<Item> Item { get; set; }
    public DbSet<NoteCategory> NoteCategory { get; set; }
    public DbSet<NoteItem> NoteItem { get; set; }
    public DbSet<NoteName> NoteName { get; set; }
    public DbSet<Novel> Novel { get; set; }
    public DbSet<WritingLog> WritingLog { get; set; }

    public static Novel CurrentNovel { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if ANDROID
        string appFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string dbPath = Path.Combine(appFolder, "nne_v3.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
#else
        optionsBuilder.UseSqlite("Data Source=nne_v3.db");
#endif
    }

    private enum ImportState
    {
        None,
        Paragraphs,
        Blank,
        FirstNoteTitle,
        SecondNoteInners,
        Paused
    }

    public void ImportRaw(string novelName, string desciptShort, string descriptBack, string genre, string rawText)
    {
        #if DEBUG
        var dtStart = DateTime.Now;
        #endif

        CurrentNovel = new()
        {
            Title = novelName,
            CreationDate = DateTime.Now,
            EditedDate = DateTime.Now,
            DescriptionShort = desciptShort,
            DescriptionBack = descriptBack,
            Genre = genre
        };
        Novel.Add(CurrentNovel);
        SaveChanges();

        var lines = rawText.Split('\n');
        var currentState = ImportState.None;

        var lstParas = new List<Item>();
        NoteName nn = null;

        //foreach (var row in lines)
        for (int i = 0; i < lines.Length; i++)
        {
            var row = lines[i];
            switch (currentState)
            {
                case ImportState.None:
                    currentState = ImportState.Paragraphs;
                    break;

                case ImportState.Paragraphs:
                    if (row.Trim() == "")
                    {
                        currentState = ImportState.FirstNoteTitle;
                    }
                    else
                    {
                        var splt = row.Split('\t');
                        lstParas.Add(new()
                        {
                            ID = int.Parse(splt[0].Trim().Replace(";", "")),
                            BeforeID = int.Parse(splt[2].Trim().Replace(";", "")),
                            AfterID = int.Parse(splt[1].Trim().Replace(";", "")),
                            Type = (EItemType)int.Parse(splt[3].Trim().Replace(";", "")),
                            Content = splt[4].Trim(),
                            NovelID = NDb.CurrentNovel.ID
                        });
                    }
                    break;
                case ImportState.FirstNoteTitle:
                    if (row.Trim() != "")
                    {
                        currentState = ImportState.SecondNoteInners;
                        nn = new()
                        {
                            Name = row.Trim(),
                            NovelID = NDb.CurrentNovel.ID
                        };
                        NoteName.Add(nn);
                        SaveChanges();
                        currentState = ImportState.SecondNoteInners;
                    }
                    break;
                case ImportState.SecondNoteInners:
                    if (row.Trim() == "")
                    {
                        currentState = ImportState.FirstNoteTitle;
                    }
                    else
                    {
                        var splt = row.Split("- ").Last();
                        var ni = new NoteItem
                        {
                            NoteNameID = nn.ID,
                            Content = splt
                        };
                        NoteItem.Add(ni);
                        SaveChanges();
                    }
                    break;  
            }

        }

        var prev = lstParas.Where(it => it.AfterID == -1).First();
        lstParas.Remove(prev);
        var prevId = prev.ID;
        prev.ID = 0;
        Item.Add(prev);
        SaveChanges();

        var next = lstParas.Where(it => it.AfterID == prevId).First();
        lstParas.Remove(next);
        var nextId = next.ID;
        var nextPrevId = next.BeforeID;
        next.ID = 0;
        next.AfterID = prev.ID;
        Item.Add(next);
        SaveChanges();

        prev.BeforeID = next.ID;
        SaveChanges();

        while (lstParas.Count > 0)
        {
            prev = next;
            next = lstParas.Where(it => it.ID == nextPrevId).First();
            lstParas.Remove(next);
            nextPrevId = next.BeforeID;
            next.ID = 0;
            next.AfterID = prev.ID;
            Item.Add(next);
            SaveChanges();

            prev.BeforeID = next.ID;
            SaveChanges();
        }
        #if DEBUG
        var tm = (DateTime.Now - dtStart).TotalSeconds;
        #endif
    }
}