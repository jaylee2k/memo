using System;
using DesktopMemo.Domain.Contracts;

namespace DesktopMemo.Domain.Interfaces;

public interface IStickyNoteService
{
    void OpenSticky(Guid noteId);
    void CloseSticky(Guid noteId);
    void SaveWindowState(Guid noteId, StickyWindowStateDto state);
}
