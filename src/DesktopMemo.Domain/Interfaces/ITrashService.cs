using System;
using System.Collections.Generic;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Enums;

namespace DesktopMemo.Domain.Interfaces;

public interface ITrashService
{
    IList<TrashItemDto> GetTrashItems();
    void RestoreNote(Guid noteId);
    void RestoreGroup(Guid groupId);
    void DeleteItemPermanently(Guid itemId, TrashItemType itemType);
    void PurgeExpiredItems();
    void EmptyTrash();
}
