using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class BooksListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;

    private readonly CrudToolbar _toolbar = new();
    private readonly ComboBox _cmbAuthorFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly ComboBox _cmbGenreFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly CheckBox _chkAvailableOnly = new() { Text = "Только доступные", AutoSize = true };

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
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = ThemeManager.GridBackground
    };

    public BooksListControl(IServiceProvider services)
    {
        _services = services;
        _bookService = services.GetRequiredService<IBookService>();
        _authorService = services.GetRequiredService<IAuthorService>();
        _genreService = services.GetRequiredService<IGenreService>();
        _publisherService = services.GetRequiredService<IPublisherService>();

        Dock = DockStyle.Fill;

        BuildGridColumns();
        BuildExtraFilters();

        _toolbar.AddClicked += async (_, _) => await OnAddAsync();
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        ThemeManager.ApplyDarkTheme(this);
    }

    public async Task LoadDataAsync()
    {
        await LoadFiltersAsync();
        await ReloadAsync();
    }

    private void BuildGridColumns()
    {
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(BookDto.Id), FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(BookDto.Title), FillWeight = 120 },
            new DataGridViewTextBoxColumn { HeaderText = "Автор", DataPropertyName = nameof(BookDto.AuthorFullName), FillWeight = 90 },
            new DataGridViewTextBoxColumn { HeaderText = "Жанр", DataPropertyName = nameof(BookDto.GenreName), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Издательство", DataPropertyName = nameof(BookDto.PublisherName), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Дата издания", DataPropertyName = nameof(BookDto.PublicationDate), FillWeight = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "ISBN", DataPropertyName = nameof(BookDto.Isbn), FillWeight = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Доступно / всего", DataPropertyName = nameof(BookDto.AvailabilityText), FillWeight = 60 }
        );
    }

    private void BuildExtraFilters()
    {
        var lblAuthor = new Label { Text = "Автор:", AutoSize = true, Margin = new Padding(8, 9, 4, 0) };
        var lblGenre = new Label { Text = "Жанр:", AutoSize = true, Margin = new Padding(8, 9, 4, 0) };
        _cmbAuthorFilter.SelectedIndexChanged += async (_, _) => await ReloadAsync();
        _cmbGenreFilter.SelectedIndexChanged += async (_, _) => await ReloadAsync();
        _chkAvailableOnly.CheckedChanged += async (_, _) => await ReloadAsync();
        _toolbar.ExtraButtonsPanel.Controls.AddRange(new Control[] { lblAuthor, _cmbAuthorFilter, lblGenre, _cmbGenreFilter, _chkAvailableOnly });
    }

    private async Task LoadFiltersAsync()
    {
        var authors = await _authorService.GetAllAsync();
        var authorOptions = new List<AuthorDto> { new() { Id = 0, LastName = "— все —" } };
        authorOptions.AddRange(authors);
        _cmbAuthorFilter.DisplayMember = nameof(AuthorDto.FullName);
        _cmbAuthorFilter.ValueMember = nameof(AuthorDto.Id);
        _cmbAuthorFilter.DataSource = authorOptions;

        var genres = await _genreService.GetAllAsync();
        var genreOptions = new List<GenreDto> { new() { Id = 0, Name = "— все —" } };
        genreOptions.AddRange(genres);
        _cmbGenreFilter.DisplayMember = nameof(GenreDto.Name);
        _cmbGenreFilter.ValueMember = nameof(GenreDto.Id);
        _cmbGenreFilter.DataSource = genreOptions;
    }

    private async Task ReloadAsync()
    {
        try
        {
            int? authorId = _cmbAuthorFilter.SelectedValue is int a && a > 0 ? a : null;
            int? genreId = _cmbGenreFilter.SelectedValue is int g && g > 0 ? g : null;
            var data = await _bookService.GetAllAsync(_toolbar.SearchBox.Text, authorId, genreId, _chkAvailableOnly.Checked);
            _grid.DataSource = data.ToList();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is BookDto b && b.AvailableCopies == 0)
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }
        }
        catch (Exception ex) { Ui.ShowError(this, ex.Message); }
    }

    private BookDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as BookDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите книгу."); return; }
        var dto = await _bookService.GetByIdAsync(current.Id) ?? new BookDto();
        await ShowBookEdit(dto);
    }

    private async Task OnAddAsync()
    {
        var authors = await _authorService.GetAllAsync();
        if (authors.Count == 0)
        {
            Ui.ShowInfo(this, "Сначала добавьте хотя бы одного автора в разделе «Справочники → Авторы».");
            NavigateTo<AuthorsListControl>();
            return;
        }

        var genres = await _genreService.GetAllAsync();
        if (genres.Count == 0)
        {
            Ui.ShowInfo(this, "Сначала добавьте хотя бы один жанр в разделе «Справочники → Жанры».");
            NavigateTo<GenresListControl>();
            return;
        }

        await ShowBookEdit(new BookDto());
    }

    private async Task ShowBookEdit(BookDto dto)
    {
        var editControl = new BookEditControl(_services) { Dock = DockStyle.Fill };
        editControl.LoadBook(dto);
        var contentPanel = GetContentPanel();
        editControl.Saved += async (_, _) => { NavigateBack(contentPanel); };
        editControl.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(editControl);
        await editControl.InitAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите книгу."); return; }
        if (!Ui.Confirm(this, $"Удалить книгу «{current.Title}»?")) return;
        var result = await _bookService.DeleteAsync(current.Id);
        if (Ui.ReportResult(this, result, "Книга удалена.")) await ReloadAsync();
    }

    private ContentPanel? GetContentPanel() => FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault();

    private void NavigateBack(ContentPanel? contentPanel)
    {
        if (contentPanel == null) return;
        var list = new BooksListControl(_services) { Dock = DockStyle.Fill };
        _ = list.LoadDataAsync();
        contentPanel.ShowContent(list);
    }

    private void NavigateTo<T>() where T : Control
    {
        var panel = GetContentPanel();
        if (panel == null) return;
        var control = (T)Activator.CreateInstance(typeof(T), _services)!;
        control.Dock = DockStyle.Fill;
        panel.ShowContent(control);
        if (control is BooksListControl blc) _ = blc.LoadDataAsync();
        else if (control is AuthorsListControl alc) _ = alc.LoadDataAsync();
        else if (control is GenresListControl glc) _ = glc.LoadDataAsync();
        else if (control is PublishersListControl plc) _ = plc.LoadDataAsync();
        else if (control is ReadersListControl rlc) _ = rlc.LoadDataAsync();
    }
}
