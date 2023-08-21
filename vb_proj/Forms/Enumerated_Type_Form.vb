﻿Public Class Enumerated_Type_Form
    Inherits Element_Form

    Private WithEvents Enumerals_Table As DataGridView

    Public Sub New(
            form_kind As E_Form_Kind,
            default_uuid As String,
            default_name As String,
            default_description As String,
            default_enumerals_data As DataTable)

        MyBase.New(
            form_kind,
            Enumerated_Type.Metaclass_Name,
            default_uuid,
            default_name,
            default_description)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add Enumerals panel
        Dim enumerals_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(enumerals_panel)

        Dim enumerals_label As New Label With {
            .Text = "Enumerals",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        enumerals_panel.Controls.Add(enumerals_label)
        inner_item_y_pos += enumerals_label.Height

        Me.Enumerals_Table = New DataGridView With {
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = New Size(ESMT_Form.Label_Width, 150),
            .DataSource = default_enumerals_data,
            .AllowUserToAddRows = True,
            .AllowUserToDeleteRows = True,
            .ScrollBars = ScrollBars.Vertical,
            .ColumnHeadersVisible = True,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill}
        enumerals_panel.Controls.Add(Me.Enumerals_Table)
        inner_item_y_pos += Me.Enumerals_Table.Height + ESMT_Form.Marge

        enumerals_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += enumerals_panel.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Enumerals_Table.ReadOnly = True
        Me.Enumerals_Table.AllowUserToDeleteRows = False
    End Sub

End Class
