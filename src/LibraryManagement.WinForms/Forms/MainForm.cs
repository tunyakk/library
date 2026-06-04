using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Controls.Content;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Theme;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace LibraryManagement.WinForms.Forms;

public partial class MainForm : Form
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    private TopMenuBar _topMenu = null!;
    private SidePanel _sidePanel = null!;
    private ContentPanel _contentPanel = null!;
    private Panel _footerBar = null!;

    public MainForm(IConfiguration configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
        InitializeComponent();
        BuildLayout();
        ApplyTheme();
        Shown += OnFirstShown;
    }

    private void BuildLayout()
    {
        _footerBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = ThemeManager.FooterBackground
        };

        var lblStudent = new Label
        {
            Text = "Студент: Кузнецова Надежда Сергеевна | Группа: ИСс-32",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 15, 0),
            ForeColor = ThemeManager.TextPrimary,
            Font = new Font("Segoe UI", 9F)
        };
        _footerBar.Controls.Add(lblStudent);

        var lblUser = new Label
        {
            Text = "  Библиотека",
            Dock = DockStyle.Left,
            Width = 250,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ThemeManager.Accent,
            Font = new Font("Segoe UI", 9F),
            Padding = new Padding(10, 0, 0, 0)
        };
        _footerBar.Controls.Add(lblUser);

        _topMenu = new TopMenuBar();
        _topMenu.MenuItemSelected += OnTopMenuClick;
        _topMenu.Populate(new List<(string, string, string)>
        {
            ("library", "Библиотека", "\uE80F"),
            ("references", "Справочники", "\uE8A1"),
            ("books", "Книги", "\uE82F"),
            ("operations", "Операции", "\uE81D"),
            ("reports", "Отчёты", "\uE9D9")
        });

        _sidePanel = new SidePanel();
        _sidePanel.ActionSelected += OnSidebarActionClick;

        _contentPanel = new ContentPanel();

        Controls.Add(_contentPanel);
        Controls.Add(_sidePanel);
        Controls.Add(_topMenu);
        Controls.Add(_footerBar);

        _contentPanel.BringToFront();
    }

    private void ApplyTheme()
    {
        ThemeManager.ApplyDarkTheme(this);
        ThemeManager.ApplyDarkTheme(_footerBar);
    }

    private void OnFirstShown(object? sender, EventArgs e)
    {
        Shown -= OnFirstShown;
        _topMenu.SetSelected("library");
        OnTopMenuClick(this, "library");
    }

    private void OnTopMenuClick(object? sender, string sectionKey)
    {
        var actions = GetActionsForSection(sectionKey);
        var title = GetSectionTitle(sectionKey);
        _sidePanel.SetSectionTitle(title);
        _sidePanel.SetActions(actions);

        if (actions.Count > 0)
            OnSidebarActionClick(this, actions[0].Key);
    }

    private void OnSidebarActionClick(object? sender, string actionKey)
    {
        var control = CreateContentControl(actionKey);
        if (control != null)
            _contentPanel.ShowContent(control);
    }

    private Control CreateContentControl(string actionKey)
    {
        Control control = actionKey switch
        {
            "dashboard" => CreateDashboard(),
            "authors_list" => new AuthorsListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "genres_list" => new GenresListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "publishers_list" => new PublishersListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "readers_list" => new ReadersListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "books_list" => new BooksListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "book_search" => new BooksListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "issue_loan" => new IssueLoanControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "return_loan" => new ReturnLoanControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "loans_list" => new LoansListControl(_services)
            {
                Dock = DockStyle.Fill
            },
            "reports" => new ReportsControl(_services)
            {
                Dock = DockStyle.Fill
            },
            _ => CreateDashboard()
        };

        switch (control)
        {
            case AuthorsListControl c: _ = c.LoadDataAsync(); break;
            case GenresListControl c: _ = c.LoadDataAsync(); break;
            case PublishersListControl c: _ = c.LoadDataAsync(); break;
            case ReadersListControl c: _ = c.LoadDataAsync(); break;
            case BooksListControl c: _ = c.LoadDataAsync(); break;
            case LoansListControl c: _ = c.LoadDataAsync(); break;
            case IssueLoanControl c: _ = c.LoadDataAsync(); break;
        }

        return control;
    }

    private Control CreateDashboard()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.FormBackground };

        var bgImage = LoadBackgroundImage();

        var pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = ThemeManager.FormBackground
        };
        if (bgImage != null) pictureBox.Image = bgImage;

        var overlay = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(120, 26, 26, 46)
        };

        var titleLabel = new Label
        {
            Text = "Система управления библиотекой",
            Font = new Font("Segoe UI Semibold", 28F),
            ForeColor = ThemeManager.TextHeader,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var subtitleLabel = new Label
        {
            Text = "Библиотечный фонд — учёт, выдача, отчёты",
            Font = new Font("Segoe UI", 14F),
            ForeColor = ThemeManager.Accent,
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var studentLabel = new Label
        {
            Text = "Разработчик: Кузнецова Надежда Сергеевна, группа ИСс-32",
            Font = new Font("Segoe UI", 10F),
            ForeColor = ThemeManager.TextPrimary,
            BackColor = Color.Transparent,
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter
        };

        overlay.Controls.Add(titleLabel);
        overlay.Controls.Add(subtitleLabel);
        overlay.Controls.Add(studentLabel);

        panel.Controls.Add(overlay);
        panel.Controls.Add(pictureBox);

        pictureBox.SendToBack();

        return panel;
    }

    private Image? LoadBackgroundImage()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "library_bg.jpg");
        if (File.Exists(path))
        {
            try { return Image.FromFile(path); }
            catch { }
        }
        return null;
    }

    private static List<(string Key, string Icon, string Text)> GetActionsForSection(string section)
    {
        return section switch
        {
            "library" => new()
            {
                ("dashboard", "\uE80F", "Главный экран")
            },
            "references" => new()
            {
                ("authors_list", "\uE77B", "Авторы"),
                ("genres_list", "\uE8AB", "Жанры"),
                ("publishers_list", "\uE787", "Издательства"),
                ("readers_list", "\uE77B", "Читатели")
            },
            "books" => new()
            {
                ("books_list", "\uE82F", "Каталог книг"),
                ("book_search", "\uE721", "Поиск книг")
            },
            "operations" => new()
            {
                ("issue_loan", "\uE7B3", "Выдача книг"),
                ("loans_list", "\uE81D", "Список выдач")
            },
            "reports" => new()
            {
                ("reports", "\uE9D9", "Аналитика и отчёты")
            },
            _ => new()
        };
    }

    private static string GetSectionTitle(string section)
    {
        return section switch
        {
            "library" => "Библиотека",
            "references" => "Справочники",
            "books" => "Книги",
            "operations" => "Операции",
            "reports" => "Отчёты",
            _ => ""
        };
    }
}
