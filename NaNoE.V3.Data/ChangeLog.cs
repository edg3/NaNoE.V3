namespace NaNoE.V3.Data;

// TODO: Action log list - keep actions for "User specified" amount of time in settings (e.g. 2 weeks) but allow then to set it to infinite if they'd like to always come look back

public enum ChangeLogType
{
    Create,
    Update, 
    Delete,
    Move // e.g. Move paragraph up
}

public class ChangeLog
{
    public int Id { get; set; }
    public DateTime WhenOccured { get; set; }
    // e.g. "Paragraph", "Helper", etc
    public string WhatChanged { get; set; }
    public string ActionName { get; set; }
    public string ChangedFrom { get; set; }
    public string ChangedTo { get; set; }
}
