using System;
using System.Collections.Generic;

namespace DesktopMemo.Domain.Contracts;

public class GroupTreeNodeDto
{
    public Guid Id { get; set; }
    public Guid? ParentGroupId { get; set; }
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public IList<GroupTreeNodeDto> Children { get; set; }

    public GroupTreeNodeDto()
    {
        Name = string.Empty;
        Children = new List<GroupTreeNodeDto>();
    }
}
