Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Globalization


Public MustInherit Class Type
    Inherits Classifier

    Public Const SVG_COLOR As String = "rgb(136,0,21)"


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

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Package).Types.Remove(Me)
        CType(new_parent, Package).Types.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Types.Remove(Me)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        ' Shall be overriden by Array_Type, Record_Type and Fixed_Point_Type
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Specific methods
    ' -------------------------------------------------------------------------------------------- '

    Public MustOverride Function Is_Value_Valid(value_str As String) As Boolean


End Class


Public MustInherit Class Basic_Type
    Inherits Type

    Public Const Metaclass_Name As String = "Basic_Type"

    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Type.Metaclass_Name
    End Function

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

    Public Overrides Function Compute_SVG_Content() As String
        Me.SVG_Width = Get_Box_Width(SVG_MIN_CHAR_PER_LINE)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Title (Name + stereotype)
        Me.SVG_Content &= Get_Title_Rectangle(
            0, 0, Me.Name, Type.SVG_COLOR, Me.SVG_Width, "basic_type")

        ' Description compartment
        Dim split_description As List(Of String) = Split_String(
            Me.Description, SVG_MIN_CHAR_PER_LINE)
        Dim desc_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height ' - stroke width
        Return Me.SVG_Content

    End Function

End Class


Public Class Basic_Integer_Type
    Inherits Basic_Type

    Public Enum E_Signedness_Type
        SIGNED
        UNSIGNED
    End Enum

    Public Size As Integer ' number of bytes
    Public Signedness As E_Signedness_Type

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Dim is_valid As Boolean = False
        Select Case Me.Signedness
            Case E_Signedness_Type.SIGNED
                Dim is_int As Boolean
                Dim value_int As Int64
                is_int = Int64.TryParse(
                    value_str,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    value_int)
                If is_int = True Then
                    Select Case Me.Size
                        Case 1
                            If value_int >= -128 And value_int <= 127 Then
                                is_valid = True
                            End If
                        Case 2
                            If value_int >= -32768 And value_int <= 32767 Then
                                is_valid = True
                            End If
                        Case 4
                            If value_int >= -2147483648 And value_int <= 2147483647 Then
                                is_valid = True
                            End If
                        Case 8
                            is_valid = True
                    End Select
                End If
            Case E_Signedness_Type.UNSIGNED
                Dim is_uint As Boolean
                Dim value_uint As UInt64
                is_uint = UInt64.TryParse(
                    value_str,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    value_uint)
                If is_uint = True Then
                    Select Case Me.Size
                        Case 1
                            If value_uint <= 255 Then
                                is_valid = True
                            End If
                        Case 2
                            If value_uint <= 65535 Then
                                is_valid = True
                            End If
                        Case 4
                            If value_uint <= 4294967295 Then
                                is_valid = True
                            End If
                        Case 8
                            is_valid = True
                    End Select
                End If
        End Select
        Return is_valid
    End Function

End Class

Public Class Basic_Carrier_Type
    Inherits Basic_Type

    Public Size As Integer ' number of bytes

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Dim is_valid As Boolean = False
        Dim is_uint As Boolean
        Dim value_uint As UInt64
        is_uint = UInt64.TryParse(
                    value_str,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    value_uint)
        If is_uint = True Then
            If value_uint < Math.Pow(2, Me.Size * 8) - 1 Then
                is_valid = True
            End If
        ElseIf Regex.IsMatch(value_str, "^0x[0-9A-F]{" & Me.size * 2 & "}$") Then
            is_valid = True
        End If
        Return is_valid
    End Function

End Class

Public Class Basic_Boolean_Type
    Inherits Basic_Type

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        If Regex.IsMatch(value_str, "^(true|false)$", RegexOptions.IgnoreCase) Then
            Return True
        Else
            Return False
        End If
    End Function

End Class

Public Class Basic_Character_Type
    Inherits Basic_Type

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Return True
    End Function

End Class


Public Class Array_Type
    Inherits Type

    Public First_Dimension As Cardinality
    Public Second_Dimension As Cardinality
    Public Third_Dimension As Cardinality
    Public Base_Type_Ref As Guid

    Public Const Metaclass_Name As String = "Array_Type"

    Private Shared ReadOnly Base_Type_Rule As New Modeling_Rule(
        "Base_Type",
        "Shall reference one base Type.")
    Private Shared ReadOnly Array_Base_Not_Self_Rule As New Modeling_Rule(
        "Array_Base_Not_Self",
        "Shall not reference itself.")
    Private Shared ReadOnly First_Dimension_Used_Rule As New Modeling_Rule(
        "First_Dimension_Used",
        "First_Dimension shall not be 1.")
    Private Shared ReadOnly Third_Dimension_Used_Rule As New Modeling_Rule(
        "Third_Dimension_Used",
        "Third_Dimension can be used ony if Second_Dimension is not 1.")
    Private Shared ReadOnly Dimensions_Not_Zero_Rule As New Modeling_Rule(
        "Dimensions_Not_Zero",
        "Dimensions can not be 0 or *.")


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
            dimension_1 As Cardinality,
            dimension_2 As Cardinality,
            dimension_3 As Cardinality,
            base_type_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.First_Dimension = dimension_1
        Me.Second_Dimension = dimension_2
        Me.Third_Dimension = dimension_3
        Me.Base_Type_Ref = base_type_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_Metaclass_Name() As String
        Return Array_Type.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements.Clear()
        Dim referenced_element As Software_Element = Me.Get_Elmt_From_Prj_By_Id(Me.Base_Type_Ref)
        If Not IsNothing(referenced_element) Then
            If TypeOf referenced_element Is Type Then
                Me.Needed_Elements.Add(CType(referenced_element, Classifier))
            End If
        End If
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Type
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Dim is_valid As Boolean = True

        Dim validation_regexp_str As String = "\s*(?<data>[a-zA-Z_0-9,.]+)\s*"

        Dim min As String
        Dim max As String
        Dim occurence As String

        If Not Me.Third_Dimension.Is_One() Then
            min = Me.Third_Dimension.Get_Minimum().ToString
            If Me.Third_Dimension.Is_Max_Any() Then
                max = ""
            Else
                max = Me.Third_Dimension.Get_Maximum().ToString
            End If
            occurence = "{" & min & "," & max & "}"
            validation_regexp_str = "\s*\[(?:" & validation_regexp_str & ")" & occurence & "\s*\]"
        End If

        If Not Me.Second_Dimension.Is_One() Then
            min = Me.Second_Dimension.Get_Minimum().ToString
            If Me.Second_Dimension.Is_Max_Any() Then
                max = ""
            Else
                max = Me.Second_Dimension.Get_Maximum().ToString
            End If
            occurence = "{" & min & "," & max & "}"
            validation_regexp_str = "\s*\[(?:" & validation_regexp_str & ")" & occurence & "\s*\]"
        End If

        min = Me.First_Dimension.Get_Minimum().ToString
        If Me.First_Dimension.Is_Max_Any() Then
            max = ""
        Else
            max = Me.First_Dimension.Get_Maximum().ToString
        End If
        occurence = "{" & min & "," & max & "}"
        validation_regexp_str = "^\s*\[(?:" & validation_regexp_str & ")" & occurence & "\s*\]$"

        Dim validation_regexp As New Regex(validation_regexp_str)

        Dim regex_match As Match
        regex_match = validation_regexp.Match(value_str)

        If regex_match.Success = True Then
            Dim element_match As MatchCollection
            element_match = Regex.Matches(regex_match.Value, "([a-zA-Z_0-9,.])+")
            Dim base_type As Type = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Base_Type_Ref), Type)
            If Not IsNothing(base_type) Then
                For idx = 0 To element_match.Count - 1
                    If Not base_type.Is_Value_Valid(element_match.Item(idx).Value) Then
                        is_valid = False
                        Exit For
                    End If
                Next
            Else
                is_valid = False
            End If
        Else
            is_valid = False
        End If

        Return is_valid
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Software_Element) = Me.Get_All_Types_From_Project()
        type_list.Remove(Me)

        Dim edition_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Base_Type_Ref),
            type_list,
            Me.First_Dimension.Get_Value(),
            Me.Second_Dimension.Get_Value(),
            Me.Third_Dimension.Get_Value())
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then
            ' Update the array type
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.First_Dimension = New Cardinality(edition_form.Get_First_Dimension())
            Me.Second_Dimension = New Cardinality(edition_form.Get_Second_Dimension())
            Me.Third_Dimension = New Cardinality(edition_form.Get_Third_Dimension())
            Me.Base_Type_Ref = edition_form.Get_Ref_Element_Identifier()
            Me.Update_Views()
        End If

    End Sub

    Public Overrides Sub View()
        Dim elmt_view_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Base_Type_Ref),
            Nothing, ' Useless for View
            Me.First_Dimension.Get_Value(),
            Me.Second_Dimension.Get_Value(),
            Me.Third_Dimension.Get_Value())
        elmt_view_form.ShowDialog()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String

        ' Compute Box width (it depends on the longuest line of the attributes compartment)
        ' Build the lines of the attributes compartment
        Dim dim_str As String = "["
        dim_str &= Me.First_Dimension.Get_Value() & "]"
        If Not Me.Second_Dimension.Is_One() Then
            dim_str &= "[" & Me.Second_Dimension.Get_Value() & "]"
            If Not Me.Third_Dimension.Is_One() Then
                dim_str &= "[" & Me.Third_Dimension.Get_Value() & "]"
            End If
        End If
        Dim attr_lines As New List(Of String) From {
            "Dimension : " & dim_str,
            "Base : " & Get_Elmt_Name_From_Proj_By_Id(Me.Base_Type_Ref)
        }

        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(attr_lines, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Width = Get_Box_Width(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(
            0, 0, Me.Name, Type.SVG_COLOR, Me.SVG_Width, Array_Type.Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) =
            Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        ' Add attributes compartment
        Dim attr_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR,
            Me.SVG_Width,
            attr_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + attr_rect_height ' - 2*stroke width
        Return Me.SVG_Content
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim base_type_check = New Consistency_Check_Report_Item(Me, Array_Type.Base_Type_Rule)
        report.Add_Item(base_type_check)
        Dim base_type As Software_Element = Me.Get_Elmt_From_Prj_By_Id(Me.Base_Type_Ref)
        base_type_check.Set_Compliance(TypeOf base_type Is Type)

        Dim array_base_not_self_check =
            New Consistency_Check_Report_Item(Me, Array_Type.Array_Base_Not_Self_Rule)
        report.Add_Item(array_base_not_self_check)
        array_base_not_self_check.Set_Compliance(Me.Base_Type_Ref <> Me.Identifier)

        Dim first_dimension_used_check =
            New Consistency_Check_Report_Item(Me, Array_Type.First_Dimension_Used_Rule)
        report.Add_Item(first_dimension_used_check)
        first_dimension_used_check.Set_Compliance(Not Me.First_Dimension.Is_One())

        Dim third_dimension_used_check =
            New Consistency_Check_Report_Item(Me, Array_Type.Third_Dimension_Used_Rule)
        report.Add_Item(third_dimension_used_check)
        third_dimension_used_check.Set_Compliance(False)
        If Not Me.Third_Dimension.Is_One() Then
            If Not Me.Second_Dimension.Is_One() Then
                third_dimension_used_check.Set_Compliance(True)
            End If
        Else
            third_dimension_used_check.Set_Compliance(True)
        End If

        Dim dimensions_not_zero_rule_check =
            New Consistency_Check_Report_Item(Me, Array_Type.Dimensions_Not_Zero_Rule)
        report.Add_Item(dimensions_not_zero_rule_check)
        If Not (Me.First_Dimension.Get_Minimum() = 0 _
            Or Me.Second_Dimension.Get_Minimum() = 0 _
            Or Me.Third_Dimension.Get_Minimum() = 0 _
            Or Me.First_Dimension.Is_Any() _
            Or Me.Second_Dimension.Is_Any() _
            Or Me.Third_Dimension.Is_Any()) Then
            dimensions_not_zero_rule_check.Set_Compliance(True)
        Else
            dimensions_not_zero_rule_check.Set_Compliance(False)
        End If
    End Sub

End Class


Public Class Enumerated_Type
    Inherits Type

    Public Enumerals As New List(Of Enumeral)

    Public Const Metaclass_Name As String = "Enumerated_Type"

    Private Shared ReadOnly Context_Menu As New Enumerated_Type_Context_Menu()

    Private Shared ReadOnly Enumerals_Rule As New Modeling_Rule(
        "Enumerals",
        "Shall aggregate at least two Enumerals.")

    Private Shared ReadOnly Unique_Enumeral_Name_Rule As New Modeling_Rule(
        "Unique_Enumeral_Name",
        "Name of Enumerals shall be unique.")


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

    Public Overrides Function Get_Metaclass_Name() As String
        Return Enumerated_Type.Metaclass_Name
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Enumerated_Type.Context_Menu
    End Function

    Protected Overrides Function Compute_Children_For_Post_Treat() As List(Of Software_Element)
        Me.Children.AddRange(Me.Enumerals)
        Return Me.Children
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Type
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Dim is_valid As Boolean = False
        For Each current_enumeral In Me.Enumerals
            If value_str = current_enumeral.Name Then
                is_valid = True
                Exit For
            End If
        Next
        Return is_valid
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Enumeral()

        Dim creation_form As New Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Enumeral.Metaclass_Name,
            "",
            "ENUMERAL",
            "")

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_enumeral As New Enumeral(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Enumerals.Add(new_enumeral)
            Me.Children.Add(new_enumeral)
            Me.Get_Project().Add_Element_To_Project(new_enumeral)
            Me.Update_Views()
        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String
        Me.SVG_Width = Get_Box_Width(SVG_MIN_CHAR_PER_LINE)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(
            0, 0, Me.Name, Type.SVG_COLOR, Me.SVG_Width, Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) =
            Split_String(Me.Description, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        ' Add enumerals compartment
        Dim enumeral_lines As New List(Of String)
        For Each enumeral In Me.Enumerals
            enumeral_lines.Add(enumeral.Name)
        Next
        Dim enum_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            enumeral_lines,
            Type.SVG_COLOR,
            Me.SVG_Width,
            enum_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + enum_rect_height ' - 2*stroke width
        Return Me.SVG_Content

    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim enumerals_check As New _
            Consistency_Check_Report_Item(Me, Enumerated_Type.Enumerals_Rule)
        report.Add_Item(enumerals_check)
        enumerals_check.Set_Compliance(Me.Enumerals.Count >= 2)

        Dim unique_enumeral_name_check As New _
            Consistency_Check_Report_Item(Me, Enumerated_Type.Unique_Enumeral_Name_Rule)
        report.Add_Item(unique_enumeral_name_check)
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
        unique_enumeral_name_check.Set_Compliance(is_compliant)

    End Sub


End Class


Public Class Enumeral
    Inherits Software_Element

    Public Const Metaclass_Name As String = "Enumeral"


    Private Shared ReadOnly Valid_Enumeral_Name_Regex As String =
        "^[A-Z][A-Z0-9_]{1," & NB_CHARS_MAX_FOR_SYMBOL - 1 & "}$"

    Private Shared ReadOnly Enumeral_Name_Rule As New Modeling_Rule(
        "Enumeral_Name",
        "Name shall match " & Valid_Enumeral_Name_Regex)


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

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Enumerated_Type).Enumerals.Remove(Me)
        CType(new_parent, Enumerated_Type).Enumerals.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_enum_type As Enumerated_Type = CType(Me.Owner, Enumerated_Type)
        Me.Node.Remove()
        parent_enum_type.Enumerals.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Enumeral.Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Enumerated_Type
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim enumeral_name_check As New Consistency_Check_Report_Item(Me, Enumeral_Name_Rule)
        report.Add_Item(enumeral_name_check)
        enumeral_name_check.Set_Compliance(Regex.IsMatch(Me.Name, Valid_Enumeral_Name_Regex))
    End Sub


End Class


Public Class Fixed_Point_Type
    Inherits Type

    Public Base_Type_Ref As Guid

    Public Unit As String
    Public Resolution As String
    Public Offset As String

    Public Const Metaclass_Name As String = "Fixed_Point_Type"

    Private Const Zero_Decimal_Regex_Str As String = "^0+[,|.]?0*$"
    Private Shared ReadOnly Valid_Decimal_Ratio_Regex As New _
        Regex("^(?<Sign>-?)(?<Num>\d+[.|,]?\d*)(\/(?<Den>\d+[.|,]?\d*))?$")

    Private Shared ReadOnly Valid_Power_Two_Regex As New _
        Regex("^(?<Sign>-?)2\^(?<Power_Sign>-?)(?<Power>\d+)$")

    Private Shared ReadOnly Unit_Rule As New Modeling_Rule(
        "Unit",
        "Unit shall be set.")
    Private Shared ReadOnly Base_Type_Rule As New Modeling_Rule(
        "Base_Type",
        "Referenced type shall be a Basic_Integer_Type.")
    Private Shared ReadOnly Resolution_Rule As New Modeling_Rule(
        "Resolution",
        "Resolution shall be a strictly positive decimal value.")
    Private Shared ReadOnly Offset_Rule As New Modeling_Rule(
        "Offset",
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
        Dim result As String = "unresolved"
        Dim referenced_type As Software_Element
        referenced_type = Me.Get_Elmt_From_Prj_By_Id(Me.Base_Type_Ref)
        If Not IsNothing(referenced_type) Then
            If get_full_path = True Then
                result = referenced_type.Get_Path()
            Else
                result = referenced_type.Name
            End If
        End If
        Return result
    End Function

    Public Shared Function Is_Resolution_Valid(resolution_str As String) As Boolean
        Dim result As Boolean = False
        Dim regex_match As Match
        regex_match = Fixed_Point_Type.Valid_Decimal_Ratio_Regex.Match(resolution_str)
        If regex_match.Success = True Then
            result = True
            ' Resolution is not valid if it is = 0
            Dim num_str As String = regex_match.Groups.Item("Num").Value
            If Regex.IsMatch(num_str, Fixed_Point_Type.Zero_Decimal_Regex_Str) Then
                result = False
            End If
            ' Resolution is not valid if it is < 0
            If regex_match.Groups.Item("Sign").Value = "-" Then
                result = False
            End If
            ' Resolution is not valid if denominator is 0
            If regex_match.Groups.Item("Den").Success = True Then
                Dim den_str As String = regex_match.Groups.Item("Den").Value
                If Regex.IsMatch(den_str, Fixed_Point_Type.Zero_Decimal_Regex_Str) Then
                    result = False
                End If
            End If
        Else
            regex_match = Fixed_Point_Type.Valid_Power_Two_Regex.Match(resolution_str)
            If regex_match.Success = True Then
                result = True
                ' Resolution is not valid if it is < 0
                If regex_match.Groups.Item("Sign").Value = "-" Then
                    result = False
                End If
            End If
        End If
        Return result
    End Function

    Public Shared Function Is_Valid_Fixed_Point(fp_str As String) As Boolean
        Dim result As Boolean
        Dim regex_match As Match
        regex_match = Fixed_Point_Type.Valid_Decimal_Ratio_Regex.Match(fp_str)
        If regex_match.Success = True Then
            result = True
            If Not IsNothing(regex_match.Groups.Item("Den")) Then
                Dim den_str As String = regex_match.Groups.Item("Den").Value
                If Regex.IsMatch(den_str, Fixed_Point_Type.Zero_Decimal_Regex_Str) Then
                    result = False
                End If
            End If
        Else
            result = Fixed_Point_Type.Valid_Power_Two_Regex.Match(fp_str).Success
        End If
        Return result


    End Function

    Public Shared Function Is_Offset_Valid(offset_str As String) As Boolean
        Return Is_Valid_Fixed_Point(offset_str)
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_Metaclass_Name() As String
        Return Fixed_Point_Type.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements.Clear()
        Dim data_type As Type
        data_type = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Base_Type_Ref), Type)
        If Not IsNothing(data_type) Then
            Me.Needed_Elements.Add(data_type)
        End If
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Type
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Dim is_valid As Boolean = False
        Dim is_value_decimal As Boolean = Is_Valid_Fixed_Point(value_str)

        is_valid = is_value_decimal
        ' need to test if value can be reached knowing Resolutio, Offset and Base_Type

        Return is_valid
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        Dim edit_form As New Fixed_Point_Type_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Referenced_Type(True),
            Me.Get_All_Basic_Int_From_Project(),
            Me.Unit,
            Me.Resolution,
            Me.Offset)

        Dim edition_form_result As DialogResult = edit_form.ShowDialog()
        If edition_form_result = DialogResult.OK Then

            ' Update the fixed point type
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Base_Type_Ref = edit_form.Get_Ref_Element_Identifier()
            Me.Unit = edit_form.Get_Unit()
            Me.Resolution = edit_form.Get_Resolution()
            Me.Offset = edit_form.Get_Offset()

            Me.Update_Views()
        End If
    End Sub

    Public Overrides Sub View()

        Dim view_form As New Fixed_Point_Type_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
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

    Public Overrides Function Compute_SVG_Content() As String
        ' Compute Box width (it depends on the longuest line of the attributes compartment)
        ' Build the lines of the attributes compartment
        Dim attr_lines As New List(Of String) From {
            "Base : " & Get_Referenced_Type(False),
            "Unit : " & Me.Unit,
            "Resolution : " & Me.Resolution,
            "Offset : " & Me.Offset}

        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(attr_lines, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Width = Get_Box_Width(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(
            0, 0, Me.Name, Type.SVG_COLOR, Me.SVG_Width, Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) =
            Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        ' Add attributes compartment
        Dim attr_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR,
            Me.SVG_Width,
            attr_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + attr_rect_height ' - 2*stroke width
        Return Me.SVG_Content
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
        Dim referenced_type As Software_Element = Me.Get_Elmt_From_Prj_By_Id(Me.Base_Type_Ref)
        base_type_check.Set_Compliance(TypeOf referenced_type Is Basic_Integer_Type)

        Dim resol_check = New Consistency_Check_Report_Item(Me, Fixed_Point_Type.Resolution_Rule)
        report.Add_Item(resol_check)
        resol_check.Set_Compliance(Fixed_Point_Type.Is_Resolution_Valid(Me.Resolution))

        Dim offset_check = New Consistency_Check_Report_Item(Me, Fixed_Point_Type.Offset_Rule)
        report.Add_Item(offset_check)
        offset_check.Set_Compliance(Fixed_Point_Type.Is_Offset_Valid(Me.Offset))

    End Sub

End Class


Public Class Record_Type
    Inherits Type

    Public Fields As New List(Of Record_Field)

    Public Const Metaclass_Name As String = "Record_Type"

    Private Shared ReadOnly Context_Menu As New Record_Type_Context_Menu()

    Private Shared ReadOnly Fields_Rule As New Modeling_Rule(
        "Fields",
        "Shall aggregate at least two Fields.")

    Private Shared ReadOnly Fields_Base_Type_Rule As New Modeling_Rule(
        "Fields_Base_Type",
        "Shall not be referenced by its Fields.")


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

    Public Overrides Function Get_Metaclass_Name() As String
        Return Record_Type.Metaclass_Name
    End Function


    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Record_Type.Context_Menu
    End Function

    Protected Overrides Function Compute_Children_For_Post_Treat() As List(Of Software_Element)
        Me.Children.AddRange(Me.Fields)
        Return Me.Children
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements.Clear()
        For Each fd In Me.Fields
            Dim data_type As Type
            data_type = CType(Me.Get_Elmt_From_Prj_By_Id(fd.Referenced_Type_Id), Type)
            If Not IsNothing(data_type) Then
                If Not Me.Needed_Elements.Contains(data_type) Then
                    Me.Needed_Elements.Add(data_type)
                End If
            End If
        Next
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Type
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Value_Valid(value_str As String) As Boolean
        Dim all_valid As Boolean = False
        Dim struct_match As Match
        struct_match = Regex.Match(value_str, "^\s*\{([\d|\D]*)\}\s*$")
        If struct_match.Success = True Then
            Dim struct_fields_match As MatchCollection
            struct_fields_match = Regex.Matches(struct_match.Groups(1).Value, "([^;])+")
            Dim fields_list As New List(Of String)
            For idx = 0 To struct_fields_match.Count - 1
                fields_list.Add(struct_fields_match.Item(idx).Value.Trim)
            Next
            If fields_list.Count = CDbl(Me.Fields.Count) Then
                all_valid = True
                For idx = 0 To fields_list.Count - 1
                    Dim field_type_id As Guid = Me.Fields(idx).Referenced_Type_Id
                    Dim field_type As Type = CType(Get_Elmt_From_Prj_By_Id(field_type_id), Type)
                    If field_type.Is_Value_Valid(fields_list(idx)) = False Then
                        all_valid = False
                        Exit For
                    End If
                Next
            End If
        End If
        Return all_valid
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Field()

        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Record_Field.Metaclass_Name,
            "",
            Record_Field.Metaclass_Name,
            "",
            "Type",
            "",
            Me.Get_All_Types_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_field As New Record_Field(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element_Identifier())
            Me.Fields.Add(new_field)
            Me.Children.Add(new_field)
            Me.Get_Project().Add_Element_To_Project(new_field)
            Me.Update_Views()
        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String

        ' Compute Box width (it depends on the longuest line of the fields compartment)
        ' Build the lines of the fields compartment
        Dim fields_lines As New List(Of String)
        For Each field In Me.Fields
            Dim referenced_type_name As String = field.Get_Referenced_Type_Name()
            fields_lines.Add("+ " & field.Name & " : " & referenced_type_name)
        Next

        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(fields_lines, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Width = Get_Box_Width(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(
            0, 0, Me.Name, Type.SVG_COLOR, Me.SVG_Width, Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) =
            Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        ' Add fields compartment
        Dim fields_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            fields_lines,
            Type.SVG_COLOR,
            Me.SVG_Width,
            fields_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + fields_rect_height ' - 2*stroke width
        Return Me.SVG_Content

    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim fields_check As New Consistency_Check_Report_Item(Me, Record_Type.Fields_Rule)
        report.Add_Item(fields_check)
        fields_check.Set_Compliance(Me.Fields.Count >= 2)

        Dim fields_base_type_check As _
            New Consistency_Check_Report_Item(Me, Fields_Base_Type_Rule)
        report.Add_Item(fields_base_type_check)
        Dim children_reference_me As Boolean = False
        For Each f In Me.Fields
            If f.Referenced_Type_Id = Me.Identifier Then
                children_reference_me = True
                fields_base_type_check.Set_Message("Referenced by : " & f.Name)
                Exit For
            End If
        Next
        fields_base_type_check.Set_Compliance(Not children_reference_me)

    End Sub

End Class


Public Class Record_Field
    Inherits Typed_Element

    Public Const Metaclass_Name As String = "Record_Field"


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
        MyBase.New(name, description, owner, parent_node, base_type_ref)
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
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String

        Dim attr_lines As New List(Of String) From {
            "Base : " & Me.Get_Referenced_Type_Path()}

        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Math.Max(attr_lines(0).Length, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Width = Get_Box_Width(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        Me.SVG_Content &= Get_Title_Rectangle(
            0, 0, Me.Name, Type.SVG_COLOR, Me.SVG_Width, "field")

        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Type.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        Dim attr_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            attr_lines,
            Type.SVG_COLOR,
            Me.SVG_Width)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + attr_rect_height ' - 2*stroke width
        Return Me.SVG_Content
    End Function


End Class