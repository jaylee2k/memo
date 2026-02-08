using System;

namespace DesktopMemo.Domain.Entities;

public class StickyWindowState
{
    public Guid NoteId { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsAlwaysOnTop { get; set; }
    public DateTime LastOpenedAtUtc { get; set; }

    public virtual Note Note { get; set; }
}
