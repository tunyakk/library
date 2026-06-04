using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class GenreEditControl : BaseContentControl
{
    private readonly IServiceProvider _services;
    private readonly IGenreService _genreService;

    private readonly TextBox _txtName = new() { Width = 320 };
    private readonly TextBox _txtDescription = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Width = 320,
        Height = 130
    };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30 };

    private GenreDto _genre = new();

    public event EventHandler? Saved;
    public event EventHandler? Cancelled;

    public GenreEditControl(IServiceProvider services)
    {
        _services = services;
        _genreService = services.GetRequiredService<IGenreService>();

        BuildLayout();
        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

        ThemeManager.ApplyDarkTheme(this);
    }

    public void LoadGenre(GenreDto dto)
    {
        _genre = dto;
        _txtName.Text = dto.Name;
        _txtDescription.Text = dto.Description ?? string.Empty;
    }

    public void LoadEmpty()
    {
        _genre = new GenreDto();
        _txtName.Text = string.Empty;
        _txtDescription.Text = string.Empty;
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(15),
            AutoSize = true,
            Height = 220
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Название:"), 0, 0); layout.Controls.Add(_txtName, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Описание:"), 0, 1); layout.Controls.Add(_txtDescription, 1, 1);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(220, 10);
        _btnCancel.Location = new Point(330, 10);
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(buttons);
        Controls.Add(layout);
    }

    private async Task SaveAsync()
    {
        _btnSave.Enabled = false;
        try
        {
            _genre.Name = _txtName.Text;
            _genre.Description = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text;

            var result = await _genreService.SaveAsync(_genre);
            if (Ui.ReportResult(FindForm(), result))
            {
                Saved?.Invoke(this, EventArgs.Empty);
            }
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
