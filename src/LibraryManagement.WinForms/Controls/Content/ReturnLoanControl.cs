using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Controls.Content;

public class ReturnLoanControl : UserControl
{
    private readonly IServiceProvider _services;
    private readonly ILoanService _loanService;

    private readonly Label _lblInfo = new() { AutoSize = false, Width = 380, Height = 60, Padding = new Padding(0, 4, 0, 4) };
    private readonly NumericUpDown _numFine = new() { Width = 120, Minimum = 0, Maximum = 100000, DecimalPlaces = 2, Increment = 50m };
    private readonly TextBox _txtNotes = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Width = 380, Height = 70 };
    private readonly Button _btnSave = new() { Text = "Принять возврат", Width = 140, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30 };

    private LoanDto? _loan;

    public event EventHandler? LoanReturned;
    public event EventHandler? Cancelled;

    public ReturnLoanControl(IServiceProvider services)
    {
        _services = services;
        _loanService = services.GetRequiredService<ILoanService>();

        Dock = DockStyle.Fill;

        BuildLayout();
        _btnSave.Click += async (_, _) => await ReturnAsync();
        _btnCancel.Click += (_, _) => Cancelled?.Invoke(this, EventArgs.Empty);

        ThemeManager.ApplyDarkTheme(this);
    }

    public void LoadLoan(LoanDto loan)
    {
        _loan = loan;
        var overdueText = loan.DaysRemaining < 0 ? $" (просрочка {-loan.DaysRemaining} дн.)" : "";
        _lblInfo.Text = $"Книга: {loan.BookTitle}\n" +
                        $"Читатель: {loan.ReaderFullName} (билет {loan.ReaderCardNumber})\n" +
                        $"Срок: {loan.DueDate:dd.MM.yyyy}{overdueText}";
        if (loan.FineAmount.HasValue)
        {
            _numFine.Value = Math.Min(_numFine.Maximum, Math.Max(_numFine.Minimum, loan.FineAmount.Value));
        }
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(15),
            AutoSize = true,
            Height = 300
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Выдача:"), 0, 0); layout.Controls.Add(_lblInfo, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Штраф:"), 0, 1); layout.Controls.Add(_numFine, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Заметка:"), 0, 2); layout.Controls.Add(_txtNotes, 1, 2);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(220, 10);
        _btnCancel.Location = new Point(370, 10);
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(buttons);
        Controls.Add(layout);
    }

    private async Task ReturnAsync()
    {
        if (_loan is null)
        {
            Ui.ShowError(this, "Не выбрана выдача.");
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            var notes = string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text;
            var result = await _loanService.ReturnAsync(_loan.Id, _numFine.Value, notes);
            if (Ui.ReportResult(this, result, "Книга возвращена."))
            {
                LoanReturned?.Invoke(this, EventArgs.Empty);
            }
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
