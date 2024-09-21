Imports System.Globalization

Public Class Array_Type_Form
    Inherits Element_With_Ref_Form

    Private WithEvents First_Dim_TexBox As New TextBox
    Private WithEvents Second_Dim_TexBox As New TextBox
    Private WithEvents Third_Dim_TexBox As New TextBox

    Public Sub New(
            form_kind As E_Form_Kind,
            default_uuid As String,
            default_name As String,
            default_description As String,
            default_ref_element_path As String,
            ref_element_list As List(Of Software_Element),
            default_first_dim As String,
            default_second_dim As String,
            default_third_dim As String)

        MyBase.New(
            form_kind,
            Array_Type.Metaclass_Name,
            default_uuid,
            default_name,
            default_description,
            "Base Type",
            default_ref_element_path,
            ref_element_list)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Add dimension panel
        Dim dimension_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(dimension_panel)

        Dim dimension_label As New Label With {
            .Text = "Dimension",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        dimension_panel.Controls.Add(dimension_label)
        inner_item_y_pos += dimension_label.Height + ESMT_Form.Marge

        Dim first_dim_label As New Label With {
            .Text = "First dimension :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        dimension_panel.Controls.Add(first_dim_label)
        With Me.First_Dim_TexBox
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            .Text = default_first_dim
            .Location = New Point(ESMT_Form.Marge + first_dim_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        dimension_panel.Controls.Add(Me.First_Dim_TexBox)
        inner_item_y_pos += Me.First_Dim_TexBox.Height + ESMT_Form.Marge

        Dim second_dim_label As New Label With {
            .Text = "Second dimension :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        dimension_panel.Controls.Add(second_dim_label)
        With Me.Second_Dim_TexBox
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            .Text = default_second_dim
            .Location = New Point(ESMT_Form.Marge + second_dim_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        dimension_panel.Controls.Add(Me.Second_Dim_TexBox)
        inner_item_y_pos += Me.Second_Dim_TexBox.Height + ESMT_Form.Marge

        Dim third_dim_label As New Label With {
            .Text = "Third dimension :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        dimension_panel.Controls.Add(third_dim_label)
        With Me.Third_Dim_TexBox
            .BackColor = Background_Color
            .ForeColor = Foreground_Color
            .Text = default_third_dim
            .Location = New Point(ESMT_Form.Marge + third_dim_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        dimension_panel.Controls.Add(Me.Third_Dim_TexBox)
        inner_item_y_pos += Me.Third_Dim_TexBox.Height + ESMT_Form.Marge

        dimension_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += dimension_panel.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Get_First_Dimension() As String
        Return Me.First_Dim_TexBox.Text
    End Function

    Public Function Get_Second_Dimension() As String
        Return Me.Second_Dim_TexBox.Text
    End Function

    Public Function Get_Third_Dimension() As String
        Return Me.Third_Dim_TexBox.Text
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.First_Dim_TexBox.ReadOnly = True
        Me.Second_Dim_TexBox.ReadOnly = True
        Me.Third_Dim_TexBox.ReadOnly = True
    End Sub

End Class
