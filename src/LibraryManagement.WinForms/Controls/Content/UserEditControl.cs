using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls.Content;

public class UserEditControl : UserControl
{
    private readonly IUserService _userService;

    private readonly TextBox _txtUsername = new() { Width = 280 };
    private readonly TextBox _txtFullName = new() { Width = 280 };
    private readonly ComboBox _cmbRole = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly CheckBox _chkActive = new() { Text = "Активен", Checked = true, AutoSize = true };
    private readonly TextBox _txtPassword = new() { Width = 280, UseSystemPasswordChar = true };
    private readonly Label _lblPasswordHint = new() { AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8.25F, FontStyle.Italic) };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30 };

    private UserDto _user = new();

    public event EventHandler? Saved;
    public event EventHandler? Cancelled;

    public UserEditControl(IUserService userService)
    {
        _userService = userService;

        Dock = DockStyle.Fill;

        _cmbRole.Items.Add(new RoleItem(UserRole.Librarian, "Библиотекарь"));
        _cmbRole.Items.Add(new RoleItem(UserRole.Administrator, "Администратор"));
        _cmbRole.Items.Add(new RoleItem(UserRole.Director, "Директор"));
        _cmbRole.SelectedIndex = 0;

        BuildLayout();
        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnCancel.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

        ThemeManager.ApplyDarkTheme(this);
    }

    public void LoadUser(UserDto? dto)
    {
        _user = dto ?? new UserDto();
        _txtUsername.Text = _user.Username;
        _txtFullName.Text = _user.FullName;
        _chkActive.Checked = _user.IsActive;
        for (int i = 0; i < _cmbRole.Items.Count; i++)
        {
            if (_cmbRole.Items[i] is RoleItem r && r.Role == _user.Role) { _cmbRole.SelectedIndex = i; break; }
        }
        _txtPassword.Text = string.Empty;
        _lblPasswordHint.Text = _user.Id == 0
            ? "Минимум 6 символов."
            : "Оставьте пустым, чтобы не менять пароль.";
    }

    public void LoadEmpty()
    {
        _user = new UserDto();
        _txtUsername.Text = string.Empty;
        _txtFullName.Text = string.Empty;
        _cmbRole.SelectedIndex = 0;
        _chkActive.Checked = true;
        _txtPassword.Text = string.Empty;
        _lblPasswordHint.Text = "Минимум 6 символов.";
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(15),
            AutoSize = true,
            Height = 280
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Логин:"), 0, 0); layout.Controls.Add(_txtUsername, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("ФИО:"), 0, 1); layout.Controls.Add(_txtFullName, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Роль:"), 0, 2); layout.Controls.Add(_cmbRole, 1, 2);
        layout.Controls.Add(Ui.MakeLabel(""), 0, 3); layout.Controls.Add(_chkActive, 1, 3);
        layout.Controls.Add(Ui.MakeLabel("Пароль:"), 0, 4); layout.Controls.Add(_txtPassword, 1, 4);
        layout.Controls.Add(Ui.MakeLabel(""), 0, 5); layout.Controls.Add(_lblPasswordHint, 1, 5);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(250, 10);
        _btnCancel.Location = new Point(360, 10);
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
            _user.Username = _txtUsername.Text;
            _user.FullName = _txtFullName.Text;
            _user.Role = (_cmbRole.SelectedItem as RoleItem)?.Role ?? UserRole.Librarian;
            _user.IsActive = _chkActive.Checked;
            _user.Password = string.IsNullOrWhiteSpace(_txtPassword.Text) ? null : _txtPassword.Text;

            var result = await _userService.SaveAsync(_user);
            if (Ui.ReportResult(this, result))
            {
                Saved?.Invoke(this, EventArgs.Empty);
            }
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }

    private record RoleItem(UserRole Role, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}
