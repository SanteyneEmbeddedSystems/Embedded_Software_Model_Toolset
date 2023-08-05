Imports System.IO


Public MustInherit Class Type
    Inherits Software_Element

    Protected Shared SVG_COLOR As String = "rgb(136,0,21)"

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
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '
    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Dim is_allowed As Boolean = False
        If parent.GetType() = GetType(Top_Level_Package) _
            Or parent.GetType() = GetType(Package) Then
            is_allowed = True
        End If
        Return is_allowed
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Package).Types.Remove(Me)
        CType(new_parent, Package).Types.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Types.Remove(Me)
    End Sub

End Class


Public MustInherit Class Basic_Type
    Inherits Type

    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '
    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Basic_Type",
            .SelectedImageKey = "Basic_Type",
            .ContextMenuStrip = Software_Element.Read_Only_Context_Menu,
            .Tag = Me}
    End Sub

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        Throw New Exception("A Basic Type cannot be moved.")
    End Sub

    Protected Overrides Sub Remove_Me()
        Throw New Exception("A Basic Type cannot be removed.")
    End Sub

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Software_Element.Read_Only_Context_Menu
    End Function


    Public Overrides Function Get_SVG_File_Path() As String
        Dim svg_folder As String = Path.GetTempPath()
        Dim svg_file_full_path As String
        svg_file_full_path = svg_folder & Path.DirectorySeparatorChar & Me.Name & ".svg"
        Return svg_file_full_path
    End Function

End Class


Public Class Basic_Integer_Type
    Inherits Basic_Type

    Public Shared ReadOnly Metaclass_Name As String = "Basic_Integer_Type"

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Integer_Type.Metaclass_Name
    End Function

End Class


Public Class Basic_Boolean_Type
    Inherits Basic_Type

    Public Shared ReadOnly Metaclass_Name As String = "Basic_Boolean_Type"

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Boolean_Type.Metaclass_Name
    End Function

End Class


Public Class Basic_Floating_Point_Type
    Inherits Basic_Type

    Public Shared ReadOnly Metaclass_Name As String = "Basic_Floating_Point_Type"

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Floating_Point_Type.Metaclass_Name
    End Function

End Class


Public Class Array_Type
    Inherits Type

    Public Multiplicity As UInteger
    Public Base_Type_Ref As Guid

    Public Shared ReadOnly Metaclass_Name As String = "Array_Type"

    Public Shared ReadOnly Multiplicity_Minimum_Value As UInteger = 2

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
            multiplicity As UInteger,
            base_type_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Multiplicity = multiplicity
        Me.Base_Type_Ref = base_type_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Array_Type",
            .SelectedImageKey = "Array_Type",
            .ContextMenuStrip = Software_Element.Leaf_Context_Menu,
            .Tag = Me}
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Array_Type.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        type_list.Remove(Me)
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(type_list)
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        Dim current_referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            current_referenced_type_path = type_by_uuid_dict(Me.Base_Type_Ref).Get_Path()
        End If

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)

        Dim edition_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Array_Type.Metaclass_Name,
            Me.UUID.ToString,
            Me.Name,
            Me.Description,
            forbidden_name_list,
            "Base Type",
            current_referenced_type_path,
            type_by_path_dict.Keys.ToList(),
            Me.Multiplicity.ToString())
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Get the type referenced by the array
            Dim new_referenced_type As Software_Element = Nothing
            new_referenced_type = type_by_path_dict(edition_form.Get_Ref_Rerenced_Element_Path())

            ' Update the array type
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Multiplicity = CUInt(edition_form.Get_Multiplicity())
            Me.Base_Type_Ref = new_referenced_type.UUID

            Me.Display_Package_Modified()
        End If

    End Sub

    Public Overrides Sub View()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path
        Dim referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            referenced_type_path = type_by_uuid_dict(Me.Base_Type_Ref).Get_Path()
        End If

        Dim elmt_view_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Array_Type.Metaclass_Name,
            Me.UUID.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
            "Base Type",
            referenced_type_path,
            Nothing, ' Useless for View
            Me.Multiplicity.ToString())
        elmt_view_form.ShowDialog()

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name, Type.SVG_COLOR, "array")

        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            desc_rect_height)

        Dim attr_lines As New List(Of String)
        attr_lines.Add("Multiplicity : " & Me.Multiplicity)

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path
        Dim referenced_type_name As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            referenced_type_name = type_by_uuid_dict(Me.Base_Type_Ref).Name
        End If
        attr_lines.Add("Base : " & referenced_type_name)

        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR)

        Return svg_content
    End Function

End Class


Public Class Enumerated_Type
    Inherits Type

    Public Enumerals As New List(Of Enumeral)

    Public Shared ReadOnly Metaclass_Name As String = "Enumerated_Type"

    Public Class Enumeral
        Public Name As String
        Public Description As String

        Public Sub New()
        End Sub

        Public Sub New(name As String, description As String)
            Me.Name = name
            Me.Description = description
        End Sub

    End Class


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
            enumerals_table As DataTable)
        MyBase.New(name, description, owner, parent_node)
        Update_Enumerals(enumerals_table)
    End Sub

    Private Sub Update_Enumerals(enumerals_table As DataTable)
        Me.Enumerals.Clear()
        Dim row As DataRow
        For Each row In enumerals_table.Rows
            Dim enumeral_name As String
            If Not IsDBNull(row("Name")) Then
                enumeral_name = CStr(row("Name"))
            Else
                enumeral_name = ""
            End If
            Dim enumeral_description As String
            If Not IsDBNull(row("Description")) Then
                enumeral_description = CStr(row("Description"))
            Else
                enumeral_description = ""
            End If
            Me.Enumerals.Add(New Enumeral(enumeral_name, enumeral_description))
        Next
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Enumerated_Type",
            .SelectedImageKey = "Enumerated_Type",
            .ContextMenuStrip = Software_Element.Leaf_Context_Menu,
            .Tag = Me}
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Enumerated_Type.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)

        Dim enumerals_table As New DataTable
        With enumerals_table
            .Columns.Add("Name", GetType(String))
            .Columns.Add("Description", GetType(String))
        End With
        For Each enumeral In Me.Enumerals
            enumerals_table.Rows.Add(enumeral.Name, enumeral.Description)
        Next

        Dim edit_form As New Enumerated_Type_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Get_Metaclass_Name(),
            Me.UUID.ToString(),
            Me.Name,
            Me.Description,
            forbidden_name_list,
            enumerals_table)

        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Update_Enumerals(enumerals_table)
            Me.Display_Package_Modified()
        End If

    End Sub

    Public Overrides Sub View()

        Dim enumerals_table As New DataTable
        With enumerals_table
            .Columns.Add("Name", GetType(String))
            .Columns.Add("Description", GetType(String))
        End With
        For Each enumeral In Me.Enumerals
            enumerals_table.Rows.Add(enumeral.Name, enumeral.Description)
        Next

        Dim view_form As New Enumerated_Type_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Get_Metaclass_Name(),
            Me.UUID.ToString(),
            Me.Name,
            Me.Description,
            Nothing, ' forbidden name list
            enumerals_table)
        view_form.ShowDialog()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        ' Title (Name + stereotype)
        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name, Type.SVG_COLOR, "enumeration")

        ' Description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            desc_rect_height)

        ' Enumerals compartment
        Dim enumeral_lines As New List(Of String)
        For Each enumeral In Me.Enumerals
            enumeral_lines.Add(enumeral.Name)
        Next
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            enumeral_lines,
            Type.SVG_COLOR)

        Return svg_content

    End Function


End Class