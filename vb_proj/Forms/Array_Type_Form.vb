Imports System.Globalization

Public Class Array_Type_Form
    Inherits Element_With_Ref_Form

    Private WithEvents Multiplicity_TexBox As TextBox

    Public Sub New(
            form_kind As E_Form_Kind,
            element_metaclass_name As String,
            default_uuid As String,
            default_name As String,
            default_description As String,
            forbidden_name_list As List(Of String),
            default_ref_element_path As String,
            ref_element_path_list As List(Of String),
            default_multiplicity As String)

        MyBase.New(
            form_kind,
            element_metaclass_name,
            default_uuid,
            default_name,
            default_description,
            forbidden_name_list,
            "Base Type",
            default_ref_element_path,
            ref_element_path_list)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Add multiplicity panel
        Dim multiplicity_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(multiplicity_panel)

        Dim multiplicity_label As New Label With {
            .Text = "Multiplicity",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        multiplicity_panel.Controls.Add(multiplicity_label)
        inner_item_y_pos += multiplicity_label.Height

        Me.Multiplicity_TexBox = New TextBox With {
            .Text = default_multiplicity,
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        multiplicity_panel.Controls.Add(Me.Multiplicity_TexBox)
        inner_item_y_pos += Me.Multiplicity_TexBox.Height + ESMT_Form.Marge

        multiplicity_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += multiplicity_panel.Height + ESMT_Form.Marge

        Me.Checks_List.Add(AddressOf Check_Multiplicity)


        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Get_Multiplicity() As String
        Return Me.Multiplicity_TexBox.Text
    End Function

    Private Function Check_Multiplicity() As Boolean
        Dim is_multiplicity_valid As Boolean = False
        Dim multiplicity As UInteger = 0
        Dim is_multplicity_integer As Boolean
        is_multplicity_integer = UInteger.TryParse(
            Multiplicity_TexBox.Text,
            NumberStyles.Any,
            CultureInfo.GetCultureInfo("en-US"),
            multiplicity)
        If is_multplicity_integer = True Then
            If multiplicity >= 1 Then
                is_multiplicity_valid = True
            End If
        End If
        If is_multiplicity_valid = False Then
            MsgBox(
                "Multiplicity shall be a integer equal or lager than 1.",
                MsgBoxStyle.Exclamation)
        End If
        Return is_multiplicity_valid
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Multiplicity_TexBox.ReadOnly = True
    End Sub

End Class
