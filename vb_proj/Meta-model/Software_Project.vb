Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text

Public Class Software_Project

    Public Name As String
    Public Identifier As Guid
    Public Description As String

    <XmlArrayItemAttribute(GetType(Package_Reference)), XmlArray("Packages_References")>
    Public Packages_References_List As List(Of Package_Reference)

    Private Node As TreeNode

    Private Xml_File_Path As String
    Private ReadOnly Top_Level_Packages_List As New List(Of Top_Level_Package)

    Public Const Project_File_Extension As String = ".prjx"

    Private Shared ReadOnly Context_Menu As New Project_Context_Menu

    Private Shared ReadOnly Project_Serializer As New XmlSerializer(GetType(Software_Project))

    Public Const Metaclass_Name As String = "Project"

    Private Diagram_Viewer As WebBrowser

    Private ReadOnly Elements As New Dictionary(Of Guid, Software_Element)
    Private ReadOnly Types As New Dictionary(Of String, Type)
    Private ReadOnly Basic_Integer_Types As New Dictionary(Of String, Basic_Integer_Type)
    Private ReadOnly Interfaces As New Dictionary(Of String, Software_Interface)
    Private ReadOnly Component_Types As New Dictionary(Of String, Component_Type)


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
            model_browser As TreeView,
            diagram_viewer As WebBrowser)
        Me.Name = name
        Me.Description = desc
        Me.Identifier = Guid.NewGuid()
        Me.Create_Node()
        model_browser.Nodes.Add(Me.Node)
        Me.Xml_File_Path = file_path
        Me.Packages_References_List = New List(Of Package_Reference)
        Me.Diagram_Viewer = diagram_viewer
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' 
    ' -------------------------------------------------------------------------------------------- '

    Protected Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = Software_Project.Metaclass_Name,
            .SelectedImageKey = Software_Project.Metaclass_Name,
            .ContextMenuStrip = Software_Project.Context_Menu,
            .Tag = Me}
    End Sub

    Public Function Get_Children_Name() As List(Of String)
        Dim children_name As New List(Of String)
        For Each pkg In Me.Top_Level_Packages_List
            children_name.Add(pkg.Name)
        Next
        Return children_name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Loading method
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Load(
            project_file_path As String,
            model_browser As TreeView,
            diagram_viewer As WebBrowser) As Software_Project

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
            model_browser.Nodes.Add(new_sw_proj.Node)

            ' Set or initialize private attributes
            new_sw_proj.Xml_File_Path = project_file_path
            new_sw_proj.Diagram_Viewer = diagram_viewer

            ' Load the top level Packages aggregated by the project
            Environment.CurrentDirectory = Path.GetDirectoryName(project_file_path)
            For Each pkg_ref In new_sw_proj.Packages_References_List
                Dim new_pkg As Top_Level_Package

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

    Public Sub Edit()

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

        End If

    End Sub

    Public Sub View()

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

        Dim pkg_creation_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Package.Metaclass_Name,
            "",
            Package.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            Path.GetDirectoryName(Me.Xml_File_Path),
            Package.Metaclass_Name,
            Top_Level_Package.Package_File_Extension)

        Dim creation_result As DialogResult = pkg_creation_form.ShowDialog()
        If creation_result = DialogResult.OK Then

            Dim pkg_file_path As String = pkg_creation_form.Get_File_Full_Path()

            ' Create the new package
            Dim created_pkg As New Top_Level_Package(
                pkg_creation_form.Get_Element_Name(),
                pkg_creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                pkg_file_path)

            ' Add package to project
            Me.Record_Package(created_pkg.Name, pkg_file_path, True)
            Me.Top_Level_Packages_List.Add(created_pkg)

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

    Public Sub Add_Element_To_Project(element As Software_Element)
        Me.Elements.Add(element.Identifier, element)
        If TypeOf element Is Type Then
            Me.Types.Add(element.Get_Path, CType(element, Type))
            If TypeOf element Is Basic_Integer_Type Then
                Me.Basic_Integer_Types.Add(element.Get_Path, CType(element, Basic_Integer_Type))
            End If
        ElseIf TypeOf element Is Software_Interface Then
            Me.Interfaces.Add(element.Get_Path, CType(element, Software_Interface))
        ElseIf TypeOf element Is Component_Type Then
            Me.Component_Types.Add(element.Get_Path, CType(element, Component_Type))
        End If
    End Sub

    Public Sub Remove_Element_From_Project(element As Software_Element)
        Me.Elements.Remove(element.Identifier)
        If TypeOf element Is Type Then
            Me.Types.Remove(element.Get_Path)
            If TypeOf element Is Basic_Integer_Type Then
                Me.Basic_Integer_Types.Remove(element.Get_Path)
            End If
        ElseIf TypeOf element Is Software_Interface Then
            Me.Interfaces.Remove(element.Get_Path)
        ElseIf TypeOf element Is Component_Type Then
            Me.Component_Types.Remove(element.Get_Path)
        End If
    End Sub

    Public Sub Move_Element_In_Project(old_path As String, new_path As String)
        Dim elmt As Software_Element
        If Me.Types.ContainsKey(old_path) Then
            elmt = Me.Types(old_path)
            Me.Types.Remove(old_path)
            Me.Types.Add(new_path, CType(elmt, Type))
        ElseIf Me.Basic_Integer_Types.ContainsKey(old_path) Then
            elmt = Me.Basic_Integer_Types(old_path)
            Me.Basic_Integer_Types.Remove(old_path)
            Me.Basic_Integer_Types.Add(new_path, CType(elmt, Basic_Integer_Type))
        ElseIf Me.Interfaces.ContainsKey(old_path) Then
            elmt = Me.Interfaces(old_path)
            Me.Interfaces.Remove(old_path)
            Me.Interfaces.Add(new_path, CType(elmt, Software_Interface))
        ElseIf Me.Component_Types.ContainsKey(old_path) Then
            elmt = Me.Component_Types(old_path)
            Me.Component_Types.Remove(old_path)
            Me.Component_Types.Add(new_path, CType(elmt, Component_Type))
        End If
    End Sub

    Public Function Get_Element_By_Identifier(id As Guid) As Software_Element
        If Me.Elements.ContainsKey(id) Then
            Return Me.Elements(id)
        Else
            Return Nothing
        End If
    End Function

    Public Function Get_Type_By_Path(path As String) As Type
        If Me.Types.ContainsKey(path) Then
            Return Me.Types(path)
        Else
            Return Nothing
        End If
    End Function

    Public Function Get_All_Types_Path() As List(Of String)
        Return Me.Types.Keys.ToList
    End Function

    Public Function Get_Basic_Integer_Type_By_Path(path As String) As Basic_Integer_Type
        If Me.Basic_Integer_Types.ContainsKey(path) Then
            Return Me.Basic_Integer_Types(path)
        Else
            Return Nothing
        End If
    End Function

    Public Function Get_All_Basic_Integer_Types_Path() As List(Of String)
        Return Me.Basic_Integer_Types.Keys.ToList
    End Function

    Public Function Get_Interface_By_Path(path As String) As Software_Interface
        If Me.Interfaces.ContainsKey(path) Then
            Return Me.Interfaces(path)
        Else
            Return Nothing
        End If
    End Function

    Public Function Get_All_Interfaces_Path() As List(Of String)
        Return Me.Interfaces.Keys.ToList
    End Function

    Public Function Get_Component_Type_By_Path(path As String) As Component_Type
        If Me.Component_Types.ContainsKey(path) Then
            Return Me.Component_Types(path)
        Else
            Return Nothing
        End If
    End Function

    Public Function Get_All_Component_Types_Path() As List(Of String)
        Return Me.Component_Types.Keys.ToList
    End Function

    Public Sub Remove_Package(pkg_name As String)

        ' Ask for confirmation
        Dim confirmation_result As MsgBoxResult
        confirmation_result = MsgBox(
            "Do you confirm removal of the package ?",
            MsgBoxStyle.OkCancel)

        If confirmation_result = MsgBoxResult.Ok Then

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

        End If

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

    Public Sub Update_Diagram(sw_elmnt As Software_Element)
        Dim svg_file_path As String = sw_elmnt.Update_SVG_Diagram()
        Me.Diagram_Viewer.Navigate(svg_file_path)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' ------------------------------------------------------------------------------------------- '

    Public Function Get_SVG_File_Path() As String
        Dim svg_folder As String = Path.GetDirectoryName(Me.Xml_File_Path)
        Dim svg_file_full_path As String
        svg_file_full_path = svg_folder & Path.DirectorySeparatorChar & Me.Name & ".svg"
        Return svg_file_full_path
    End Function

    Public Function Update_SVG_Diagram() As String

        Dim svg_file_full_path As String = Me.Get_SVG_File_Path()
        Dim file_stream As New StreamWriter(svg_file_full_path, False)

        file_stream.WriteLine("<?xml version=""1.0"" encoding=""UTF-8""?>")
        file_stream.WriteLine("<svg")
        file_stream.WriteLine("  Version=""1.1""")
        file_stream.WriteLine("  xmlns=""http://www.w3.org/2000/svg""")
        file_stream.WriteLine("  xmlns:xlink=""http://www.w3.org/1999/xlink""")
        file_stream.WriteLine("  xmlns:svg=""http://www.w3.org/2000/svg""")
        file_stream.WriteLine("  width=""3000px"" height=""1000px"">")
        file_stream.WriteLine("  <style>text{font-size:" & SVG.SVG_FONT_SIZE &
         "px;font-family:Consolas;fill:black;text-anchor:start;}</style>")
        file_stream.WriteLine(Me.Compute_SVG_Content())

        file_stream.WriteLine("</svg>")
        file_stream.Close()
        Return svg_file_full_path

    End Function

    Public Function Compute_SVG_Content() As String
        Dim svg_content As String = ""
        Dim not_sorted_pkgs As New List(Of Top_Level_Package)
        not_sorted_pkgs.AddRange(Me.Top_Level_Packages_List)
        Dim sorted_pkgs As New List(Of Top_Level_Package)
        Dim sorted_pkgs_list As New List(Of List(Of Top_Level_Package))

        While not_sorted_pkgs.Count <> 0
            Dim not_sorted_pkgs_copy As New List(Of Top_Level_Package)
            not_sorted_pkgs_copy.AddRange(not_sorted_pkgs)
            Dim current_sorted_pkg As New List(Of Top_Level_Package)
            For Each pkg In not_sorted_pkgs_copy
                Dim all_need_pkg_are_sorted As Boolean = True
                For Each needed_pkg In pkg.Get_Needed_Element()
                    If Not sorted_pkgs.Contains(CType(needed_pkg, Top_Level_Package)) Then
                        all_need_pkg_are_sorted = False
                        Exit For
                    End If
                Next
                If all_need_pkg_are_sorted = True Then
                    current_sorted_pkg.Add(pkg)
                End If
            Next
            For Each pkg In current_sorted_pkg
                not_sorted_pkgs.Remove(pkg)
                sorted_pkgs.Add(pkg)
            Next
            sorted_pkgs_list.Add(current_sorted_pkg)
        End While

        Dim from_point_dico As New Dictionary(Of Top_Level_Package, SVG_POINT)
        Dim to_point_dico As New Dictionary(Of Top_Level_Package, SVG_POINT)


        Dim pkg_list_idx As Integer = 0
        For Each pkg_list In sorted_pkgs_list
            Dim pkg_idx As Integer = 0
            For Each pkg In pkg_list
                Dim x_pos = pkg_idx * (Package.SVG_PKG_BOX_WIDTH + 20)
                Dim y_pos = pkg_list_idx * 200
                svg_content &= pkg.Compute_SVG_Content()
                svg_content &= "  <use xlink:href=""#" & pkg.Get_SVG_Id() &
                                  """ transform=""translate(" & x_pos &
                                  "," & y_pos & ")"" />" & vbCrLf
                from_point_dico.Add(
                    pkg,
                    New SVG_POINT With {
                        .X_Pos = x_pos + pkg.Get_SVG_Width() \ 2,
                        .Y_Pos = y_pos})
                to_point_dico.Add(
                    pkg,
                    New SVG_POINT With {
                        .X_Pos = x_pos + pkg.Get_SVG_Width() \ 2,
                        .Y_Pos = y_pos + pkg.Get_SVG_Height()})
                pkg_idx += 1
            Next
            pkg_list_idx += 1
        Next

        svg_content &= Get_Open_Arrow_Marker()

        For Each pkg In Me.Top_Level_Packages_List
            For Each need_pkg In pkg.Get_Needed_Element
                svg_content &= Get_SVG_Line(
                    from_point_dico(pkg).X_Pos,
                    from_point_dico(pkg).Y_Pos,
                    to_point_dico(CType(need_pkg, Top_Level_Package)).X_Pos,
                    to_point_dico(CType(need_pkg, Top_Level_Package)).Y_Pos,
                    "black",
                    True,
                    "open_arrow")
            Next
        Next

        Return svg_content
    End Function


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
