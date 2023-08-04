Public Class Element_Form

    Inherits ESMT_Form

    Public Enum E_Form_Kind
        CREATION_FORM
        EDITION_FORM
        VIEW_FORM
    End Enum

    Protected Kind As E_Form_Kind

    Private UUID_TextBox As TextBox
    Protected WithEvents Name_TextBox As TextBox
    Private Description_TextBox As RichTextBox
    Protected WithEvents Main_Button As Button

    Private Forbidden_Names As List(Of String)

    Protected Checks_List As New List(Of Func(Of Boolean))

    Public Sub New(
            form_kind As E_Form_Kind,
            element_metaclass_name As String,
            default_uuid As String,
            default_name As String,
            default_description As String,
            forbidden_name_list As List(Of String))

        Me.Kind = form_kind

        Me.Forbidden_Names = forbidden_name_list


        Dim item_y_pos As Integer = ESMT_Form.Marge
        Dim inner_item_y_pos As Integer

        '------------------------------------------------------------------------------------------'
        ' Add element UUID panel
        If Me.Kind <> E_Form_Kind.CREATION_FORM Then
            inner_item_y_pos = ESMT_Form.Marge

            Dim uuid_panel As New Panel With {
                .Location = New Point(ESMT_Form.Marge, item_y_pos),
                .BorderStyle = BorderStyle.FixedSingle}
            Me.Controls.Add(uuid_panel)

            Dim uuid_label As New Label With {
                .Text = "UUID",
                .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
                .Size = ESMT_Form.Label_Size}
            uuid_panel.Controls.Add(uuid_label)
            inner_item_y_pos += uuid_label.Height

            Me.UUID_TextBox = New TextBox With {
                .Text = default_uuid,
                .ReadOnly = True,
                .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
                .Size = ESMT_Form.Label_Size}
            uuid_panel.Controls.Add(Me.UUID_TextBox)
            inner_item_y_pos += Me.UUID_TextBox.Height + ESMT_Form.Marge

            uuid_panel.Size = New Size(Panel_Width, inner_item_y_pos)
            item_y_pos += uuid_panel.Height + ESMT_Form.Marge
        End If


        '------------------------------------------------------------------------------------------'
        ' Add name panel
        inner_item_y_pos = ESMT_Form.Marge

        Dim name_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(name_panel)

        Dim name_label As New Label With {
            .Text = "Name",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        name_panel.Controls.Add(name_label)
        inner_item_y_pos += name_label.Height

        Me.Name_TextBox = New TextBox With {
            .Text = default_name,
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        name_panel.Controls.Add(Me.Name_TextBox)
        inner_item_y_pos += Me.Name_TextBox.Height + ESMT_Form.Marge

        name_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += name_panel.Height + ESMT_Form.Marge

        Me.Checks_List.Add(AddressOf Check_Name)
        Me.Checks_List.Add(AddressOf Check_Brothers_Name)


        '------------------------------------------------------------------------------------------'
        ' Add description panel
        inner_item_y_pos = Marge

        Dim desc_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(desc_panel)

        Dim description_label As New Label With {
            .Text = "Description",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        desc_panel.Controls.Add(description_label)
        inner_item_y_pos += description_label.Height

        Me.Description_TextBox = New RichTextBox With {
            .Text = default_description,
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = New Size(ESMT_Form.Label_Width, 100)}
        desc_panel.Controls.Add(Me.Description_TextBox)
        inner_item_y_pos += Me.Description_TextBox.Height + ESMT_Form.Marge

        desc_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += desc_panel.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Design main button
        Me.Main_Button = New Button With {
            .Size = ESMT_Form.Button_Size,
            .Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)}
        Select Case Me.Kind
            Case E_Form_Kind.CREATION_FORM
                Me.Main_Button.Text = "Create"
            Case E_Form_Kind.EDITION_FORM
                Me.Main_Button.Text = "Apply"
            Case E_Form_Kind.VIEW_FORM
                Me.Main_Button.Text = "OK"
        End Select
        Me.Controls.Add(Me.Main_Button)

        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Design Form
        Select Case Me.Kind
            Case E_Form_Kind.CREATION_FORM
                Me.Text = "Create"
                Me.Name_TextBox.Select()
            Case E_Form_Kind.EDITION_FORM
                Me.Text = "Edit"
                Me.Name_TextBox.Select()
            Case E_Form_Kind.VIEW_FORM
                Me.Text = "View"
                Me.Main_Button.Select()
        End Select
        Me.Text &= " " & element_metaclass_name
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.FormBorderStyle = FormBorderStyle.FixedDialog

    End Sub

    Public Function Get_Element_Name() As String
        Return Me.Name_TextBox.Text
    End Function

    Public Function Get_Element_Description() As String
        Return Me.Description_TextBox.Text
    End Function

    Protected Overridable Sub Set_Fields_Read_Only()
        Me.Name_TextBox.ReadOnly = True
        Me.Description_TextBox.ReadOnly = True
    End Sub

    Private Sub Main_Button_Clicked() Handles Main_Button.Click

        Dim elmt_is_valid As Boolean = True

        If Me.Kind <> E_Form_Kind.VIEW_FORM Then
            Dim check_function As Func(Of Boolean)
            For Each check_function In Me.Checks_List
                elmt_is_valid = check_function()
                ' Display only one error
                If elmt_is_valid = False Then
                    Exit For
                End If
            Next
        End If

        If elmt_is_valid = True Then
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        End If

    End Sub

    Private Function Check_Name() As Boolean
        Dim name_is_valid As Boolean = True
        If Not Software_Element.Is_Symbol_Valid(Me.Name_TextBox.Text) Then
            MsgBox("Invalid name", MsgBoxStyle.Exclamation)
            name_is_valid = False
        End If
        Return name_is_valid
    End Function

    Private Function Check_Brothers_Name() As Boolean
        Dim name_is_valid As Boolean = True
        If Not IsNothing(Me.Forbidden_Names) Then
            If Me.Forbidden_Names.Contains(Me.Name_TextBox.Text) Then
                MsgBox(
                    "A element with the same name is already aggregated",
                    MsgBoxStyle.Exclamation)
                name_is_valid = False
            End If
        End If
        Return name_is_valid
    End Function

    Private Sub Form_Created() Handles Me.Load
        If Me.Kind = E_Form_Kind.VIEW_FORM Then
            Me.Description_TextBox.BackColor = Color.FromArgb(255, 240, 240, 240)
            Me.Set_Fields_Read_Only()
        End If
    End Sub

End Class
