using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls;

public class TopMenuBar : Panel
{
    public event EventHandler<string>? MenuItemSelected;

    private readonly List<(string Key, Panel Container, Label Icon)> _items = new();

    public TopMenuBar()
    {
        Dock = DockStyle.Top;
        Height = 50;
        BackColor = ThemeManager.TopMenuBackground;
    }

    public void Populate(List<(string Key, string Text, string Icon)> items)
    {
        Controls.Clear();
        _items.Clear();

        int x = 5;
        foreach (var item in items)
        {
            var container = new Panel
            {
                Width = 115,
                Height = 44,
                Location = new Point(x, 3),
                BackColor = ThemeManager.TopMenuBackground,
                Tag = item.Key
            };

            var iconLabel = new Label
            {
                Text = item.Icon,
                Font = ThemeManager.IconFont,
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Left,
                Width = 35,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var textLabel = new Label
            {
                Text = item.Text,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 5, 0)
            };

            container.Controls.Add(textLabel);
            container.Controls.Add(iconLabel);
            container.Click += OnItemClick;
            iconLabel.Click += OnItemClick;
            textLabel.Click += OnItemClick;

            _items.Add((item.Key, container, iconLabel));
            Controls.Add(container);
            x += 120;
        }
    }

    public void SetSelected(string key)
    {
        foreach (var (k, container, icon) in _items)
        {
            if (k == key)
            {
                container.BackColor = ThemeManager.SideButtonSelected;
                icon.ForeColor = ThemeManager.TextHeader;
                foreach (Control c in container.Controls)
                    if (c is Label lbl) lbl.ForeColor = ThemeManager.TextHeader;
            }
            else
            {
                container.BackColor = ThemeManager.TopMenuBackground;
                icon.ForeColor = ThemeManager.TextPrimary;
                foreach (Control c in container.Controls)
                    if (c is Label lbl) lbl.ForeColor = ThemeManager.TextPrimary;
            }
        }
    }

    private void OnItemClick(object? sender, EventArgs e)
    {
        var control = sender as Control;
        var key = control?.Tag as string ?? control?.Parent?.Tag as string;
        if (key != null)
        {
            SetSelected(key);
            MenuItemSelected?.Invoke(this, key);
        }
    }
}
