using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class ReadersListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IReaderService _readerService;
    private readonly CrudToolbar _toolbar = new();
    private readonly CheckBox _chkIncludeBlocked = new() { Text = "Включая заблокированных", AutoSize = true, Checked = true };
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

    public ReadersListControl(IServiceProvider services)
    {
        _services = services;
        _readerService = services.GetRequiredService<IReaderService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(ReaderDto.Id), FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "№ билета", DataPropertyName = nameof(ReaderDto.CardNumber), FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = nameof(ReaderDto.LastName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = nameof(ReaderDto.FirstName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = nameof(ReaderDto.MiddleName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Телефон", DataPropertyName = nameof(ReaderDto.Phone), FillWeight = 50 },
            new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = nameof(ReaderDto.Email), FillWeight = 60 },
            new DataGridViewCheckBoxColumn { HeaderText = "Заблокирован", DataPropertyName = nameof(ReaderDto.IsBlocked), FillWeight = 35 }
        );

        _chkIncludeBlocked.CheckedChanged += async (_, _) => await ReloadAsync();
        _toolbar.ExtraButtonsPanel.Controls.Add(_chkIncludeBlocked);
        _toolbar.AddClicked += async (_, _) => await ShowEditAsync(new ReaderDto { RegistrationDate = DateTime.UtcNow });
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
            var data = await _readerService.GetAllAsync(_toolbar.SearchBox.Text, _chkIncludeBlocked.Checked);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex) { Ui.ShowError(FindForm(), ex.Message); }
    }

    private ReaderDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as ReaderDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите читателя."); return; }
        var dto = await _readerService.GetByIdAsync(current.Id) ?? new ReaderDto();
        await ShowEditAsync(dto);
    }

    private async Task ShowEditAsync(ReaderDto dto)
    {
        var editControl = new ReaderEditControl(_services) { Dock = DockStyle.Fill };
        editControl.LoadReader(dto);
        var contentPanel = FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault();
        editControl.Saved += async (_, _) => { NavigateBack(contentPanel); };
        editControl.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(editControl);
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите читателя."); return; }
        if (!Ui.Confirm(FindForm(), $"Удалить читателя «{current.FullName}»?")) return;
        var result = await _readerService.DeleteAsync(current.Id);
        if (Ui.ReportResult(FindForm(), result, "Читатель удалён.")) await ReloadAsync();
    }

    private void NavigateBack(ContentPanel? contentPanel)
    {
        if (contentPanel == null) return;
        var list = new ReadersListControl(_services) { Dock = DockStyle.Fill };
        _ = list.LoadDataAsync();
        contentPanel.ShowContent(list);
    }
}
