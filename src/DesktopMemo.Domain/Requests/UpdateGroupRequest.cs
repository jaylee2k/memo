using System;

namespace DesktopMemo.Domain.Requests;

public class UpdateGroupRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public UpdateGroupRequest()
    {
        Name = string.Empty;
    }
}
