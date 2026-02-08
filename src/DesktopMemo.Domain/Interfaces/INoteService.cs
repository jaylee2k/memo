using System;
using System.Collections.Generic;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Requests;

namespace DesktopMemo.Domain.Interfaces;

public interface INoteService
{
    NoteDto CreateNote(CreateNoteRequest req);
    NoteDto UpdateNote(UpdateNoteRequest req);
    void MoveNote(Guid noteId, Guid targetGroupId);
    void SoftDeleteNote(Guid noteId);
    IList<NoteDto> GetNotesByGroup(Guid groupId);
    NoteDto GetNote(Guid noteId);
}
