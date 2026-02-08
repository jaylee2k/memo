using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopMemo.App.Infrastructure;
using DesktopMemo.Domain.Interfaces;
using DesktopMemo.Domain.Requests;

namespace DesktopMemo.App.ViewModels;

public class StickyNoteViewModel : ObservableObject, IDisposable
{
    private readonly Guid _noteId;
    private readonly INoteService _noteService;
    private readonly Debouncer _debouncer;
    private bool _suspend;

    private string _title;
    private string _content;
    private string _fontFamily;
    private double _fontSize;
    private string _fontWeight;
    private string _fontStyle;

    public Guid NoteId => _noteId;

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                QueueSave();
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                QueueSave();
            }
        }
    }

    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }

    public string FontWeight
    {
        get => _fontWeight;
        set => SetProperty(ref _fontWeight, value);
    }

    public string FontStyle
    {
        get => _fontStyle;
        set => SetProperty(ref _fontStyle, value);
    }

    public StickyNoteViewModel(Guid noteId, INoteService noteService)
    {
        _noteId = noteId;
        _noteService = noteService;
        _debouncer = new Debouncer(TimeSpan.FromMilliseconds(500));

        _title = string.Empty;
        _content = string.Empty;
        _fontFamily = "Segoe UI";
        _fontSize = 14;
        _fontWeight = "Normal";
        _fontStyle = "Normal";

        Load();
    }

    private void Load()
    {
        _suspend = true;
        try
        {
            var note = _noteService.GetNote(_noteId);
            if (note == null)
            {
                return;
            }

            Title = note.Title;
            Content = note.ContentMarkdown;
            FontFamily = note.FontFamily;
            FontSize = note.FontSize;
            FontWeight = note.FontWeight;
            FontStyle = note.FontStyle;
        }
        finally
        {
            _suspend = false;
        }
    }

    private void QueueSave()
    {
        if (_suspend)
        {
            return;
        }

        _debouncer.Bounce(Save);
    }

    public void Save()
    {
        var current = _noteService.GetNote(_noteId);
        if (current == null)
        {
            return;
        }

        _noteService.UpdateNote(new UpdateNoteRequest
        {
            Id = _noteId,
            Title = Title,
            ContentMarkdown = Content,
            FontFamily = current.FontFamily,
            FontSize = current.FontSize,
            FontWeight = current.FontWeight,
            FontStyle = current.FontStyle,
            IsUnderline = current.IsUnderline,
            FontColorHex = current.FontColorHex,
            AlarmEnabled = current.AlarmEnabled,
            AlarmAtUtc = current.AlarmAtUtc,
            TimeZoneId = current.TimeZoneId,
            RepeatType = current.RepeatType,
            RepeatEndUtc = current.RepeatEndUtc
        });
    }

    public void Dispose()
    {
        _debouncer.Dispose();
    }
}
