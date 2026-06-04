using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;
using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls.Content;

public class UsersListControl : UserControl
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    private readonly CrudToolbar _toolbar = new();
    private readonly Font _boldFont = new("Segoe UI", 9F, FontStyle.Bold);
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
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
        BackgroundColor = Color.White
    };

    public event EventHandler<UserDto>? EditRequested;
    public event EventHandler? RefreshRequested;

    public UsersListControl(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;

        Dock = DockStyle.Fill;

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(UserDto.Id), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Логин", DataPropertyName = nameof(UserDto.Username) },
            new DataGridViewTextBoxColumn { HeaderText = "ФИО", DataPropertyName = nameof(UserDto.FullName) },
            new DataGridViewTextBoxColumn { HeaderText = "Роль", DataPropertyName = nameof(UserDto.Role) },
            new DataGridViewCheckBoxColumn { HeaderText = "Активен", DataPropertyName = nameof(UserDto.IsActive) },
            new DataGridViewTextBoxColumn { HeaderText = "Последний вход", DataPropertyName = nameof(UserDto.LastLoginAt), DefaultCellStyle = { Format = "dd.MM.yyyy HH:mm" } }
        );

        _toolbar.SearchBox.Visible = false;
        _toolbar.AddClicked += async (_, _) => await OnAddAsync();
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();

        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);

        ThemeManager.ApplyDarkTheme(this);
    }

    public async Task LoadDataAsync()
    {
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _userService.GetAllAsync();
            _grid.DataSource = data.ToList();

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is UserDto u)
                {
                    if (!u.IsActive) row.DefaultCellStyle.ForeColor = Color.Gray;
                    if (u.Role == UserRole.Administrator || u.Role == UserRole.Director)
                        row.DefaultCellStyle.Font = _boldFont;
                }
            }
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить пользователей: " + ex.Message);
        }
    }

    private UserDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as UserDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите пользователя в списке."); return; }
        await OnEditAsync(current.Id);
    }

    private async Task OnAddAsync()
    {
        EditRequested?.Invoke(this, new UserDto());
    }

    private async Task OnEditAsync(int id)
    {
        var dto = await _userService.GetByIdAsync(id) ?? new UserDto();
        EditRequested?.Invoke(this, dto);
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите пользователя в списке."); return; }

        if (current.Id == _currentUser.UserId)
        {
            Ui.ShowError(this, "Нельзя удалить собственную учётную запись.");
            return;
        }

        if (!Ui.Confirm(this, $"Удалить пользователя «{current.Username}»?")) return;

        var result = await _userService.DeleteAsync(current.Id);
        Ui.ReportResult(this, result, "Пользователь удалён.");
        await ReloadAsync();
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }
}
