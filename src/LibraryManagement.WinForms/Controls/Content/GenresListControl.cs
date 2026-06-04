using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class GenresListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IGenreService _genreService;
    private readonly CrudToolbar _toolbar = new();
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public GenresListControl(IServiceProvider services)
    {
        _services = services;
        _genreService = services.GetRequiredService<IGenreService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(GenreDto.Id), FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(GenreDto.Name), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = nameof(GenreDto.Description), FillWeight = 120 }
        );

        _toolbar.AddClicked += async (_, _) => await ShowEditAsync(new GenreDto());
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        ThemeManager.ApplyDarkTheme(this);
    }

    public async Task LoadDataAsync() => await ReloadAsync();

    private async Task ReloadAsync()
    {
        try { _grid.DataSource = (await _genreService.GetAllAsync(_toolbar.SearchBox.Text)).ToList(); }
        catch (Exception ex) { Ui.ShowError(FindForm(), ex.Message); }
    }

    private GenreDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as GenreDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите жанр."); return; }
        var dto = await _genreService.GetByIdAsync(current.Id) ?? new GenreDto();
        await ShowEditAsync(dto);
    }

    private async Task ShowEditAsync(GenreDto dto)
    {
        var editControl = new GenreEditControl(_services) { Dock = DockStyle.Fill };
        editControl.LoadGenre(dto);
        var contentPanel = FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault();
        editControl.Saved += async (_, _) => { NavigateBack(contentPanel); };
        editControl.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(editControl);
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите жанр."); return; }
        if (!Ui.Confirm(FindForm(), $"Удалить жанр «{current.Name}»?")) return;
        var result = await _genreService.DeleteAsync(current.Id);
        if (Ui.ReportResult(FindForm(), result, "Жанр удалён.")) await ReloadAsync();
    }

    private void NavigateBack(ContentPanel? contentPanel)
    {
        if (contentPanel == null) return;
        var list = new GenresListControl(_services) { Dock = DockStyle.Fill };
        _ = list.LoadDataAsync();
        contentPanel.ShowContent(list);
    }
}
