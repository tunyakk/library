using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class AuthorsListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IAuthorService _authorService;
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

    public AuthorsListControl(IServiceProvider services)
    {
        _services = services;
        _authorService = services.GetRequiredService<IAuthorService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(AuthorDto.Id), FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = nameof(AuthorDto.LastName), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = nameof(AuthorDto.FirstName), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = nameof(AuthorDto.MiddleName), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Дата рождения", DataPropertyName = nameof(AuthorDto.BirthDate), DefaultCellStyle = { Format = "dd.MM.yyyy" }, FillWeight = 60 }
        );

        _toolbar.AddClicked += async (_, _) => await ShowEditAsync(new AuthorDto());
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
        try
        {
            var data = await _authorService.GetAllAsync(_toolbar.SearchBox.Text);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex) { Ui.ShowError(FindForm(), ex.Message); }
    }

    private AuthorDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as AuthorDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите автора."); return; }
        var dto = await _authorService.GetByIdAsync(current.Id) ?? new AuthorDto();
        await ShowEditAsync(dto);
    }

    private async Task ShowEditAsync(AuthorDto dto)
    {
        var editControl = new AuthorEditControl(_services) { Dock = DockStyle.Fill };
        editControl.LoadAuthor(dto);
        var contentPanel = GetContentPanel();
        editControl.Saved += async (_, _) => { NavigateBack(contentPanel); };
        editControl.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(editControl);
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите автора."); return; }
        if (!Ui.Confirm(FindForm(), $"Удалить автора «{current.FullName}»?")) return;
        var result = await _authorService.DeleteAsync(current.Id);
        if (Ui.ReportResult(FindForm(), result, "Автор удалён.")) await ReloadAsync();
    }

    private ContentPanel? GetContentPanel() => FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault()
        ?? FindForm()?.Controls.OfType<Control>().SelectMany(c => c.Controls.OfType<ContentPanel>()).FirstOrDefault();

    private void NavigateBack(ContentPanel? contentPanel)
    {
        if (contentPanel == null) return;
        var listControl = new AuthorsListControl(_services) { Dock = DockStyle.Fill };
        _ = listControl.LoadDataAsync();
        contentPanel.ShowContent(listControl);
    }
}
