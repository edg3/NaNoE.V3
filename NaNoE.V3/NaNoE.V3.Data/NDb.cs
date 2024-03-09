using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using NaNoE.V3.Data.Models;

namespace NaNoE.V3.Data;

public class NDb : DbContext
{
    public DbSet<Item> Items { get; set; }
    public DbSet<NoteCategory> NoteCategories { get; set; }
    public DbSet<NoteItem> NoteItems { get; set; }
    public DbSet<NoteName> NoteNames { get; set; }
    public DbSet<Novel> Novels { get; set; }
    public DbSet<WritingLog> WritingLogs { get; set; }

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

    public void ImportRaw(string rawText)
    {
        // id; 	idbefore; 	idafter; 	nitem; 	sdata
        // rows; then states blank lines till [Helpers]

        // then go through each row
        // - ignore blanks
        // - content = title, e.g. [C] Sidd Fiddlehorn
        // - each line after its noted, e.g. 30 years old, birthday 4/7/1990, ...
        // blank => go back to top after saving - ignore blanks
    }
}