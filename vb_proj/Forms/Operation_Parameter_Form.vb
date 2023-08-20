Public Class Operation_Parameter_Form
    Inherits Element_With_Ref_Form

    Private WithEvents Direction_ComboBox As New ComboBox

    Public Sub New(
            form_kind As E_Form_Kind,
            default_uuid As String,
            default_name As String,
            default_description As String,
            forbidden_name_list As List(Of String),
            default_ref_type_path As String,
            ref_type_list As List(Of Software_Element),
            directions As String(),
            default_direction As String)

        MyBase.New(
            form_kind,
            Operation_Parameter.Metaclass_Name,
            default_uuid,
            default_name,
            default_description,
            forbidden_name_list,
            "Type",
            default_ref_type_path,
            ref_type_list)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add Direction panel
        Dim direction_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(direction_panel)

        Dim direction_label As New Label With {
            .Text = "Direction",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        direction_panel.Controls.Add(direction_label)
        inner_item_y_pos += direction_label.Height

        With Me.Direction_ComboBox
            .Items.AddRange(directions)
            .Text = default_direction
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos)
            .Size = ESMT_Form.Label_Size
            .DropDownStyle = ComboBoxStyle.DropDownList
            .BackColor = Color.FromName("Control")
        End With
        direction_panel.Controls.Add(Me.Direction_ComboBox)
        inner_item_y_pos += Me.Direction_ComboBox.Height + ESMT_Form.Marge

        direction_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += direction_panel.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Get_Direction() As String
        Return Me.Direction_ComboBox.Text
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Direction_ComboBox.Enabled = False
    End Sub

End Class

