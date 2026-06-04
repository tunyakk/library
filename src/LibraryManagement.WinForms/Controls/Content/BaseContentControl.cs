using LibraryManagement.WinForms.Theme;

namespace LibraryManagement.WinForms.Controls.Content;

public class BaseContentControl : UserControl
{
    public BaseContentControl()
    {
        BackColor = ThemeManager.FormBackground;
        ForeColor = ThemeManager.TextPrimary;
    }

    public virtual void OnActivate() { }

    public virtual void OnDeactivate() { }
}
