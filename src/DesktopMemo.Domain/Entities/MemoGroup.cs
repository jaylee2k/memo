using System;
using System.Collections.Generic;

namespace DesktopMemo.Domain.Entities;

public class MemoGroup
{
    public Guid Id { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public virtual MemoGroup ParentGroup { get; set; }
    public virtual ICollection<MemoGroup> Children { get; set; }
    public virtual ICollection<Note> Notes { get; set; }

    public MemoGroup()
    {
        Name = string.Empty;
        Children = new List<MemoGroup>();
        Notes = new List<Note>();
    }
}
