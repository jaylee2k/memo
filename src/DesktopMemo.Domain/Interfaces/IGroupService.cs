using System;
using System.Collections.Generic;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Requests;

namespace DesktopMemo.Domain.Interfaces;

public interface IGroupService
{
    GroupTreeNodeDto CreateGroup(CreateGroupRequest req);
    GroupTreeNodeDto UpdateGroup(UpdateGroupRequest req);
    void SoftDeleteGroup(Guid groupId);
    IList<GroupTreeNodeDto> GetGroupTree();
    Guid GetOrCreateInboxGroupId();
}
