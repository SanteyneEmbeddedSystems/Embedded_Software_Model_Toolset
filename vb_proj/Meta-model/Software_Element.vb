Imports System.Text.RegularExpressions
Imports System.IO

Public MustInherit Class Software_Element

    Public Name As String
    Public Identifier As Guid
    Public Description As String

    Protected Node As TreeNode

    Protected Owner As Software_Element = Nothing
    Protected Children As New List(Of Software_Element)

    Protected Shared ReadOnly Read_Only_Context_Menu As New Read_Only_Context_Menu
    Private Shared ReadOnly Leaf_Context_Menu As New Leaf_Context_Menu

    Public Const NB_CHARS_MAX_FOR_SYMBOL As Integer = 32

    Private Shared ReadOnly Brother_Rule As New Modeling_Rule(
        "Brother_Name",
        "Elements aggregated by the same owner shall have a different name.")

    Protected SVG_Content As String = ""
    Protected SVG_Width As Integer = 0
    Protected SVG_Height As Integer = 0
    Protected Alt_SVG_Width As Integer = 0
    Protected Alt_SVG_Height As Integer = 0


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
    ' Generic methods
    ' -------------------------------------------------------------------------------------------- '

    Public MustOverride Function Get_Metaclass_Name() As String

    ' Shall be overridden by all non-leaf software elements
    Protected Overridable Function Compute_Children_For_Post_Treat() As List(Of Software_Element)
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
        Dim children As List(Of Software_Element) = Me.Compute_Children_For_Post_Treat()
        If Not IsNothing(children) Then
            For Each child In children
                child.Owner = Me
                child.Post_Treat_After_Deserialization(Me.Node)
            Next
        End If
    End Sub

    Public Function Get_Children_Name() As List(Of String)
        Dim children_name As New List(Of String)
        Dim children As List(Of Software_Element) = Me.Children
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

    Protected Overridable Function Get_Read_Only_Context_Menu() As ContextMenuStrip
        Return Software_Element.Read_Only_Context_Menu
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
        Me.Node.ContextMenuStrip = Get_Read_Only_Context_Menu()
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

    Protected Function Get_SVG_Def_Group_Header(Optional is_alt As Boolean = False) As String
        If is_alt = False Then
            Return "  <defs>" & vbCrLf & "  <g id=""" & Me.Get_SVG_Id & """>" & vbCrLf
        Else
            Return "  <defs>" & vbCrLf & "  <g id=""" & Me.Get_SVG_Id & "_alt"">" & vbCrLf
        End If
    End Function

    Protected Shared Function Get_SVG_Def_Group_Footer() As String
        Return "  </g>" & vbCrLf & "  </defs>" & vbCrLf
    End Function

    Public Function Get_SVG_File_Path() As String
        Return Path.GetTempPath() & Me.Get_SVG_Id() & ".svg"
    End Function

    Public Function Get_Alternative_SVG_File_Path() As String
        Return Path.GetTempPath() & Me.Get_SVG_Id() & "_alt.svg"
    End Function

    Public Overridable Function Create_SVG_File() As String
        Dim svg_file_full_path As String = Me.Get_SVG_File_Path()
        Dim file_stream As New StreamWriter(svg_file_full_path, False)
        Dim def_group As String = Me.Get_SVG_Def_Group()
        file_stream.WriteLine(
            Get_SGV_File_Header(
                Me.SVG_Height + 2 * SVG_BOX_MARGIN,
                Me.SVG_Width + 2 * SVG_BOX_MARGIN))
        file_stream.WriteLine(def_group)
        file_stream.WriteLine("  <use xlink:href=""#" & Me.Get_SVG_Id() &
            """ transform=""translate(" & SVG_BOX_MARGIN & "," & SVG_BOX_MARGIN & ")"" />")
        file_stream.WriteLine("</svg>")
        file_stream.Close()
        Return svg_file_full_path
    End Function

    Public Function Create_Alternative_SVG_File() As String
        Dim svg_file_full_path As String = Me.Get_Alternative_SVG_File_Path()
        Dim file_stream As New StreamWriter(svg_file_full_path, False)
        Dim def_group As String = Me.Get_Alternative_SVG_Def_Group()
        file_stream.WriteLine(
            Get_SGV_File_Header(
                Me.Alt_SVG_Height + 2 * SVG_BOX_MARGIN,
                Me.Alt_SVG_Width + 2 * SVG_BOX_MARGIN))
        file_stream.WriteLine(def_group)
        file_stream.WriteLine("  <use xlink:href=""#" & Me.Get_SVG_Id() & "_alt" &
            """ transform=""translate(" & SVG_BOX_MARGIN & "," & SVG_BOX_MARGIN & ")"" />")
        file_stream.WriteLine("</svg>")
        file_stream.Close()
        Return svg_file_full_path
    End Function

    Public Overridable Function Get_SVG_Def_Group() As String
        Return ""
    End Function

    Public Overridable Function Get_Alternative_SVG_Def_Group() As String
        Return ""
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
        Dim brother_check As New Consistency_Check_Report_Item(Me, Software_Element.Brother_Rule)
        report.Add_Item(brother_check)
        Dim brothers_name As List(Of String) = Me.Get_Forbidden_Name_List()
        brother_check.Set_Compliance(Not brothers_name.Contains(Me.Name))
    End Sub

End Class


Public MustInherit Class Named_Element
    Inherits Software_Element

    Protected Shared Valid_Symbol_Regex As String =
        "^[a-zA-Z][a-zA-Z0-9_]{1," & NB_CHARS_MAX_FOR_SYMBOL - 1 & "}$"

    Protected Shared ReadOnly Name_Pattern_Rule As New Modeling_Rule(
        "Name_Pattern",
        "Name shall match " & Valid_Symbol_Regex)

    Protected Const SVG_MIN_CHAR_PER_LINE As Integer = NB_CHARS_MAX_FOR_SYMBOL


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


    ' -------------------------------------------------------------------------------------------- '
    ' Shared
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Is_Symbol_Valid(symbol As String) As Boolean
        Return Regex.IsMatch(symbol, Named_Element.Valid_Symbol_Regex)
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim name_check As New Consistency_Check_Report_Item(Me, Named_Element.Name_Pattern_Rule)
        report.Add_Item(name_check)
        name_check.Set_Compliance(Named_Element.Is_Symbol_Valid(Me.Name))
    End Sub

End Class


Public MustInherit Class Described_Element
    Inherits Named_Element

    Private Shared ReadOnly Description_Mandatory_Rule As New Modeling_Rule(
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
            Consistency_Check_Report_Item(Me, Described_Element.Description_Mandatory_Rule)
        report.Add_Item(desc_check)
        desc_check.Set_Compliance(Me.Description <> "")
    End Sub

End Class


Public MustInherit Class Typed_Element
    Inherits Described_Element

    Public Referenced_Type_Id As Guid

    Private Shared ReadOnly Type_Rule As New Modeling_Rule(
        "Type",
        "Shall reference one Type.")


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
        Me.Referenced_Type_Id = element_ref
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Specific methods
    '----------------------------------------------------------------------------------------------'

    Public Function Get_Referenced_Type_Name() As String
        Return Get_Elmt_Name_From_Proj_By_Id(Me.Referenced_Type_Id)
    End Function

    Public Function Get_Referenced_Type_Path() As String
        Return Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Type_Id)
    End Function


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
            "Type",
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Type_Id),
            Me.Get_All_Types_From_Project())

        Dim edition_form_result As DialogResult = edit_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Update Me
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Referenced_Type_Id = edit_form.Get_Ref_Element_Identifier()

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
            "Type",
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Type_Id),
            Nothing)
        elmt_view_form.ShowDialog()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim type_check = New Consistency_Check_Report_Item(Me, Type_Rule)
        report.Add_Item(type_check)
        Dim referenced_type As Software_Element = Me.Get_Elmt_From_Prj_By_Id(Me.Referenced_Type_Id)
        type_check.Set_Compliance(TypeOf referenced_type Is Type)
    End Sub

End Class