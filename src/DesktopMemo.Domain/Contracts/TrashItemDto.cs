using System;
using DesktopMemo.Domain.Enums;

namespace DesktopMemo.Domain.Contracts;

public class TrashItemDto
{
    public Guid Id { get; set; }
    public TrashItemType ItemType { get; set; }
    public string Name { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public TrashItemDto()
    {
        Name = string.Empty;
    }
}
