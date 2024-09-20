Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports System.Reflection

Public Class Top_Level_Package
    Inherits Package

    Implements Dependent_Element

    Private Enum E_PACKAGE_STATUS
        LOCKED
        NOT_FOUND
        CORRUPTED
        READABLE
        WRITABLE
    End Enum

    Private Xml_File_Path As String
    Private Status As E_PACKAGE_STATUS = E_PACKAGE_STATUS.WRITABLE
    Private Project As Software_Project

    Private Shared ReadOnly Writable_Context_Menu As New Top_Package_Writable_Context_Menu
    Private Shared ReadOnly Readable_Context_Menu As New Top_Package_Readable_Context_Menu
    Private Shared ReadOnly Unloaded_Context_Menu As New Top_Package_Unloaded_Context_Menu

    Private Shared ReadOnly Pkg_Serializer As New XmlSerializer(GetType(Top_Level_Package))

    Public Const Package_File_Extension As String = ".pkgx"

    Private Has_Been_Modified_Since_Last_Metrics_Computation As Boolean = True

    Private ReadOnly Needed_Elements_List As New List(Of Classifier)
    Private ReadOnly Needed_Top_Packages_List As New List(Of Dependent_Element)


    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New()
    End Sub

    Public Sub New(
            name As String,
            description As String,
            owner As Software_Project,
            parent_node As TreeNode,
            file_path As String)
        MyBase.New(name, description, Nothing, parent_node)
        Me.Project = owner
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

    Public Overrides Function Get_Forbidden_Name_List() As List(Of String)
        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Project.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)
        Return forbidden_name_list
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
                    .Owner = Nothing
                    .Project = parent_project
                End With
                If is_writable = True Then
                    pkg.Status = E_PACKAGE_STATUS.WRITABLE
                Else
                    pkg.Status = E_PACKAGE_STATUS.READABLE
                End If
                pkg.Post_Treat_After_Deserialization(parent_node)
                If is_writable = False Then
                    pkg.Node.ContextMenuStrip = Top_Level_Package.Readable_Context_Menu
                    If Not IsNothing(pkg.Children) Then
                        For Each child In pkg.Children
                            child.Apply_Read_Only_Context_Menu()
                        Next
                    End If
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
            .Owner = Nothing
            .Project = parent_project
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
            .Owner = Nothing
            .Project = parent_project
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
        pkg.Owner = Nothing
        pkg.Project = parent_project
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
        If Not IsNothing(Me.Children) Then
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

    Public Function Get_Owner_Project() As Software_Project
        Return Me.Project
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
            my_directory,
            my_file_name,
            Top_Level_Package.Package_File_Extension)

        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = edit_form.Get_Element_Name()
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
        Me.Has_Been_Modified_Since_Last_Metrics_Computation = True
    End Sub

    Public Sub Display_Dependencies()
        Me.Find_Needed_Elements()
        Dim message As String
        If Me.Needed_Top_Packages_List.Count = 0 Then
            message = "This package does not depend on any other one."
        Else
            message = "This package depends on the following packages :" & vbCrLf
            For Each pkg In Me.Needed_Top_Packages_List
                message &= CType(pkg, Top_Level_Package).Name & vbCrLf
            Next
        End If
        MsgBox(message, MsgBoxStyle.OkOnly, "Package dependencies")

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for Dependent_Element
    ' -------------------------------------------------------------------------------------------- '

    Function Get_Needed_Element() As List(Of Dependent_Element) _
        Implements Dependent_Element.Get_Needed_Element
        If Me.Has_Been_Modified_Since_Last_Metrics_Computation = True Then
            Me.Find_Needed_Elements()
            Me.Has_Been_Modified_Since_Last_Metrics_Computation = False
        End If
        Return Me.Needed_Top_Packages_List
    End Function

    Private Sub Find_Needed_Elements()
        Me.Needed_Elements_List.Clear()
        Me.Needed_Top_Packages_List.Clear()
        Dim tmp_needed_elements_list = New List(Of Classifier)

        Dim pkg_list As New List(Of Package) From {Me}
        Me.Get_All_Sub_Packages(pkg_list)

        ' Parse the list of sub packages + Me
        Dim pkg As Package
        For Each pkg In pkg_list
            For Each swct In pkg.Component_Types
                tmp_needed_elements_list.AddRange(swct.Find_Needed_Elements())
            Next
            For Each sw_if In pkg.Interfaces
                tmp_needed_elements_list.AddRange(sw_if.Find_Needed_Elements())
            Next
            For Each data_type In pkg.Types
                tmp_needed_elements_list.AddRange(data_type.Find_Needed_Elements())
            Next
            For Each compo In pkg.Compositions
                tmp_needed_elements_list.AddRange(compo.Find_Needed_Elements())
            Next
        Next

        tmp_needed_elements_list = tmp_needed_elements_list.Distinct().ToList

        For Each element In tmp_needed_elements_list
            Dim owner_pkg As Top_Level_Package = element.Get_Top_Package()
            If owner_pkg.Identifier <> Me.Identifier Then
                If Not Me.Needed_Top_Packages_List.Contains(owner_pkg) Then
                    Me.Needed_Top_Packages_List.Add(owner_pkg)
                End If
                Me.Needed_Elements_List.Add(element)
            End If
        Next
    End Sub

End Class
