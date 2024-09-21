Imports System.Globalization

Public Class Fixed_Point_Type_Form
    Inherits Element_With_Ref_Form

    Private WithEvents Unit_TexBox As TextBox
    Private WithEvents Resolution_TexBox As TextBox
    Private WithEvents Offset_TexBox As TextBox

    Public Sub New(
            form_kind As E_Form_Kind,
            default_uuid As String,
            default_name As String,
            default_description As String,
            default_ref_element_path As String,
            ref_element_list As List(Of Software_Element),
            default_unit As String,
            default_resolution As String,
            default_offset As String)

        MyBase.New(
            form_kind,
            Fixed_Point_Type.Metaclass_Name,
            default_uuid,
            default_name,
            default_description,
            "Base integer type",
            default_ref_element_path,
            ref_element_list)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Add fixed point defintion panel
        Dim fixed_point_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(fixed_point_panel)

        Dim fixed_point_label As New Label With {
            .Text = "Fixed point definition",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        fixed_point_panel.Controls.Add(fixed_point_label)
        inner_item_y_pos += fixed_point_label.Height

        Dim unit_label As New Label With {
            .Text = "Unit :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        fixed_point_panel.Controls.Add(unit_label)
        Me.Unit_TexBox = New TextBox With {
            .BackColor = Background_Color,
            .ForeColor = Foreground_Color,
            .Text = default_unit,
            .Location = New Point(ESMT_Form.Marge + unit_label.Width, inner_item_y_pos),
            .Size = ESMT_Form.Field_Value_Size}
        fixed_point_panel.Controls.Add(Me.Unit_TexBox)
        inner_item_y_pos += Me.Unit_TexBox.Height + ESMT_Form.Marge

        Dim resol_label As New Label With {
            .Text = "Resolution :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        fixed_point_panel.Controls.Add(resol_label)
        Me.Resolution_TexBox = New TextBox With {
            .BackColor = Background_Color,
            .ForeColor = Foreground_Color,
            .Text = default_resolution,
            .Location = New Point(ESMT_Form.Marge + resol_label.Width, inner_item_y_pos),
            .Size = ESMT_Form.Field_Value_Size}
        fixed_point_panel.Controls.Add(Me.Resolution_TexBox)
        inner_item_y_pos += Me.Resolution_TexBox.Height + ESMT_Form.Marge

        Dim offset_label As New Label With {
            .Text = "Offset :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        fixed_point_panel.Controls.Add(offset_label)
        Me.Offset_TexBox = New TextBox With {
            .BackColor = Background_Color,
            .ForeColor = Foreground_Color,
            .Text = default_offset,
            .Location = New Point(ESMT_Form.Marge + offset_label.Width, inner_item_y_pos),
            .Size = ESMT_Form.Field_Value_Size}
        fixed_point_panel.Controls.Add(Me.Offset_TexBox)
        inner_item_y_pos += Me.Offset_TexBox.Height + ESMT_Form.Marge

        fixed_point_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += fixed_point_panel.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Get_Unit() As String
        Return Me.Unit_TexBox.Text
    End Function

    Public Function Get_Resolution() As String
        Return Me.Resolution_TexBox.Text
    End Function

    Public Function Get_Offset() As String
        Return Me.Offset_TexBox.Text
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Unit_TexBox.ReadOnly = True
        Me.Resolution_TexBox.ReadOnly = True
        Me.Offset_TexBox.ReadOnly = True
    End Sub

End Class

