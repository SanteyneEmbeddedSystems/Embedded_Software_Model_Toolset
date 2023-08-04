Imports System.Text.RegularExpressions
Imports System.IO

Public MustInherit Class Software_Element

    Public Name As String
    Public UUID As Guid
    Public Description As String

    Protected Node As TreeNode

    Protected Owner As Software_Element = Nothing
    Protected Children As New List(Of Software_Element)
    Protected Children_Is_Computed As Boolean = False

    Public Shared NB_CHARS_MAX_FOR_SYMBOL As Integer = 32
    Protected Shared Valid_Symbol_Regex As String =
        "^[a-zA-Z][a-zA-Z0-9_]{1," & NB_CHARS_MAX_FOR_SYMBOL - 1 & "}$"

    Protected Shared Read_Only_Context_Menu As New Read_Only_Context_Menu
    Protected Shared Leaf_Context_Menu As New Leaf_Context_Menu


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
        Me.UUID = Guid.NewGuid()
        Me.Owner = owner
        Me.Create_Node()
        parent_node.Nodes.Add(Me.Node)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Shared
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Is_Symbol_Valid(symbol As String) As Boolean
        Dim result As Boolean = False
        If Regex.IsMatch(symbol, Software_Element.Valid_Symbol_Regex) Then
            result = True
        End If
        Return result
    End Function

    Public Shared Function Get_Default_Description() As String
        Return "A good description is always useful."
    End Function

    Public Shared Function Create_Path_Dictionary_From_List(
            elmt_list As IEnumerable(Of Software_Element)) As Dictionary(Of String, Software_Element)
        Dim dico As New Dictionary(Of String, Software_Element)
        For Each elmt In elmt_list
            dico.Add(elmt.Get_Path(), elmt)
        Next
        Return dico
    End Function

    Public Shared Function Create_UUID_Dictionary_From_List(
            elmt_list As IEnumerable(Of Software_Element)) As Dictionary(Of Guid, Software_Element)
        Dim dico As New Dictionary(Of Guid, Software_Element)
        For Each elmt In elmt_list
            dico.Add(elmt.UUID, elmt)
        Next
        Return dico
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Generic methods
    ' -------------------------------------------------------------------------------------------- '

    Protected Overridable Function Get_Children() As List(Of Software_Element)
        Return Nothing
    End Function

    Protected MustOverride Sub Create_Node()

    Protected Sub Post_Treat_After_Deserialization(parent_node As TreeNode)
        Me.Create_Node()
        parent_node.Nodes.Add(Me.Node)
        If Not Me.Get_Top_Package().Is_Writable() Then
            Me.Node.ContextMenuStrip = Software_Element.Read_Only_Context_Menu
        End If
        Dim children As List(Of Software_Element) = Me.Get_Children()
        If Not IsNothing(children) Then
            For Each child In children
                child.Owner = Me
                child.Post_Treat_After_Deserialization(Me.Node)
            Next
        End If
    End Sub

    Public Function Get_Top_Package() As Top_Level_Package
        Dim top_pkg As Top_Level_Package = Nothing
        Dim current_elmt As Software_Element = Me
        Dim parent As Software_Element = current_elmt.Owner
        If Not IsNothing(parent) Then ' parent = Nothing when Me is Software_Project
            If IsNothing(parent.Owner) Then
                top_pkg = CType(current_elmt, Top_Level_Package)
            Else
                While Not IsNothing(parent.Owner)
                    current_elmt = parent
                    parent = current_elmt.Owner
                End While
                top_pkg = CType(current_elmt, Top_Level_Package)
            End If
        End If
        Return top_pkg
    End Function

    Public Sub Display_Package_Modified()
        Dim owner_pkg As Top_Level_Package = Me.Get_Top_Package()
        owner_pkg.Display_Modified()
    End Sub

    Public Function Get_Children_Name() As List(Of String)
        Dim children_name As New List(Of String)
        For Each child In Me.Get_Children
            children_name.Add(child.Name)
        Next
        Return children_name
    End Function

    Public MustOverride Function Is_Allowed_Parent(parent As Software_Element) As Boolean

    Public Sub Move(new_parent As Software_Element)
        ' Manage top level packages
        Me.Display_Package_Modified()
        new_parent.Display_Package_Modified()

        ' Update model
        Me.Move_Me(new_parent)
        Me.Owner.Children.Remove(Me)
        Me.Owner = new_parent
        new_parent.Children.Add(Me)

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

    Protected Overridable Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Software_Element.Leaf_Context_Menu
    End Function

    Protected MustOverride Sub Move_Me(new_parent As Software_Element)

    Protected MustOverride Sub Remove_Me()

    Public Function Get_Path() As String
        Dim my_path As String = Me.Get_Path_Separator() & Me.Name
        Dim parent As Software_Element = Me.Owner
        While Not IsNothing(parent.Owner)
            my_path = parent.Get_Path_Separator() & parent.Name & my_path
            parent = parent.Owner
        End While
        Return my_path
    End Function

    Protected Overridable Function Get_Path_Separator() As String
        Return "::"
    End Function

    Protected Function Get_Project() As Software_Project
        Dim current_element As Software_Element = Me
        While Not IsNothing(current_element.Owner)
            current_element = current_element.Owner
        End While
        Return CType(current_element, Software_Project)
    End Function

    Protected Function Get_Type_List_From_Project() As List(Of Type)
        Return Get_Project().Get_Type_List()
    End Function

    Public MustOverride Function Get_Metaclass_Name() As String

    Protected Function Get_Top_Package_Folder() As String
        Dim top_pkg As Top_Level_Package = Me.Get_Top_Package()
        Return top_pkg.Get_Folder()
    End Function



    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overridable Sub Edit()

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)

        Dim edit_form As New Element_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Get_Metaclass_Name(),
            Me.UUID.ToString(),
            Me.Name,
            Me.Description,
            forbidden_name_list)

        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Display_Package_Modified()
        End If

    End Sub

    Public Sub Remove()
        Dim remove_dialog_result As MsgBoxResult
        remove_dialog_result = MsgBox(
            "Do you want to remove """ & Me.Name & """ and all its aggregated elements ?",
             MsgBoxStyle.OkCancel,
            "Remove element")
        If remove_dialog_result = MsgBoxResult.Ok Then
            Me.Remove_Me()
            Me.Owner.Children.Remove(Me)
            Me.Display_Package_Modified()
        End If
    End Sub

    Public Overridable Sub View()
        Dim view_form As New Element_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Get_Metaclass_Name(),
            Me.UUID.ToString(),
            Me.Name,
            Me.Description,
            Nothing) ' forbidden name list
        view_form.ShowDialog()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overridable Function Get_SVG_File_Path() As String
        Return Me.Get_Top_Package_Folder() & Path.DirectorySeparatorChar & Me.Name & ".svg"
    End Function

    Public Overridable Function Update_SVG_Diagram() As String

        Dim svg_file_full_path As String = Me.Get_SVG_File_Path()
        Dim file_stream As New StreamWriter(svg_file_full_path, False)

        file_stream.WriteLine("<?xml version=""1.0"" encoding=""UTF-8""?>")
        file_stream.WriteLine()
        file_stream.WriteLine("<svg")
        file_stream.WriteLine("  Version=""1.1""")
        file_stream.WriteLine("  xmlns=""http://www.w3.org/2000/svg""")
        file_stream.WriteLine("  xmlns:svg=""http://www.w3.org/2000/svg"">")
        file_stream.WriteLine("  <style>text{font-size:" & SVG.SVG_FONT_SIZE &
         "px;font-family:Consolas;fill:black;text-anchor:start;}</style>")
        file_stream.WriteLine(Me.Get_SVG_Content(10, 10))
        file_stream.WriteLine("</svg>")

        file_stream.Close()

        Return svg_file_full_path

    End Function

    Public Overridable Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String
        svg_content = "  <text x=""" & x_pos & "px"" y=""" & y_pos & "px"">" & Me.Name & "</text>" &
            "<text x=""" & x_pos & "px"" y=""" & y_pos + 20 & "px"">" & Me.Description & "</text>"
        Return svg_content
    End Function


End Class