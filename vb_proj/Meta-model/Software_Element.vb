Imports System.Text.RegularExpressions
Imports System.IO

Public MustInherit Class Software_Element

    Public Name As String
    Public Identifier As Guid
    Public Description As String

    Protected Node As TreeNode

    Protected Owner As Software_Element = Nothing
    Protected Children As New List(Of Software_Element)
    Protected Children_Is_Computed As Boolean = False

    Public Const NB_CHARS_MAX_FOR_SYMBOL As Integer = 32
    Protected Shared Valid_Symbol_Regex As String =
        "^[a-zA-Z][a-zA-Z0-9_]{1," & NB_CHARS_MAX_FOR_SYMBOL - 1 & "}$"

    Protected Shared ReadOnly Read_Only_Context_Menu As New Read_Only_Context_Menu
    Private Shared ReadOnly Leaf_Context_Menu As New Leaf_Context_Menu

    Protected Shared ReadOnly Name_Pattern_Rule As New Modeling_Rule(
        "Name_Pattern",
        "Name shall match " & Valid_Symbol_Regex)
    Private Shared ReadOnly Brother_Rule As New Modeling_Rule(
        "Brother_Name",
        "Elements aggregated by the same owner shall have a different name.")

    Protected Const SVG_MIN_CHAR_PER_LINE As Integer = NB_CHARS_MAX_FOR_SYMBOL

    Protected SVG_Content As String = ""
    Protected SVG_Width As Integer = 0
    Protected SVG_Height As Integer = 0


    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New()
    End Sub

    Public Sub New(
            name As String,
            description As String,
            owner As Software_Element,
            parent_node As TreeNode)
        Me.Name = name
        Me.Description = description
        Me.Identifier = Guid.NewGuid()
        Me.Owner = owner
        Me.Create_Node()
        parent_node.Nodes.Add(Me.Node)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Shared
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Is_Symbol_Valid(symbol As String) As Boolean
        Return Regex.IsMatch(symbol, Software_Element.Valid_Symbol_Regex)
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Generic methods
    ' -------------------------------------------------------------------------------------------- '

    Public MustOverride Function Get_Metaclass_Name() As String

    ' Shall be overridden by all non-leaf software elements
    Protected Overridable Function Get_Children() As List(Of Software_Element)
        Return Nothing
    End Function

    ' Shall be overridden by "attribute like" software elements
    Protected Overridable Function Get_Path_Separator() As String
        Return "::"
    End Function

    Protected Sub Post_Treat_After_Deserialization(parent_node As TreeNode)
        Me.Create_Node()
        parent_node.Nodes.Add(Me.Node)
        If Not Me.Get_Top_Package().Is_Writable() Then
            Me.Node.ContextMenuStrip = Software_Element.Read_Only_Context_Menu
        End If
        Me.Get_Project().Add_Element_To_Project(Me)
        Dim children As List(Of Software_Element) = Me.Get_Children()
        If Not IsNothing(children) Then
            For Each child In children
                child.Owner = Me
                child.Post_Treat_After_Deserialization(Me.Node)
            Next
        End If
    End Sub

    Public Function Get_Children_Name() As List(Of String)
        Dim children_name As New List(Of String)
        Dim children As List(Of Software_Element) = Me.Get_Children()
        If Not IsNothing(children) Then
            For Each child In children
                children_name.Add(child.Name)
            Next
        End If
        Return children_name
    End Function

    Public Overridable Function Get_Forbidden_Name_List() As List(Of String)
        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)
        Return forbidden_name_list
    End Function

    Public Function Get_Path() As String
        Dim my_path As String = Me.Get_Path_Separator() & Me.Name
        Dim parent As Software_Element = Me.Owner
        While Not IsNothing(parent)
            my_path = parent.Get_Path_Separator() & parent.Name & my_path
            parent = parent.Owner
        End While
        Return my_path
    End Function

    Protected Function Get_Project() As Software_Project
        Return Get_Top_Package().Get_Owner_Project()
    End Function

    Public Function Get_Top_Package() As Top_Level_Package
        Dim current_element As Software_Element = Me
        While Not IsNothing(current_element.Owner)
            current_element = current_element.Owner
        End While
        Return CType(current_element, Top_Level_Package)
    End Function

    Protected Function Get_Top_Package_Folder() As String
        Dim top_pkg As Top_Level_Package = Me.Get_Top_Package()
        Return top_pkg.Get_Folder()
    End Function

    Public Function Get_Elmt_From_Prj_By_Id(id As Guid) As Software_Element
        Return Me.Get_Project().Get_Element_By_Identifier(id)
    End Function

    Public Function Get_Elmt_Name_From_Proj_By_Id(id As Guid) As String
        Dim element As Software_Element = Me.Get_Project().Get_Element_By_Identifier(id)
        If IsNothing(element) Then
            Return "unresolved"
        Else
            Return element.Name
        End If
    End Function

    Public Function Get_Elmt_Path_From_Proj_By_Id(id As Guid) As String
        Dim element As Software_Element = Me.Get_Project().Get_Element_By_Identifier(id)
        If IsNothing(element) Then
            Return "unresolved"
        Else
            Return element.Get_Path()
        End If
    End Function

    Protected Function Get_All_Types_From_Project() As List(Of Software_Element)
        Return Me.Get_Project().Get_All_Types()
    End Function

    Protected Function Get_All_Basic_Int_From_Project() As List(Of Software_Element)
        Return Me.Get_Project().Get_All_Basic_Integer_Types()
    End Function

    Protected Function Get_All_Interfaces_From_Project() As List(Of Software_Element)
        Return Me.Get_Project().Get_All_Interfaces()
    End Function

    Protected Function Get_All_Component_Types_From_Project() As List(Of Software_Element)
        Return Me.Get_Project().Get_All_Component_Types()
    End Function

    Public Function Get_Owner() As Software_Element
        Return Me.Owner
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for tree view management
    ' -------------------------------------------------------------------------------------------- '

    Public MustOverride Function Is_Allowed_Parent(parent As Software_Element) As Boolean

    Protected MustOverride Sub Move_Me(new_parent As Software_Element)

    Protected MustOverride Sub Remove_Me()

    Protected Overridable Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Software_Element.Leaf_Context_Menu
    End Function

    Protected Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = Me.Get_Metaclass_Name(),
            .SelectedImageKey = Me.Get_Metaclass_Name(),
            .ContextMenuStrip = Get_Writable_Context_Menu(),
            .Tag = Me}
    End Sub

    Private Sub Display_Package_Modified()
        ' Display, in the tree view, that top package is modified (not saved)
        Dim owner_pkg As Top_Level_Package = Me.Get_Top_Package()
        owner_pkg.Display_Modified()
    End Sub

    Public Sub Move(new_parent As Software_Element)

        ' Manage top level packages
        Me.Display_Package_Modified()
        new_parent.Display_Package_Modified()

        ' Update model
        Me.Move_Me(new_parent)
        Me.Owner.Children.Remove(Me)
        Me.Owner = new_parent
        new_parent.Children.Add(Me)
        Dim new_path = Me.Get_Path()

        ' Manage TreeNode
        Me.Node.Remove()
        new_parent.Node.Nodes.Add(Me.Node)

    End Sub

    Public Sub Apply_Read_Only_Context_Menu()
        Me.Node.ContextMenuStrip = Software_Element.Read_Only_Context_Menu
        If Not IsNothing(Me.Children) Then
            For Each child In Me.Children
                child.Apply_Read_Only_Context_Menu()
            Next
        End If
    End Sub

    Public Sub Apply_Writable_Context_Menu()
        Me.Node.ContextMenuStrip = Me.Get_Writable_Context_Menu()
        If Not IsNothing(Me.Children) Then
            For Each child In Me.Children
                child.Apply_Writable_Context_Menu()
            Next
        End If
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overridable Sub Edit()
        Dim edit_form As New Element_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Get_Metaclass_Name(),
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description)
        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Update_Views()
        End If
    End Sub

    Public Sub Remove()
        Dim remove_dialog_result As MsgBoxResult
        remove_dialog_result = MsgBox(
            "Do you want to remove """ & Me.Name & """ and all its aggregated elements ?",
             MsgBoxStyle.OkCancel,
            "Remove element")
        If remove_dialog_result = MsgBoxResult.Ok Then
            Me.Get_Project().Remove_Element_From_Project(Me)
            Me.Remove_Me()
            Me.Owner.Children.Remove(Me)
            Me.Display_Package_Modified()
        End If
    End Sub

    Public Overridable Sub View()
        Dim view_form As New Element_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Get_Metaclass_Name(),
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description)
        view_form.ShowDialog()
    End Sub



    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Function Get_SVG_Id() As String
        Return Me.Identifier.ToString()
    End Function

    Protected Function Get_SVG_Def_Group_Header() As String
        Return "  <defs>" & vbCrLf &
              "  <g id=""" & Me.Get_SVG_Id & """>" & vbCrLf
    End Function

    Protected Shared Function Get_SVG_Def_Group_Footer() As String
        Return "  </g>" & vbCrLf & "  </defs>" & vbCrLf
    End Function

    Public Overridable Function Get_SVG_File_Path() As String
        Return Me.Get_Top_Package_Folder() & Path.DirectorySeparatorChar & Me.Get_SVG_Id() & ".svg"
    End Function

    Public Overridable Function Update_SVG_Diagram() As String

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
        file_stream.WriteLine("  <use xlink:href=""#" &
                              Me.Get_SVG_Id() &
                              """ transform=""translate(0,0)"" />")
        file_stream.WriteLine("</svg>")
        file_stream.Close()
        Return svg_file_full_path

    End Function

    Public Overridable Function Compute_SVG_Content() As String
        Me.SVG_Width = Get_Box_Width(SVG_MIN_CHAR_PER_LINE)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(0, 0, Me.Name,
            "lightblue", Me.SVG_Width, Me.Get_Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            "lightblue",
            Me.SVG_Width,
            desc_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = desc_rect_height + SVG_TITLE_HEIGHT
        Return Me.SVG_Content

    End Function

    Public Function Get_SVG_Content() As String
        Return Me.SVG_Content
    End Function

    Public Function Get_SVG_Width() As Integer
        Return Me.SVG_Width
    End Function

    Public Function Get_SVG_Height() As Integer
        Return Me.SVG_Height
    End Function

    Public Sub Update_Views()
        Me.Display_Package_Modified()
        ' Refresh Diagram view
        Me.Get_Project().Update_Diagram(Me)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Check_Consistency(report As Consistency_Check_Report)
        Me.Check_Own_Consistency(report)
        For Each child In Me.Children
            child.Check_Consistency(report)
        Next
    End Sub

    Protected Overridable Sub Check_Own_Consistency(report As Consistency_Check_Report)

        Dim name_check As New Consistency_Check_Report_Item(Me, Software_Element.Name_Pattern_Rule)
        report.Add_Item(name_check)
        name_check.Set_Compliance(Software_Element.Is_Symbol_Valid(Me.Name))

        Dim brother_check As New Consistency_Check_Report_Item(Me, Software_Element.Brother_Rule)
        report.Add_Item(brother_check)
        Dim brothers_name As List(Of String) = Me.Get_Forbidden_Name_List()
        brother_check.Set_Compliance(Not brothers_name.Contains(Me.Name))

    End Sub

End Class


Public MustInherit Class Must_Describe_Software_Element
    Inherits Software_Element

    Private Shared ReadOnly Desc_Rule As New Modeling_Rule(
        "Description_Mandatory",
        "Description is mandatory.")


    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New()
    End Sub

    Public Sub New(
        name As String,
        description As String,
        owner As Software_Element,
        parent_node As TreeNode)
        MyBase.New(name, description, owner, parent_node)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim desc_check As New _
            Consistency_Check_Report_Item(Me, Must_Describe_Software_Element.Desc_Rule)
        report.Add_Item(desc_check)
        desc_check.Set_Compliance(Me.Description <> "")
    End Sub

End Class


Public MustInherit Class Software_Element_Wih_Reference
    Inherits Must_Describe_Software_Element

    Public Element_Ref As Guid


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
            element_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Element_Ref = element_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        Dim edit_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Get_Metaclass_Name(),
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Referenceable_Element_Kind(),
            Me.Get_Referenced_Element_Path(),
            Me.Get_Referenceable_Element_List())

        Dim edition_form_result As DialogResult = edit_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Update Me
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Element_Ref = edit_form.Get_Ref_Element_Identifier()

            Me.Update_Views()
        End If

    End Sub

    Public Overrides Sub View()
        Dim elmt_view_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Get_Metaclass_Name(),
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Referenceable_Element_Kind(),
            Me.Get_Referenced_Element_Path(),
            Nothing)
        elmt_view_form.ShowDialog()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'


    '----------------------------------------------------------------------------------------------'
    ' Specific methods
    '----------------------------------------------------------------------------------------------'

    Public Function Get_Referenced_Element_Name() As String
        Return Get_Elmt_Name_From_Proj_By_Id(Me.Element_Ref)
    End Function

    Public Function Get_Referenced_Element_Path() As String
        Return Get_Elmt_Path_From_Proj_By_Id(Me.Element_Ref)
    End Function

    Protected MustOverride Function Get_Referenceable_Element_List() As List(Of Software_Element)

    Protected MustOverride Function Get_Referenceable_Element_Kind() As String

End Class


Public MustInherit Class Software_Element_With_Type_Reference
    Inherits Software_Element_Wih_Reference

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
            element_ref As Guid)
        MyBase.New(name, description, owner, parent_node, element_ref)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element_Wih_Reference
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Function Get_Referenceable_Element_List() As List(Of Software_Element)
        Return Me.Get_All_Types_From_Project()
    End Function

    Protected Overrides Function Get_Referenceable_Element_Kind() As String
        Return "Type"
    End Function

End Class