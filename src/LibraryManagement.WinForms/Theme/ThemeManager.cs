namespace LibraryManagement.WinForms.Theme;

public static class ThemeManager
{
    public static readonly Color FormBackground = Color.FromArgb(26, 26, 46);
    public static readonly Color TopMenuBackground = Color.FromArgb(22, 33, 62);
    public static readonly Color SidePanelBackground = Color.FromArgb(26, 26, 46);
    public static readonly Color SideButtonNormal = Color.FromArgb(15, 52, 96);
    public static readonly Color SideButtonHover = Color.FromArgb(20, 60, 110);
    public static readonly Color SideButtonSelected = Color.FromArgb(83, 52, 131);
    public static readonly Color TextPrimary = Color.FromArgb(224, 224, 224);
    public static readonly Color TextHeader = Color.White;
    public static readonly Color Accent = Color.FromArgb(79, 195, 247);
    public static readonly Color GridBackground = Color.FromArgb(30, 30, 50);
    public static readonly Color GridHeaderBackground = Color.FromArgb(40, 40, 65);
    public static readonly Color GridRowAlternate = Color.FromArgb(35, 35, 55);
    public static readonly Color TextBoxBackground = Color.FromArgb(40, 40, 65);
    public static readonly Color ControlBorder = Color.FromArgb(60, 60, 90);
    public static readonly Color FooterBackground = Color.FromArgb(22, 33, 62);

    public static readonly Font DefaultFont = new("Segoe UI", 9F);
    public static readonly Font HeaderFont = new("Segoe UI Semibold", 11F);
    public static readonly Font IconFont = new("Segoe MDL2 Assets", 16F);

    public static void ApplyDarkTheme(Form form)
    {
        form.BackColor = FormBackground;
        form.ForeColor = TextPrimary;
        form.Font = DefaultFont;
        ApplyDarkTheme((Control)form);
    }

    public static void ApplyDarkTheme(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            switch (control)
            {
                case Button btn:
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = SideButtonNormal;
                    btn.ForeColor = TextPrimary;
                    btn.FlatAppearance.BorderColor = ControlBorder;
                    btn.FlatAppearance.MouseOverBackColor = SideButtonHover;
                    btn.Font = DefaultFont;
                    break;

                case TextBox tb:
                    tb.BackColor = TextBoxBackground;
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.Font = DefaultFont;
                    break;

                case DataGridView dgv:
                    dgv.BackgroundColor = GridBackground;
                    dgv.DefaultCellStyle.BackColor = GridBackground;
                    dgv.DefaultCellStyle.ForeColor = TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = SideButtonSelected;
                    dgv.DefaultCellStyle.SelectionForeColor = TextHeader;
                    dgv.DefaultCellStyle.Font = DefaultFont;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackground;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextHeader;
                    dgv.ColumnHeadersDefaultCellStyle.Font = DefaultFont;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.RowHeadersVisible = false;
                    dgv.GridColor = ControlBorder;
                    dgv.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case TabPage tp:
                    tp.BackColor = FormBackground;
                    tp.ForeColor = TextPrimary;
                    break;

                case Panel panel:
                    panel.BackColor = SidePanelBackground;
                    break;

                case UserControl uc:
                    uc.BackColor = FormBackground;
                    uc.ForeColor = TextPrimary;
                    break;

                case Label lbl:
                    lbl.ForeColor = TextPrimary;
                    lbl.BackColor = Color.Transparent;
                    break;

                case TreeView tv:
                    tv.BackColor = SidePanelBackground;
                    tv.ForeColor = TextPrimary;
                    tv.BorderStyle = BorderStyle.None;
                    tv.Font = DefaultFont;
                    break;

                case TabControl tc:
                    tc.BackColor = FormBackground;
                    tc.ForeColor = TextPrimary;
                    tc.Font = DefaultFont;
                    break;

                case PictureBox pb:
                    pb.BackColor = Color.Transparent;
                    break;

                case CheckBox cb:
                    cb.ForeColor = TextPrimary;
                    cb.BackColor = Color.Transparent;
                    break;

                case ComboBox cb:
                    cb.BackColor = TextBoxBackground;
                    cb.ForeColor = TextPrimary;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;

                case GroupBox gb:
                    gb.ForeColor = TextPrimary;
                    gb.BackColor = Color.Transparent;
                    break;
            }

            if (control.HasChildren)
            {
                ApplyDarkTheme(control);
            }
        }
    }
}
