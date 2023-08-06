Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text

Public Class Software_Project

    Inherits Must_Describe_Software_Element

    <XmlArrayItemAttribute(GetType(Package_Reference)), XmlArray("Packages_References")>
    Public Packages_References_List As List(Of Package_Reference)

    Private Xml_File_Path As String
    Private Top_Level_Packages_List As New List(Of Top_Level_Package)

    Public Shared ReadOnly Project_File_Extension As String = ".prjx"

    Private Shared Context_Menu As New Project_Context_Menu

    Private Shared Project_Serializer As New XmlSerializer(GetType(Software_Project))

    Public Shared ReadOnly Metaclass_Name As String = "Project"


    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    ' Default for deserialization
    Public Sub New()
    End Sub

    Public Sub New(
            name As String,
            desc As String,
            file_path As String,
            browser As TreeView)
        Me.Name = name
        Me.Description = desc
        Me.Identifier = Guid.NewGuid()
        Me.Create_Node()
        browser.Nodes.Add(Me.Node)
        Me.Xml_File_Path = file_path
        Me.Packages_References_List = New List(Of Package_Reference)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Project",
            .SelectedImageKey = "Project",
            .ContextMenuStrip = Software_Project.Context_Menu,
            .Tag = Me}
    End Sub

    Protected Overrides Function Get_Children() As List(Of Software_Element)
        If Me.Children_Is_Computed = False Then
            Me.Children_Is_Computed = True
            Me.Children.AddRange(Me.Top_Level_Packages_List)
        End If
        Return Me.Children
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return False
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        ' Currently not needed.
    End Sub

    Protected Overrides Sub Remove_Me()
        ' Currently not needed, a project cannot be removed.
    End Sub

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Throw New Exception("Project contextual menu shall not be modified.")
        Return Nothing
    End Function

    Public Overrides Function Get_Metaclass_Name() As String
        Return "Project"
    End Function

    Public Overrides Function Get_SVG_File_Path() As String
        Dim svg_folder As String = Path.GetDirectoryName(Me.Xml_File_Path)
        Dim svg_file_full_path As String
        svg_file_full_path = svg_folder & Path.DirectorySeparatorChar & Me.Name & ".svg"
        Return svg_file_full_path
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Loading method
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Load(
            project_file_path As String,
            browser As TreeView) As Software_Project

        Dim new_sw_proj As Software_Project = Nothing

        ' Deserialize xml file containing project data
        Dim reader As New XmlTextReader(project_file_path)
        Try
            new_sw_proj = CType(Software_Project.Project_Serializer.Deserialize(reader),
                Software_Project)
        Catch
            MsgBox("The project file is invalid !", MsgBoxStyle.Critical)
        End Try
        reader.Close()

        ' Check that deserialization is OK
        If Not IsNothing(new_sw_proj) Then

            ' Add project in browser
            new_sw_proj.Create_Node()
            browser.Nodes.Add(new_sw_proj.Node)

            ' Set or initialize private attributes
            new_sw_proj.Xml_File_Path = project_file_path

            ' Load the top level Packages aggregated by the project
            Environment.CurrentDirectory = Path.GetDirectoryName(project_file_path)
            For Each pkg_ref In new_sw_proj.Packages_References_List
                Dim new_pkg As Top_Level_Package = Nothing

                ' Compute full file path of the package to be loaded
                Dim new_pkg_path As String
                new_pkg_path = Path.GetFullPath(pkg_ref.Relative_Path)
                pkg_ref.Set_Full_Path(new_pkg_path)

                new_pkg = Top_Level_Package.Load(
                    new_sw_proj,
                    pkg_ref.Last_Known_Name,
                    new_pkg_path,
                    new_sw_proj.Node,
                    pkg_ref.Is_Writable)

                pkg_ref.Last_Known_Name = new_pkg.Name

                If Not IsNothing(new_pkg) Then
                    new_sw_proj.Top_Level_Packages_List.Add(new_pkg)
                End If
            Next
        End If

        Return new_sw_proj
    End Function

    Public Sub Add_Predefined_Package()
        Dim types_pkg As Top_Level_Package = Top_Level_Package.Load_Basic_Types(Me, Me.Node)
        Me.Top_Level_Packages_List.Insert(0, types_pkg)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        Dim my_directory As String
        Dim my_file_name As String
        my_directory = Path.GetDirectoryName(Me.Xml_File_Path)
        my_file_name = Path.GetFileNameWithoutExtension(Me.Xml_File_Path)

        Dim prj_edit_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Software_Project.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing,
            my_directory,
            my_file_name,
            Software_Project.Project_File_Extension)

        Dim edit_result As DialogResult
        edit_result = prj_edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = prj_edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = prj_edit_form.Get_Element_Description()
            Me.Display_Modified()
        End If

    End Sub

    Public Overrides Sub View()

        Dim my_directory As String
        Dim my_file_name As String
        my_directory = Path.GetDirectoryName(Me.Xml_File_Path)
        my_file_name = Path.GetFileNameWithoutExtension(Me.Xml_File_Path)

        Dim view_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Software_Project.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing,
            my_directory,
            my_file_name,
            Software_Project.Project_File_Extension)

        view_form.ShowDialog()

    End Sub

    Public Sub Save()

        ' Save top level packages
        For Each pkg In Me.Top_Level_Packages_List
            If pkg.Is_Writable() Then
                pkg.Save()
            End If
        Next

        ' Save Me
        Dim writer As New XmlTextWriter(Me.Xml_File_Path, Encoding.UTF8) With {
            .Indentation = 2,
            .IndentChar = " "c,
            .Formatting = Formatting.Indented}
        Software_Project.Project_Serializer.Serialize(writer, Me)
        writer.Close()

        ' Update model browser view
        Me.Display_Saved()

    End Sub

    Public Sub Load_Package(is_writable As Boolean)

        ' Display a form asking for package file
        Dim load_pkg_dialog = New OpenFileDialog With {
            .Title = "Select software model package file",
            .Filter = "Package file|*" & Top_Level_Package.Package_File_Extension,
            .CheckFileExists = True}
        Dim result As DialogResult = load_pkg_dialog.ShowDialog()

        ' Test the result from the form
        If result = DialogResult.OK Then
            ' The user has clicked on "Open"

            Dim pkg_file_path As String = load_pkg_dialog.FileName

            ' Check that the select file is not already loaded
            If Me.Is_Package_Loaded(pkg_file_path) Then
                ' File is already loaded
                ' Display an error message
                MsgBox("This package is already loaded", MsgBoxStyle.Exclamation)
            Else
                ' File is not already loaded
                ' Load the package from the file given by user
                Dim loaded_pkg As Top_Level_Package = Top_Level_Package.Load(
                    Me,
                    "temp",
                    pkg_file_path,
                    Me.Node,
                    is_writable)
                If Not IsNothing(loaded_pkg) Then
                    Me.Record_Package(loaded_pkg.Name, pkg_file_path, is_writable)
                    Me.Top_Level_Packages_List.Add(loaded_pkg)
                    Me.Display_Modified()
                End If
            End If
        End If
    End Sub

    Public Sub Create_Package()

        Dim proposed_directory As String = Path.GetDirectoryName(Me.Xml_File_Path)

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Get_Children_Name()

        Dim pkg_creation_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Package.Metaclass_Name,
            "",
            Package.Metaclass_Name,
            "A good description is always useful.",
            forbidden_name_list,
            proposed_directory,
            Package.Metaclass_Name,
            Top_Level_Package.Package_File_Extension)

        Dim creation_result As DialogResult = pkg_creation_form.ShowDialog()
        If creation_result = DialogResult.OK Then

            Dim pkg_file_path As String = pkg_creation_form.Get_File_Full_Path()

            ' Create the new package
            Dim created_pkg As Top_Level_Package = Nothing
            created_pkg = New Top_Level_Package(
                pkg_creation_form.Get_Element_Name(),
                pkg_creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                pkg_file_path)

            ' Add package to project
            Me.Record_Package(created_pkg.Name, pkg_file_path, True)
            Me.Top_Level_Packages_List.Add(created_pkg)
            Me.Children.Add(created_pkg)

            Me.Display_Modified()

        End If

    End Sub

    Public Sub Check_Model()
        Dim report As New Consistency_Check_Report(Me.Name)
        For Each pkg In Me.Top_Level_Packages_List
            pkg.Check_Consistency(report)
        Next
        Dim report_file_path As String
        report_file_path = report.Generate_Report(
            E_Report_File_Format.CSV,
            Path.GetDirectoryName(Me.Xml_File_Path),
            False,
            False)
        Process.Start(report_file_path)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for model management
    ' -------------------------------------------------------------------------------------------- '

    Public Function Get_Type_List() As List(Of Type)
        Dim type_list As New List(Of Type)
        For Each pkg In Me.Top_Level_Packages_List
            pkg.Complete_Type_List(type_list)
        Next
        Return type_list
    End Function

    Public Sub Remove_Package(pkg_name As String)
        ' Remove from packages list
        For Each pkg In Me.Top_Level_Packages_List
            If pkg.Name = pkg_name Then
                ' Remove node
                pkg.Delete()
                Me.Top_Level_Packages_List.Remove(pkg)
                Exit For
            End If
        Next

        ' Remove from package references list
        For Each ref In Me.Packages_References_List
            If ref.Last_Known_Name = pkg_name Then
                Packages_References_List.Remove(ref)
                Exit For
            End If
        Next

        Me.Display_Modified()

    End Sub

    Public Sub Display_Package_File_Path(pkg_name As String)
        For Each ref In Me.Packages_References_List
            If ref.Last_Known_Name = pkg_name Then
                MsgBox(
                    pkg_name & "file path :" & vbCrLf &
                    "relative : " & ref.Relative_Path & vbCrLf &
                    "local : " & ref.Get_Full_Path(),
                    MsgBoxStyle.Information,
                    "Package file path")
                Exit Sub
            End If
        Next
        ' Reached if package not found
        Throw New Exception("Package not found !")
    End Sub

    Public Sub Make_Package_Read_Only(pkg_name As String)

        For Each pkg In Me.Top_Level_Packages_List
            If pkg.Name = pkg_name Then
                pkg.Make_Read_Only()
                Exit For
            End If
        Next

        For Each ref In Me.Packages_References_List
            If ref.Last_Known_Name = pkg_name Then
                ref.Is_Writable = False
                Exit For
            End If
        Next

        Me.Display_Modified()

    End Sub

    Public Sub Make_Package_Writable(pkg_name As String)

        For Each pkg In Me.Top_Level_Packages_List
            If pkg.Name = pkg_name Then
                pkg.Make_Writable()
                Exit For
            End If
        Next

        For Each ref In Me.Packages_References_List
            If ref.Last_Known_Name = pkg_name Then
                ref.Is_Writable = True
                Exit For
            End If
        Next

        Me.Display_Modified()

    End Sub

    Public Sub Update_Pkg_Known_Name(previous_name As String, new_name As String)
        For Each ref In Me.Packages_References_List
            If ref.Last_Known_Name = previous_name Then
                ref.Last_Known_Name = new_name
                Exit For
            End If
        Next
        Me.Display_Modified()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Private methods
    ' -------------------------------------------------------------------------------------------- '

    ' Create a new Package_Reference and add it to My list
    Private Sub Record_Package(pkg_name As String, pkg_file_path As String, is_writable As Boolean)
        Dim relative_pkg_path As String
        relative_pkg_path = Make_Relative_Path(Me.Xml_File_Path, pkg_file_path)
        Dim pkg_ref As New Package_Reference With {
            .Last_Known_Name = pkg_name,
            .Relative_Path = relative_pkg_path,
            .Is_Writable = is_writable}
        pkg_ref.Set_Full_Path(pkg_file_path)
        Me.Packages_References_List.Add(pkg_ref)
        Me.Save()
    End Sub

    Private Sub Display_Saved()
        Me.Node.Text = Me.Name
    End Sub

    Private Sub Display_Modified()
        Me.Node.Text = Me.Name & " *"
    End Sub

    Private Function Is_Package_Loaded(pkg_file_path As String) As Boolean
        Dim result As Boolean = False
        For Each ref In Me.Packages_References_List
            If ref.Get_Full_Path() = pkg_file_path Then
                result = True
                Exit For
            End If
        Next
        Return result
    End Function

End Class


Public Class Package_Reference

    Public Last_Known_Name As String
    Public Relative_Path As String
    Public Is_Writable As Boolean

    Private Full_Path As String

    Public Sub Set_Full_Path(path As String)
        Me.Full_Path = path
    End Sub

    Public Function Get_Full_Path() As String
        Return Me.Full_Path
    End Function

End Class
