Public Class Element_With_Ref_Form
    Inherits Element_Form

    Protected WithEvents Referenced_Element_ComboBox As ComboBox
    Private ReadOnly Referenceable_Element_By_Path As New Dictionary(Of String, Software_Element)

    Public Sub New(
            form_kind As E_Form_Kind,
            element_metaclass_name As String,
            default_uuid As String,
            default_name As String,
            default_description As String,
            ref_element_title As String,
            default_ref_element_path As String,
            ref_element_list As List(Of Software_Element))

        MyBase.New(
            form_kind,
            element_metaclass_name,
            default_uuid,
            default_name,
            default_description)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add referenced element path panel
        Dim ref_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(ref_panel)

        Dim ref_label As New Label With {
            .Text = ref_element_title,
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        ref_panel.Controls.Add(ref_label)
        inner_item_y_pos += ref_label.Height

        Me.Referenced_Element_ComboBox = New ComboBox
        If Not IsNothing(ref_element_list) Then
            For Each ref_element In ref_element_list
                Dim ref_element_path As String = ref_element.Get_Path()
                Me.Referenceable_Element_By_Path.Add(ref_element_path, ref_element)
                Me.Referenced_Element_ComboBox.Items.Add(ref_element_path)
            Next
        Else
            Me.Referenced_Element_ComboBox.Items.Add(default_ref_element_path)
        End If
        With Me.Referenced_Element_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_ref_element_path
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos)
            .Size = ESMT_Form.Label_Size
        End With
        ref_panel.Controls.Add(Me.Referenced_Element_ComboBox)
        inner_item_y_pos += Me.Referenced_Element_ComboBox.Height + ESMT_Form.Marge

        ref_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += ref_panel.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Protected Function Get_Ref_Element() As Software_Element
        If Me.Referenced_Element_ComboBox.Text <> "" Then
            Return Me.Referenceable_Element_By_Path(Me.Referenced_Element_ComboBox.Text)
        Else
            Return Nothing
        End If
    End Function

    Public Function Get_Ref_Element_Identifier() As Guid
        If Me.Referenced_Element_ComboBox.Text <> "" Then
            Return Me.Referenceable_Element_By_Path(Me.Referenced_Element_ComboBox.Text).Identifier
        Else
            Return Guid.Empty
        End If
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Referenced_Element_ComboBox.Enabled = False
    End Sub

End Class
