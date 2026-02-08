using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DesktopMemo.Domain.Contracts;
using DesktopMemo.Domain.Entities;
using DesktopMemo.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using DesktopMemo.Data.Infrastructure;

namespace DesktopMemo.App.Services;

public class StickyNoteWindowService : IStickyNoteService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory _contextFactory;
    private readonly INoteService _noteService;
    private readonly Dictionary<Guid, Windows.StickyNoteWindow> _windows;

    public StickyNoteWindowService(IServiceProvider serviceProvider, IDbContextFactory contextFactory, INoteService noteService)
    {
        _serviceProvider = serviceProvider;
        _contextFactory = contextFactory;
        _noteService = noteService;
        _windows = new Dictionary<Guid, Windows.StickyNoteWindow>();
    }

    public void OpenSticky(Guid noteId)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_windows.TryGetValue(noteId, out var existing))
            {
                if (!existing.IsVisible)
                {
                    existing.Show();
                }

                existing.Activate();
                return;
            }

            var note = _noteService.GetNote(noteId);
            if (note == null)
            {
                return;
            }

            var vm = ActivatorUtilities.CreateInstance<ViewModels.StickyNoteViewModel>(_serviceProvider, noteId);
            var window = new Windows.StickyNoteWindow(vm, this)
            {
                Title = note.Title,
                Topmost = true
            };

            ApplyWindowState(window, noteId);
            window.Closed += (_, _) =>
            {
                vm.Dispose();
                _windows.Remove(noteId);
            };

            _windows[noteId] = window;
            window.Show();
            window.Activate();
        });
    }

    public void CloseSticky(Guid noteId)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!_windows.TryGetValue(noteId, out var window))
            {
                return;
            }

            window.Close();
        });
    }

    public void SaveWindowState(Guid noteId, StickyWindowStateDto state)
    {
        using (var context = _contextFactory.Create())
        {
            var row = context.StickyWindowStates.FirstOrDefault(x => x.NoteId == noteId);
            if (row == null)
            {
                row = new StickyWindowState
                {
                    NoteId = noteId
                };
                context.StickyWindowStates.Add(row);
            }

            row.Left = state.Left;
            row.Top = state.Top;
            row.Width = state.Width;
            row.Height = state.Height;
            row.IsAlwaysOnTop = state.IsAlwaysOnTop;
            row.LastOpenedAtUtc = state.LastOpenedAtUtc;
            context.SaveChanges();
        }
    }

    private void ApplyWindowState(Windows.StickyNoteWindow window, Guid noteId)
    {
        using (var context = _contextFactory.Create())
        {
            var row = context.StickyWindowStates.FirstOrDefault(x => x.NoteId == noteId);
            if (row == null)
            {
                return;
            }

            if (row.Width > 0)
            {
                window.Width = row.Width;
            }

            if (row.Height > 0)
            {
                window.Height = row.Height;
            }

            window.Left = row.Left;
            window.Top = row.Top;
            window.Topmost = row.IsAlwaysOnTop;
        }
    }
}
