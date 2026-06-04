using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class PublishersListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IPublisherService _publisherService;
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

    public PublishersListControl(IServiceProvider services)
    {
        _services = services;
        _publisherService = services.GetRequiredService<IPublisherService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(PublisherDto.Id), FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(PublisherDto.Name), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Город", DataPropertyName = nameof(PublisherDto.City), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = nameof(PublisherDto.Description), FillWeight = 120 }
        );

        _toolbar.AddClicked += async (_, _) => await ShowEditAsync(new PublisherDto());
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
        try { _grid.DataSource = (await _publisherService.GetAllAsync(_toolbar.SearchBox.Text)).ToList(); }
        catch (Exception ex) { Ui.ShowError(FindForm(), ex.Message); }
    }

    private PublisherDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as PublisherDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите издательство."); return; }
        var dto = await _publisherService.GetByIdAsync(current.Id) ?? new PublisherDto();
        await ShowEditAsync(dto);
    }

    private async Task ShowEditAsync(PublisherDto dto)
    {
        var editControl = new PublisherEditControl(_services) { Dock = DockStyle.Fill };
        editControl.LoadPublisher(dto);
        var contentPanel = FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault();
        editControl.Saved += async (_, _) => { NavigateBack(contentPanel); };
        editControl.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(editControl);
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(FindForm(), "Выберите издательство."); return; }
        if (!Ui.Confirm(FindForm(), $"Удалить издательство «{current.Name}»?")) return;
        var result = await _publisherService.DeleteAsync(current.Id);
        if (Ui.ReportResult(FindForm(), result, "Издательство удалено.")) await ReloadAsync();
    }

    private void NavigateBack(ContentPanel? contentPanel)
    {
        if (contentPanel == null) return;
        var list = new PublishersListControl(_services) { Dock = DockStyle.Fill };
        _ = list.LoadDataAsync();
        contentPanel.ShowContent(list);
    }
}
