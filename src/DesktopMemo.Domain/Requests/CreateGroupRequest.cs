using System;

namespace DesktopMemo.Domain.Requests;

public class CreateGroupRequest
{
    public Guid? ParentGroupId { get; set; }
    public string Name { get; set; }

    public CreateGroupRequest()
    {
        Name = "새 그룹";
    }
}
