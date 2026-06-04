using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class LoansListControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly ILoanService _loanService;
    private readonly ICurrentUserService _currentUser;

    private readonly CrudToolbar _toolbar = new();
    private readonly CheckBox _chkOnlyActive = new() { Text = "Только активные", AutoSize = true };
    private readonly CheckBox _chkOnlyOverdue = new() { Text = "Только просроченные", AutoSize = true };
    private readonly Button _btnIssue = new() { Text = "Выдать книгу", Width = 130, Height = 28 };
    private readonly Button _btnReturn = new() { Text = "Принять возврат", Width = 140, Height = 28 };

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

    public LoansListControl(IServiceProvider services)
    {
        _services = services;
        _loanService = services.GetRequiredService<ILoanService>();
        _currentUser = services.GetRequiredService<ICurrentUserService>();

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(LoanDto.Id), FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = nameof(LoanDto.BookTitle), FillWeight = 80 },
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = nameof(LoanDto.ReaderFullName), FillWeight = 70 },
            new DataGridViewTextBoxColumn { HeaderText = "Билет", DataPropertyName = nameof(LoanDto.ReaderCardNumber), FillWeight = 35 },
            new DataGridViewTextBoxColumn { HeaderText = "Выдана", DataPropertyName = nameof(LoanDto.LoanDate), DefaultCellStyle = { Format = "dd.MM.yyyy" }, FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Срок", DataPropertyName = nameof(LoanDto.DueDate), DefaultCellStyle = { Format = "dd.MM.yyyy" }, FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Возвращена", DataPropertyName = nameof(LoanDto.ReturnedAt), DefaultCellStyle = { Format = "dd.MM.yyyy" }, FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = nameof(LoanDto.StatusDisplay), FillWeight = 35 },
            new DataGridViewTextBoxColumn { HeaderText = "Штраф", DataPropertyName = nameof(LoanDto.FineAmount), DefaultCellStyle = { Format = "0.00" }, FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Кто выдал", DataPropertyName = nameof(LoanDto.IssuedByUserName), FillWeight = 50 }
        );

        _btnIssue.Click += async (_, _) => await ShowIssueAsync();
        _btnReturn.Click += async (_, _) => await ShowReturnForSelectedAsync();
        _chkOnlyActive.CheckedChanged += async (_, _) => { if (_chkOnlyActive.Checked) _chkOnlyOverdue.Checked = false; await ReloadAsync(); };
        _chkOnlyOverdue.CheckedChanged += async (_, _) => { if (_chkOnlyOverdue.Checked) _chkOnlyActive.Checked = false; await ReloadAsync(); };
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowReturnForSelectedAsync(); };

        _toolbar.AddButton.Visible = false;
        _toolbar.EditButton.Visible = false;
        _toolbar.DeleteButton.Visible = false;
        _toolbar.ExtraButtonsPanel.Controls.AddRange(new Control[] { _btnIssue, _btnReturn, _chkOnlyActive, _chkOnlyOverdue });

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        ThemeManager.ApplyDarkTheme(this);
    }

    public async Task LoadDataAsync()
    {
        await _loanService.RefreshOverdueStatusesAsync();
        _chkOnlyActive.Checked = true;
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _loanService.GetAllAsync(
                search: _toolbar.SearchBox.Text,
                onlyActive: _chkOnlyActive.Checked,
                onlyOverdue: _chkOnlyOverdue.Checked);
            _grid.DataSource = data.ToList();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is LoanDto l)
                {
                    if (l.Status == LoanStatus.Overdue) row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    else if (l.Status == LoanStatus.Returned) row.DefaultCellStyle.ForeColor = Color.Gray;
                }
            }
        }
        catch (Exception ex) { Ui.ShowError(this, ex.Message); }
    }

    private LoanDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as LoanDto;

    private async Task ShowIssueAsync()
    {
        var contentPanel = FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault();
        var control = new IssueLoanControl(_services) { Dock = DockStyle.Fill };
        control.LoanIssued += async (_, _) => { NavigateBack(contentPanel); };
        control.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(control);
        await control.LoadDataAsync();
    }

    private async Task ShowReturnForSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите выдачу."); return; }
        if (current.ReturnedAt != null) { Ui.ShowInfo(this, "Эта книга уже возвращена."); return; }
        var contentPanel = FindForm()?.Controls.OfType<ContentPanel>().FirstOrDefault();
        var control = new ReturnLoanControl(_services) { Dock = DockStyle.Fill };
        control.LoadLoan(current);
        control.LoanReturned += async (_, _) => { NavigateBack(contentPanel); };
        control.Cancelled += (_, _) => NavigateBack(contentPanel);
        contentPanel?.ShowContent(control);
    }

    private void NavigateBack(ContentPanel? contentPanel)
    {
        if (contentPanel == null) return;
        var list = new LoansListControl(_services) { Dock = DockStyle.Fill };
        _ = list.LoadDataAsync();
        contentPanel.ShowContent(list);
    }
}
