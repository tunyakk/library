using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls;

public class ContentPanel : UserControl
{
    private Control? _currentContent;

    public ContentPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.FormBackground;
    }

    public void ShowContent(Control content)
    {
        if (_currentContent != null)
        {
            Controls.Remove(_currentContent);
            _currentContent.Dispose();
        }

        _currentContent = content;
        content.Dock = DockStyle.Fill;
        Controls.Add(content);
        content.BringToFront();
    }
}
