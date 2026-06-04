using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls;

public class SidePanel : UserControl
{
    public event EventHandler<string>? ActionSelected;

    private readonly Label _header;
    private readonly FlowLayoutPanel _buttonsPanel;

    public SidePanel()
    {
        Dock = DockStyle.Left;
        Width = 200;
        BackColor = ThemeManager.SidePanelBackground;

        _header = new Label
        {
            Text = string.Empty,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Font = ThemeManager.HeaderFont,
            BackColor = ThemeManager.TopMenuBackground,
            ForeColor = ThemeManager.TextHeader
        };

        _buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Width = 200,
            Padding = new Padding(5, 10, 5, 0),
            BackColor = Color.Transparent
        };

        Controls.Add(_buttonsPanel);
        Controls.Add(_header);
    }

    public void SetSectionTitle(string title) => _header.Text = title;

    public void SetActions(List<(string Key, string Icon, string Text)> actions)
    {
        _buttonsPanel.Controls.Clear();

        foreach (var action in actions)
        {
            var btn = new Button
            {
                FlatStyle = FlatStyle.Flat,
                FlatAppearance =
                {
                    BorderSize = 0,
                    MouseOverBackColor = ThemeManager.SideButtonHover,
                    MouseDownBackColor = ThemeManager.SideButtonSelected
                },
                BackColor = ThemeManager.SideButtonNormal,
                ForeColor = ThemeManager.TextPrimary,
                Width = 190,
                Height = 50,
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = action.Key
            };

            var iconLabel = new Label
            {
                Text = action.Icon,
                Font = ThemeManager.IconFont,
                ForeColor = ThemeManager.Accent,
                BackColor = Color.Transparent,
                Width = 35,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = action.Key
            };

            var textLabel = new Label
            {
                Text = action.Text,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = ThemeManager.TextPrimary,
                BackColor = Color.Transparent,
                Width = 145,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = action.Key
            };

            btn.Controls.Add(textLabel);
            btn.Controls.Add(iconLabel);

            btn.Click += OnButtonClick;
            iconLabel.Click += OnButtonClick;
            textLabel.Click += OnButtonClick;
            btn.MouseEnter += OnButtonMouseEnter;
            btn.MouseLeave += OnButtonMouseLeave;

            _buttonsPanel.Controls.Add(btn);
        }
    }

    private void OnButtonClick(object? sender, EventArgs e)
    {
        var control = sender as Control;
        var key = control?.Tag as string;
        if (key != null)
            ActionSelected?.Invoke(this, key);
    }

    private void OnButtonMouseEnter(object? sender, EventArgs e)
    {
        if (sender is Button btn)
            btn.BackColor = ThemeManager.SideButtonHover;
    }

    private void OnButtonMouseLeave(object? sender, EventArgs e)
    {
        if (sender is Button btn)
            btn.BackColor = ThemeManager.SideButtonNormal;
    }
}
