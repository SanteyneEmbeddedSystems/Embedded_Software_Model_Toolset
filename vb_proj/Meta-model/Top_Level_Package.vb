Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports System.Reflection

Public Class Top_Level_Package
    Inherits Package

    Private Enum E_PACKAGE_STATUS
        LOCKED
        NOT_FOUND
        CORRUPTED
        READABLE
        WRITABLE
    End Enum

    Private Xml_File_Path As String
    Private Status As E_PACKAGE_STATUS = E_PACKAGE_STATUS.WRITABLE

    Private Shared ReadOnly Writable_Context_Menu As New Top_Package_Writable_Context_Menu
    Private Shared ReadOnly Readable_Context_Menu As New Top_Package_Readable_Context_Menu
    Private Shared ReadOnly Unloaded_Context_Menu As New Top_Package_Unloaded_Context_Menu

    Private Shared ReadOnly Pkg_Serializer As New XmlSerializer(GetType(Top_Level_Package))

    Public Const Package_File_Extension As String = ".pkgx"


    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New()
    End Sub

    Public Sub New(
            name As String,
            description As String,
            owner As Software_Element,
            parent_node As TreeNode,
            file_path As String)
        MyBase.New(name, description, owner, parent_node)
        Me.Xml_File_Path = file_path
        Me.Status = E_PACKAGE_STATUS.WRITABLE
        Me.Save()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return False
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Top_Level_Package.Writable_Context_Menu
    End Function

    Protected Overrides Function Get_Path_Separator() As String
        Return ""
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Various method
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Load(
            parent_project As Software_Project,
            default_name As String, ' name to display if package not loaded
            file_path As String,
            parent_node As TreeNode,
            is_writable As Boolean) As Top_Level_Package

        Dim pkg As Top_Level_Package

        If Not File.Exists(file_path) Then
            pkg = Top_Level_Package.Create_Not_Found_Package(parent_project, default_name, parent_node)
            MsgBox("Package file not found : " & vbCrLf & file_path,
                MsgBoxStyle.Critical)
        Else
            Dim reader As New XmlTextReader(file_path)
            Try
                pkg = CType(Top_Level_Package.Pkg_Serializer.Deserialize(reader), Top_Level_Package)
                With pkg
                    .Xml_File_Path = file_path
                    .Owner = parent_project
                End With
                If is_writable = True Then
                    pkg.Status = E_PACKAGE_STATUS.WRITABLE
                Else
                    pkg.Status = E_PACKAGE_STATUS.READABLE
                End If
                pkg.Post_Treat_After_Deserialization(parent_node)
                If is_writable = False Then
                    pkg.Node.ContextMenuStrip = Top_Level_Package.Readable_Context_Menu
                End If
            Catch
                pkg = Top_Level_Package.Create_Corrupted_Package(parent_project, default_name, parent_node)
                MsgBox("Package file content is invalid : " & vbCrLf & file_path,
                    MsgBoxStyle.Critical)
            End Try
            reader.Close()
        End If

        pkg.Update_Display()

        Return pkg

    End Function

    Private Shared Function Create_Corrupted_Package(
            parent_project As Software_Project,
            pkg_name As String,
            parent_node As TreeNode) As Top_Level_Package

        Dim pkg As New Top_Level_Package

        pkg.Create_Node()
        parent_node.Nodes.Add(pkg.Node)

        With pkg
            .Name = pkg_name
            .Owner = parent_project
            .Node.ContextMenuStrip = Top_Level_Package.Unloaded_Context_Menu
            .Status = E_PACKAGE_STATUS.CORRUPTED
        End With

        Return pkg

    End Function

    Private Shared Function Create_Not_Found_Package(
            parent_project As Software_Project,
            pkg_name As String,
            parent_node As TreeNode) As Top_Level_Package

        Dim pkg As New Top_Level_Package

        pkg.Create_Node()
        parent_node.Nodes.Add(pkg.Node)

        With pkg
            .Name = pkg_name
            .Owner = parent_project
            .Node.ContextMenuStrip = Top_Level_Package.Unloaded_Context_Menu
            .Status = E_PACKAGE_STATUS.NOT_FOUND
        End With

        Return pkg

    End Function

    Public Shared Function Load_Basic_Types(
            parent_project As Software_Project,
            parent_node As TreeNode) As Top_Level_Package
        Dim pkg As Top_Level_Package
        Dim exe_assembly As Assembly = Assembly.GetExecutingAssembly()
        Dim ressource_name As String = "Embedded_Software_Model_Toolset.Basic_Types.pkgx"
        Dim file_stream As Stream = exe_assembly.GetManifestResourceStream(ressource_name)
        Dim reader As New XmlTextReader(file_stream)
        pkg = CType(Top_Level_Package.Pkg_Serializer.Deserialize(reader), Top_Level_Package)
        reader.Close()
        file_stream.Close()
        pkg.Status = E_PACKAGE_STATUS.LOCKED
        pkg.Owner = parent_project
        pkg.Post_Treat_After_Deserialization(parent_node)
        pkg.Update_Display()
        parent_node.Nodes.Remove(pkg.Node)
        parent_node.Nodes.Insert(0, pkg.Node)
        Return pkg
    End Function

    Private Sub Update_Display()
        Select Case Me.Status
            Case E_PACKAGE_STATUS.LOCKED
                Me.Node.Text = Me.Name & " (locked)"
            Case E_PACKAGE_STATUS.CORRUPTED
                Me.Node.Text = Me.Name & " (corrupted)"
            Case E_PACKAGE_STATUS.NOT_FOUND
                Me.Node.Text = Me.Name & " (not found)"
            Case E_PACKAGE_STATUS.READABLE
                Me.Node.Text = Me.Name & " (ref.)"
            Case E_PACKAGE_STATUS.WRITABLE
                Me.Node.Text = Me.Name
        End Select
    End Sub

    Public Sub Display_Saved()
        If Me.Status = E_PACKAGE_STATUS.WRITABLE Then
            Me.Node.Text = Me.Name
        Else
            Throw New System.Exception("Top level package should be writable.")
        End If
    End Sub

    Public Sub Display_Modified()
        If Me.Status = E_PACKAGE_STATUS.WRITABLE Then
            Me.Node.Text = Me.Name & " *"
        Else
            Throw New System.Exception("Top level package should be writable.")
        End If
    End Sub

    Public Function Is_Writable() As Boolean
        If Me.Status = E_PACKAGE_STATUS.WRITABLE Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub Delete()
        Me.Node.Remove()
    End Sub

    Public Sub Make_Read_Only()
        Me.Save()
        Me.Status = E_PACKAGE_STATUS.READABLE
        Me.Update_Display()
        Me.Node.ContextMenuStrip = Top_Level_Package.Readable_Context_Menu
        If Not IsNothing(Children) Then
            For Each child In Me.Children
                child.Apply_Read_Only_Context_Menu()
            Next
        End If
    End Sub

    Public Sub Make_Writable()
        Me.Status = E_PACKAGE_STATUS.WRITABLE
        Me.Update_Display()
        Me.Node.ContextMenuStrip = Top_Level_Package.Writable_Context_Menu
        If Not IsNothing(Children) Then
            For Each child In Me.Children
                child.Apply_Writable_Context_Menu()
            Next
        End If
    End Sub

    Public Function Get_Folder() As String
        Return Path.GetDirectoryName(Me.Xml_File_Path)
    End Function

    Public Overrides Function Get_SVG_File_Path() As String
        If Me.Status <> E_PACKAGE_STATUS.WRITABLE Then
            Return Path.GetTempPath() & Me.Name & ".svg"
        Else
            Return MyBase.Get_SVG_File_Path()
        End If
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        Dim my_directory As String
        Dim my_file_name As String
        my_directory = Path.GetDirectoryName(Me.Xml_File_Path)
        my_file_name = Path.GetFileNameWithoutExtension(Me.Xml_File_Path)

        Dim previous_name As String = Me.Name

        Dim edit_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Package.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Forbidden_Name_List(),
            my_directory,
            my_file_name,
            Top_Level_Package.Package_File_Extension)

        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Dim old_name As String = Me.Name
            Me.Name = edit_form.Get_Element_Name()
            Update_Project(old_name)
            Me.Node.Text = Me.Name
            If previous_name <> Me.Name Then
                Me.Get_Project().Update_Pkg_Known_Name(previous_name, Me.Name)
            End If
            Me.Description = edit_form.Get_Element_Description()
            Me.Update_Views()
        End If

    End Sub

    Public Overrides Sub View()

        Dim my_directory As String
        Dim my_file_name As String
        my_directory = Path.GetDirectoryName(Me.Xml_File_Path)
        my_file_name = Path.GetFileNameWithoutExtension(Me.Xml_File_Path)

        Dim view_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Package.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing,
            my_directory,
            my_file_name,
            Top_Level_Package.Package_File_Extension)

        view_form.ShowDialog()

    End Sub

    Public Sub Save()
        ' Initialize XML writer
        Dim writer As New XmlTextWriter(Me.Xml_File_Path, Encoding.UTF8) With {
            .Indentation = 2,
            .IndentChar = " "c,
            .Formatting = Formatting.Indented}

        ' Serialize Package
        Top_Level_Package.Pkg_Serializer.Serialize(writer, Me)

        ' Close writter
        writer.Close()

        Me.Display_Saved()
    End Sub

End Class
