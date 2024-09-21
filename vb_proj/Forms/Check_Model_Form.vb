Imports System.IO

Public Class Check_Model_Form
    Inherits ESMT_Form

    Private ReadOnly Report_Format_Selector As New ComboBox
    Private ReadOnly Show_Only_Not_Compliant_ChckBx As New CheckBox
    Private ReadOnly Add_Rules_Description_ChckBx As New CheckBox
    Private Report_Directory_TxtBx As New TextBox
    Private WithEvents Report_Directory_Button As New Button
    Private WithEvents Check_Button As New Button

    Public Sub New(
        formats As String(),
        default_report_file_format As String,
        default_show_only_not_compliant As Boolean,
        default_add_rule_description As Boolean,
        default_report_directory As String)

        Dim item_y_pos As Integer = Marge
        Dim inner_item_y_pos As Integer = Marge

        '------------------------------------------------------------------------------------------'
        ' Add options panel
        inner_item_y_pos = Marge
        Dim option_label As New Label With {
            .Text = "Options",
            .Location = New Point(Marge, inner_item_y_pos),
            .Size = Label_Size}
        inner_item_y_pos += option_label.Height

        With Me.Report_Format_Selector
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            ' Order is coupled with items order in E_Report_File_Format
            .Items.AddRange(formats)
            .Text = default_report_file_format
            .Location = New Point(Marge, inner_item_y_pos)
            .Size = ESMT_Form.Label_Size
            .DropDownStyle = ComboBoxStyle.DropDownList
            .BackColor = Color.FromName("Control")
        End With
        inner_item_y_pos += Me.Report_Format_Selector.Height + Marge

        With Me.Show_Only_Not_Compliant_ChckBx
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            .Location = New Point(Marge, inner_item_y_pos)
            .Size = Path_Text_Size
            .Text = "Show only not compliant checkings"
            .Checked = default_show_only_not_compliant
        End With
        inner_item_y_pos += Me.Show_Only_Not_Compliant_ChckBx.Height + Marge

        With Me.Add_Rules_Description_ChckBx
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            .Location = New Point(Marge, inner_item_y_pos)
            .Size = Path_Text_Size
            .Text = "Add rules description in report"
            .Checked = default_add_rule_description
        End With
        inner_item_y_pos += Me.Add_Rules_Description_ChckBx.Height + Marge

        Dim option_panel As New Panel With {
            .Location = New Point(Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle,
            .Size = New Size(Panel_Width, inner_item_y_pos)}
        With option_panel.Controls
            .Add(option_label)
            .Add(Me.Report_Format_Selector)
            .Add(Me.Show_Only_Not_Compliant_ChckBx)
            .Add(Me.Add_Rules_Description_ChckBx)
        End With
        Me.Controls.Add(option_panel)
        item_y_pos += option_panel.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add report directory panel
        inner_item_y_pos = Marge

        Dim report_directory_label As New Label With {
            .Text = "Report directory selection",
            .Location = New Point(Marge, inner_item_y_pos),
            .Size = Label_Size}
        inner_item_y_pos += report_directory_label.Height

        With Me.Report_Directory_TxtBx
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            .Location = New Point(Marge, inner_item_y_pos)
            .Size = Path_Text_Size
            .Text = default_report_directory
        End With

        With Me.Report_Directory_Button
            .BackColor = Foreground_Color
            .ForeColor = Background_Color
            .Location = New Point(Path_Button_X_Pos, inner_item_y_pos)
            .Size = New Size(Path_Button_Width, 2 * Marge)
            .Text = "..."
        End With
        inner_item_y_pos += Me.Report_Directory_TxtBx.Height + Marge

        Dim report_directory_panel As New Panel With {
            .Location = New Point(Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle,
            .Size = New Size(Panel_Width, inner_item_y_pos)}
        With report_directory_panel.Controls
            .Add(report_directory_label)
            .Add(Me.Report_Directory_TxtBx)
            .Add(Me.Report_Directory_Button)
        End With
        Me.Controls.Add(report_directory_panel)
        item_y_pos += report_directory_panel.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design OK button
        With Me.Check_Button
            .BackColor = Foreground_Color
            .ForeColor = Background_Color
            .Text = "Check"
            .Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        End With
        Me.Controls.Add(Me.Check_Button)
        item_y_pos += Me.Check_Button.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Consistency check"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.BackColor = Background_Color
        Me.ForeColor = Foreground_Color

    End Sub

    Public Function Get_Report_Format() As String
        Return Me.Report_Format_Selector.Text
    End Function

    Public Function Are_Rules_Description_Added() As Boolean
        Return Me.Add_Rules_Description_ChckBx.Checked
    End Function

    Public Function Are_Only_Not_Compliant_Showed() As Boolean
        Return Me.Show_Only_Not_Compliant_ChckBx.Checked
    End Function

    Public Function Get_Report_Directory() As String
        Return Me.Report_Directory_TxtBx.Text
    End Function

    Private Sub Check() Handles Check_Button.Click
        If Directory.Exists(Me.Report_Directory_TxtBx.Text) Then
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        Else
            MsgBox("The directory to save the report shall exist !",
                MsgBoxStyle.Exclamation,
                "Wrong directory")
        End If
    End Sub

    Private Sub Report_Directory_Button_Clicked() Handles Report_Directory_Button.Click
        ESMT_Form.Select_Directory(
            "Select consistency check report directory",
            Me.Report_Directory_TxtBx)
    End Sub

End Class
