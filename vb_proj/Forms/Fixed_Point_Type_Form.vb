Imports System.Globalization

Public Class Fixed_Point_Type_Form
    Inherits Element_With_Ref_Form

    Private WithEvents Unit_TexBox As TextBox
    Private WithEvents Resolution_TexBox As TextBox
    Private WithEvents Offset_TexBox As TextBox

    Public Sub New(
            form_kind As E_Form_Kind,
            element_metaclass_name As String,
            default_uuid As String,
            default_name As String,
            default_description As String,
            forbidden_name_list As List(Of String),
            default_ref_element_path As String,
            ref_element_path_list As List(Of String),
            default_unit As String,
            default_resolution As String,
            default_offset As String)

        MyBase.New(
            form_kind,
            element_metaclass_name,
            default_uuid,
            default_name,
            default_description,
            forbidden_name_list,
            "Base integer type",
            default_ref_element_path,
            ref_element_path_list)

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
            .Text = default_offset,
            .Location = New Point(ESMT_Form.Marge + offset_label.Width, inner_item_y_pos),
            .Size = ESMT_Form.Field_Value_Size}
        fixed_point_panel.Controls.Add(Me.Offset_TexBox)
        inner_item_y_pos += Me.Offset_TexBox.Height + ESMT_Form.Marge

        fixed_point_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += fixed_point_panel.Height + ESMT_Form.Marge

        Me.Checks_List.Add(AddressOf Check_Offset)
        Me.Checks_List.Add(AddressOf Check_Resolution)
        Me.Checks_List.Add(AddressOf Check_Unit)


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

    Private Function Check_Unit() As Boolean
        Dim is_unit_valid As Boolean = True
        If Me.Unit_TexBox.Text = "" Then
            is_unit_valid = False
            MsgBox(
                "Unit shall not be empty.",
                MsgBoxStyle.Exclamation)
        End If
        Return is_unit_valid
    End Function

    Private Function Check_Resolution() As Boolean
        Dim is_resol_valid As Boolean = True
        If Not Fixed_Point_Type.Is_Resolution_Valid(Me.Resolution_TexBox.Text) Then
            is_resol_valid = False
            MsgBox(
                "Resolution shall be a non null decimal value.",
                MsgBoxStyle.Exclamation)
        End If
        Return is_resol_valid
    End Function

    Private Function Check_Offset() As Boolean
        Dim is_offset_valid As Boolean = True
        If Not Fixed_Point_Type.Is_Offset_Valid(Me.Offset_TexBox.Text) Then
            is_offset_valid = False
            MsgBox(
                "Offset shall be a decimal value.",
                MsgBoxStyle.Exclamation)
        End If
        Return is_offset_valid
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Unit_TexBox.ReadOnly = True
        Me.Resolution_TexBox.ReadOnly = True
        Me.Offset_TexBox.ReadOnly = True
    End Sub

End Class

