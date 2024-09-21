Imports System.IO

Public Class Recordable_Element_Form

    Inherits Element_Form

    Private Directory_TextBox As TextBox
    Private WithEvents Directory_Button As Button
    Private ReadOnly File_Name_TextBox As TextBox

    Private ReadOnly Element_File_Extension As String


    Public Sub New(
            form_kind As E_Form_Kind,
            element_metaclass_name As String,
            default_uuid As String,
            default_name As String,
            default_description As String,
            default_directory As String,
            default_file_name As String,
            file_extension As String)

        MyBase.New(
            form_kind,
            element_metaclass_name,
            default_uuid,
            default_name,
            default_description)

        Me.Element_File_Extension = file_extension

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add file directory selection panel
        inner_item_y_pos = Marge

        Dim dir_label As New Label With {
            .Text = "Directory",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        inner_item_y_pos += dir_label.Height

        Me.Directory_TextBox = New TextBox With {
            .BackColor = Background_Color,
            .ForeColor = Foreground_Color,
            .Location = New Point(Marge, inner_item_y_pos),
            .Size = Path_Text_Size,
            .Text = default_directory}

        Me.Directory_Button = New Button With {
            .BackColor = Foreground_Color,
            .ForeColor = Background_Color,
            .Location = New Point(Path_Button_X_Pos, inner_item_y_pos),
            .Size = ESMT_Form.Path_Button_Size,
            .Text = "..."}
        inner_item_y_pos += Me.Directory_TextBox.Height + ESMT_Form.Marge

        Dim dir_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle,
            .Size = New Size(Panel_Width, inner_item_y_pos)}
        With dir_panel.Controls
            .Add(dir_label)
            .Add(Me.Directory_TextBox)
            .Add(Me.Directory_Button)
        End With
        Me.Controls.Add(dir_panel)
        item_y_pos += dir_panel.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Add file name panel
        inner_item_y_pos = Marge

        Dim file_label As New Label With {
            .Text = "File name",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        inner_item_y_pos += file_label.Height

        Me.File_Name_TextBox = New TextBox With {
            .BackColor = Background_Color,
            .ForeColor = Foreground_Color,
            .Location = New Point(Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size,
            .Text = default_file_name & file_extension}
        inner_item_y_pos += Me.File_Name_TextBox.Height + ESMT_Form.Marge

        Dim file_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle,
            .Size = New Size(Panel_Width, inner_item_y_pos)}
        With file_panel.Controls
            .Add(file_label)
            .Add(Me.File_Name_TextBox)
        End With
        Me.Controls.Add(file_panel)
        item_y_pos += file_panel.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Design Create button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge


        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        If Me.Kind <> E_Form_Kind.CREATION_FORM Then
            Me.Set_My_Specific_Fields_Read_Only()
        Else
            Me.Checks_List = New List(Of Func(Of Boolean)) From {
                AddressOf Check_File_Name,
                AddressOf Check_File_Extension,
                AddressOf Check_Directory
            }
        End If

    End Sub


    Public Function Get_File_Full_Path() As String
        Return Me.Directory_TextBox.Text _
            & Path.DirectorySeparatorChar _
            & Me.File_Name_TextBox.Text
    End Function

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Set_My_Specific_Fields_Read_Only()
    End Sub

    Private Sub Set_My_Specific_Fields_Read_Only()
        Me.Directory_TextBox.ReadOnly = True
        Me.File_Name_TextBox.ReadOnly = True
        Me.Directory_Button.Visible = False
    End Sub

    Private Sub Name_Modified() Handles Name_TextBox.TextChanged
        If Me.Kind = E_Form_Kind.CREATION_FORM Then
            Me.File_Name_TextBox.Text = Me.Name_TextBox.Text & Me.Element_File_Extension
        End If
    End Sub

    Private Sub Path_Button_Clicked() Handles Directory_Button.Click
        ESMT_Form.Select_Directory("Choose directory", Me.Directory_TextBox)
    End Sub

    Private Function Check_File_Name() As Boolean
        Dim file_name_is_valid As Boolean = True
        Dim file_name_wo_ext_length As Integer
        file_name_wo_ext_length = Me.File_Name_TextBox.Text.Length _
                                - Me.Element_File_Extension.Length
        Dim file_name_wo_ext As String
        file_name_wo_ext = Me.File_Name_TextBox.Text.Substring(0, file_name_wo_ext_length)
        If Not Named_Element.Is_Symbol_Valid(file_name_wo_ext) Then
            MsgBox("Invalid file name", MsgBoxStyle.Exclamation)
            file_name_is_valid = False
        End If
        Return file_name_is_valid
    End Function

    Private Function Check_File_Extension() As Boolean
        Dim file_extension_is_valid As Boolean = True
        If Not Me.File_Name_TextBox.Text.EndsWith(Me.Element_File_Extension) Then
            MsgBox("Invalid file extension", MsgBoxStyle.Exclamation)
            file_extension_is_valid = False
        End If
        Return file_extension_is_valid
    End Function

    Private Function Check_Directory() As Boolean
        Dim directory_is_valid As Boolean = True
        If File.Exists(Me.Directory_TextBox.Text & Path.DirectorySeparatorChar &
                Me.File_Name_TextBox.Text) Then
            MsgBox(
                Me.File_Name_TextBox.Text & " already exists in " & Me.Directory_TextBox.Text,
                MsgBoxStyle.Critical)
            directory_is_valid = False
        End If
        Return directory_is_valid
    End Function

End Class