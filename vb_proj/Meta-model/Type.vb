Imports System.IO
Imports System.Text.RegularExpressions


Public MustInherit Class Type
    Inherits Must_Describe_Software_Element

    Public Shared ReadOnly SVG_COLOR As String = "rgb(136,0,21)"

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
        Return Path.GetTempPath() & Me.Name & ".svg"
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        ' Title (Name + stereotype)
        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name, Type.SVG_COLOR, "basic_type")

        ' Description compartment
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR)

        Return svg_content

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

    Private Shared ReadOnly Multiplicity_Rule As New Modeling_Rule(
        "Array_Multiplicity",
        "Multiplicity shall be strictly greater than 1.")
    Private Shared ReadOnly Base_Type_Rule As New Modeling_Rule(
        "Base_Type_Defined",
        "Shall reference a base Data_Type.")
    Private Shared ReadOnly Base_Not_Self_Rule As New Modeling_Rule(
        "Array_Base_Not_Self",
        "Shall not reference itself.")


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
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            forbidden_name_list,
            current_referenced_type_path,
            type_by_path_dict.Keys.ToList(),
            Me.Multiplicity.ToString())
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Get the type referenced by the array
            Dim new_referenced_type As Software_Element
            new_referenced_type = type_by_path_dict(edition_form.Get_Ref_Rerenced_Element_Path())

            ' Update the array type
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Multiplicity = CUInt(edition_form.Get_Multiplicity())
            Me.Base_Type_Ref = new_referenced_type.Identifier

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
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
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

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path
        Dim referenced_type_name As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            referenced_type_name = type_by_uuid_dict(Me.Base_Type_Ref).Name
        End If

        Dim attr_lines As New List(Of String) From {
            "Multiplicity : " & Me.Multiplicity,
            "Base : " & referenced_type_name}

        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR)

        Return svg_content
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim multiplicity_check =
            New Consistency_Check_Report_Item(Me, Array_Type.Multiplicity_Rule)
        report.Add_Item(multiplicity_check)
        multiplicity_check.Set_Compliance(Me.Multiplicity > 1)

        Dim base_type_check =
            New Consistency_Check_Report_Item(Me, Array_Type.Base_Type_Rule)
        report.Add_Item(base_type_check)
        base_type_check.Set_Compliance(Me.Base_Type_Ref <> Guid.Empty)

        Dim base_not_self_check =
            New Consistency_Check_Report_Item(Me, Array_Type.Base_Not_Self_Rule)
        report.Add_Item(base_not_self_check)
        base_not_self_check.Set_Compliance(Me.Base_Type_Ref <> Me.Identifier)

    End Sub

End Class


Public Class Enumerated_Type
    Inherits Type

    Public Enumerals As New List(Of Enumeral)

    Public Shared ReadOnly Metaclass_Name As String = "Enumerated_Type"

    Private Shared ReadOnly Nb_Enumerals As New Modeling_Rule(
        "Number_Of_Enumerals",
        "Shall aggregate at least two Enumerals.")

    Private Shared ReadOnly Unique_Enumeral_Name As New Modeling_Rule(
        "Unique_Enumeral_Name",
        "Name of Enulerals shall be unique.")


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
            Me.Identifier.ToString(),
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
            Me.Identifier.ToString(),
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim nb_enum_check As New _
            Consistency_Check_Report_Item(Me, Enumerated_Type.Nb_Enumerals)
        report.Add_Item(nb_enum_check)
        nb_enum_check.Set_Compliance(Me.Enumerals.Count >= 2)

        Dim enum_name_check As New _
            Consistency_Check_Report_Item(Me, Enumerated_Type.Unique_Enumeral_Name)
        report.Add_Item(enum_name_check)
        Dim is_compliant As Boolean = True
        Dim enumeral_name_list As New List(Of String)
        For Each enumeral In Me.Enumerals
            If Not enumeral_name_list.Contains(enumeral.Name) Then
                enumeral_name_list.Add(enumeral.Name)
            Else
                is_compliant = False
                Exit For
            End If
        Next
        enum_name_check.Set_Compliance(is_compliant)

    End Sub

End Class


Public Class Fixed_Point_Type
    Inherits Type

    Public Base_Type_Ref As Guid

    Public Unit As String
    Public Resolution As String
    Public Offset As String

    Public Shared ReadOnly Metaclass_Name As String = "Fixed_Point_Type"

    Private Shared ReadOnly Zero_Decimal_Regex_Str As String = "^0+[,|.]?0*$"
    Private Shared ReadOnly Valid_Decimal_Regex_Str As String = "\d+[.|,]?\d*"
    Private Shared ReadOnly Valid_FPT_Attr_Regex As New _
        Regex("^(?<Num>" & Valid_Decimal_Regex_Str &
            ")(\/(?<Den>" & Valid_Decimal_Regex_Str & "))?$")

    Private Shared ReadOnly Unit_Rule As New Modeling_Rule(
        "Unit_Mandatory",
        "Unit shall be set.")
    Private Shared ReadOnly Base_Type_Rule As New Modeling_Rule(
        "Base_Type_Is_Integer",
        "Referenced type shall be a Basic_Integer_Type.")
    Private Shared ReadOnly Resol_Positive_Dec As New Modeling_Rule(
        "Resolution_Strictly_Positive",
        "Resolution shall be a positive decimal value.")
    Private Shared ReadOnly Offset_Dec As New Modeling_Rule(
        "Offset_Decimal",
        "Offset shall be a decimal value.")


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
            base_type_ref As Guid,
            Unit As String,
            resolution As String,
            offset As String)
        MyBase.New(name, description, owner, parent_node)
        Me.Base_Type_Ref = base_type_ref
        Me.Unit = Unit
        Me.Resolution = resolution
        Me.Offset = offset
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Specific methods 
    ' -------------------------------------------------------------------------------------------- '

    Private Function Get_Referenced_Type(get_full_path As Boolean) As String
        ' Build the list of possible referenced type
        Dim basic_int_list As List(Of Type) = Get_Basic_Integer_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(basic_int_list)

        ' Get referenced type path or name
        Dim result As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            If get_full_path = True Then
                result = type_by_uuid_dict(Me.Base_Type_Ref).Get_Path()
            Else
                result = type_by_uuid_dict(Me.Base_Type_Ref).Name
            End If
        End If

        Return result
    End Function

    Public Shared Function Is_Resolution_Valid(resolution_str As String) As Boolean
        Dim result As Boolean = False
        Dim regex_match As Match
        regex_match = Fixed_Point_Type.Valid_FPT_Attr_Regex.Match(resolution_str)
        If regex_match.Success = True Then
            Dim num_str As String = regex_match.Groups.Item("Num").Value
            If Not Regex.IsMatch(num_str, Fixed_Point_Type.Zero_Decimal_Regex_Str) Then
                result = True
            End If
            If Not IsNothing(regex_match.Groups.Item("Den")) Then
                Dim den_str As String = regex_match.Groups.Item("Den").Value
                If Regex.IsMatch(den_str, Fixed_Point_Type.Zero_Decimal_Regex_Str) Then
                    result = False
                End If
            End If
        End If
        Return result
    End Function

    Public Shared Function Is_Offset_Valid(offset_str As String) As Boolean
        Dim result As Boolean = False
        Dim regex_match As Match
        regex_match = Fixed_Point_Type.Valid_FPT_Attr_Regex.Match(offset_str)
        If regex_match.Success = True Then
            result = True
            If Not IsNothing(regex_match.Groups.Item("Den")) Then
                Dim den_str As String = regex_match.Groups.Item("Den").Value
                If Regex.IsMatch(den_str, Fixed_Point_Type.Zero_Decimal_Regex_Str) Then
                    result = False
                End If
            End If
        End If
        Return result
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_Metaclass_Name() As String
        Return Fixed_Point_Type.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        ' Build the list of possible referenced type
        Dim basic_int_list As List(Of Type) = Get_Basic_Integer_Type_List_From_Project()
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(basic_int_list)
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(basic_int_list)

        Dim current_referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            current_referenced_type_path = type_by_uuid_dict(Me.Base_Type_Ref).Get_Path()
        End If

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)

        Dim edition_form As New Fixed_Point_Type_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Fixed_Point_Type.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            forbidden_name_list,
            current_referenced_type_path,
            type_by_path_dict.Keys.ToList(),
            Me.Unit,
            Me.Resolution,
            Me.Offset)

        Dim edition_form_result As DialogResult = edition_form.ShowDialog()
        If edition_form_result = DialogResult.OK Then

            ' Update the fixed point type
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Dim new_referenced_type As Software_Element
            new_referenced_type = type_by_path_dict(edition_form.Get_Ref_Rerenced_Element_Path())
            Me.Base_Type_Ref = new_referenced_type.Identifier
            Me.Unit = edition_form.Get_Unit()
            Me.Resolution = edition_form.Get_Resolution()
            Me.Offset = edition_form.Get_Offset()

            Me.Display_Package_Modified()

        End If
    End Sub

    Public Overrides Sub View()

        Dim view_form As New Fixed_Point_Type_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Fixed_Point_Type.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
            Get_Referenced_Type(True),
            Nothing, ' Useless for View
            Me.Unit,
            Me.Resolution,
            Me.Offset)
        view_form.ShowDialog()

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name, Type.SVG_COLOR, "fixed_point")

        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            desc_rect_height)


        Dim attr_lines As New List(Of String) From {
            "Base : " & Get_Referenced_Type(False),
            "Unit : " & Me.Unit,
            "Resolution : " & Me.Resolution,
            "Offset : " & Me.Offset}
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR)

        Return svg_content
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim unit_check = New Consistency_Check_Report_Item(Me, Fixed_Point_Type.Unit_Rule)
        report.Add_Item(unit_check)
        unit_check.Set_Compliance(Me.Unit <> "")

        Dim base_type_check = New Consistency_Check_Report_Item(Me, Fixed_Point_Type.Base_Type_Rule)
        report.Add_Item(base_type_check)
        Dim basic_int_list As List(Of Type) = Get_Basic_Integer_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(basic_int_list)
        base_type_check.Set_Compliance(type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref))

        Dim resol_check = New Consistency_Check_Report_Item(Me, Fixed_Point_Type.Resol_Positive_Dec)
        report.Add_Item(resol_check)
        resol_check.Set_Compliance(Fixed_Point_Type.Is_Resolution_Valid(Me.Resolution))

        Dim offset_check = New Consistency_Check_Report_Item(Me, Fixed_Point_Type.Offset_Dec)
        report.Add_Item(offset_check)
        offset_check.Set_Compliance(Fixed_Point_Type.Is_Offset_Valid(Me.Offset))

    End Sub

End Class


Public Class Record_Type
    Inherits Type

    Public Fields As New List(Of Record_Field)

    Public Shared ReadOnly Metaclass_Name As String = "Record_Type"

    Private Shared ReadOnly Context_Menu As New Record_Type_Context_Menu()

    Private Shared ReadOnly Nb_Fields_Rule As New Modeling_Rule(
        "Number_Of_Fields",
        "Shall aggregate at least two Fields.")


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

    Protected Overrides Sub Create_Node()
        MyBase.Create_Node()
        Me.Node.ContextMenuStrip = Record_Type.Context_Menu
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Record_Type.Metaclass_Name
    End Function

    Protected Overrides Function Get_Children() As List(Of Software_Element)
        If Me.Children_Is_Computed = False Then
            Me.Children_Is_Computed = True
            Me.Children.AddRange(Me.Fields)
        End If
        Return Me.Children
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Field()
        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        type_list.Remove(Me)
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(type_list)

        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Record_Field.Metaclass_Name,
            "",
            Record_Field.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Type",
            type_by_path_dict.Keys(0),
            type_by_path_dict.Keys.ToList())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            ' Get the type referenced by the field
            Dim ref_type As Software_Element
            ref_type = type_by_path_dict(creation_form.Get_Ref_Rerenced_Element_Path())

            Dim new_field As New Record_Field(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                ref_type.Identifier)
            Me.Fields.Add(new_field)
            Me.Children.Add(new_field)
            Me.Display_Package_Modified()
        End If
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        ' Title (Name + stereotype)
        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name, Type.SVG_COLOR, "record")

        ' Description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            desc_rect_height)

        ' Fields compartment
        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path


        Dim fields_lines As New List(Of String)
        For Each field In Me.Fields
            Dim referenced_type_name As String = "unresolved"
            If type_by_uuid_dict.ContainsKey(field.Type_Ref) Then
                referenced_type_name = type_by_uuid_dict(field.Type_Ref).Name
            End If
            fields_lines.Add("+ " & field.Name & " : " & referenced_type_name)
        Next
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            fields_lines,
            Type.SVG_COLOR)

        Return svg_content

    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim nb_fields_check As New Consistency_Check_Report_Item(Me, Record_Type.Nb_Fields_Rule)
        report.Add_Item(nb_fields_check)
        nb_fields_check.Set_Compliance(Me.Fields.Count >= 2)

    End Sub

End Class


Public Class Record_Field

    Inherits Software_Element

    Public Type_Ref As Guid

    Public Shared ReadOnly Metaclass_Name As String = "Record_Field"

    Private Shared ReadOnly Base_Type_Rule As New Modeling_Rule(
        "Record_Field_Type_Ref",
        "Shall reference a Type.")

    Private Shared ReadOnly Type_Ref_Not_Owner_Rule As New Modeling_Rule(
        "Record_Field_Type_Ref_Not_Owner",
        "Shall not reference its owner Record_Type.")


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
            base_type_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Type_Ref = base_type_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Record_Type).Fields.Remove(Me)
        CType(new_parent, Record_Type).Fields.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_record_type As Record_Type = CType(Me.Owner, Record_Type)
        Me.Node.Remove()
        parent_record_type.Fields.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Record_Field.Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return (parent.GetType() = GetType(Record_Type))
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        type_list.Remove(CType(Me.Owner, Type))
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(type_list)
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        Dim current_referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Type_Ref) Then
            current_referenced_type_path = type_by_uuid_dict(Me.Type_Ref).Get_Path()
        End If

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)

        Dim edition_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Record_Field.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            forbidden_name_list,
            "Type",
            current_referenced_type_path,
            type_by_path_dict.Keys.ToList())
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Get the type referenced by the field
            Dim new_referenced_type As Software_Element
            new_referenced_type = type_by_path_dict(edition_form.Get_Ref_Rerenced_Element_Path())

            ' Update the array type
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Type_Ref = new_referenced_type.Identifier

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
        If type_by_uuid_dict.ContainsKey(Me.Type_Ref) Then
            referenced_type_path = type_by_uuid_dict(Me.Type_Ref).Get_Path()
        End If

        Dim elmt_view_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Record_Field.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
            "Type",
            referenced_type_path,
            Nothing)
        elmt_view_form.ShowDialog()

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name, Type.SVG_COLOR, "field")

        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            desc_rect_height)

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path
        Dim referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Type_Ref) Then
            referenced_type_path = type_by_uuid_dict(Me.Type_Ref).Get_Path()
        End If

        Dim attr_lines As New List(Of String) From {
            "Base : " & referenced_type_path}

        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR)

        Return svg_content
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim type_ref_check = New Consistency_Check_Report_Item(Me, Record_Field.Base_Type_Rule)
        report.Add_Item(type_ref_check)
        type_ref_check.Set_Compliance(Me.Type_Ref <> Guid.Empty)

        Dim type_ref_not_owner_check As New _
            Consistency_Check_Report_Item(Me, Record_Field.Type_Ref_Not_Owner_Rule)
        report.Add_Item(type_ref_not_owner_check)
        type_ref_not_owner_check.Set_Compliance(Me.Type_Ref <> Me.Owner.Identifier)

    End Sub

End Class