Public MustInherit Class Software_Interface
    Inherits Must_Describe_Software_Element


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
        CType(Me.Owner, Package).Interfaces.Remove(Me)
        CType(new_parent, Package).Interfaces.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Interfaces.Remove(Me)
    End Sub

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Dim is_allowed As Boolean = False
        If parent.GetType() = GetType(Top_Level_Package) _
            Or parent.GetType() = GetType(Package) Then
            is_allowed = True
        End If
        Return is_allowed
    End Function

End Class


Public Class Client_Server_Interface
    Inherits Software_Interface

    Public Operations As New List(Of Client_Server_Operation)

    Public Shared ReadOnly Metaclass_Name As String = "Client_Server_Interface"

    Private Shared ReadOnly Context_Menu As New Client_Server_Interface_Context_Menu()

    Public Shared ReadOnly SVG_COLOR As String = "rgb(34,177,76)"

    Private Shared ReadOnly Nb_Operation_Rule As New Modeling_Rule(
        "Number_Of_Operations",
        "Shall aggregate at least one Operation.")


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

    Protected Overrides Function Get_Children() As List(Of Software_Element)
        If Me.Children_Is_Computed = False Then
            Me.Children_Is_Computed = True
            Me.Children.AddRange(Me.Operations)
        End If
        Return Me.Children
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Client_Server_Interface.Context_Menu
    End Function

    Public Overrides Function Get_Metaclass_Name() As String
        Return Client_Server_Interface.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Operation()

        Dim creation_form As New Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Client_Server_Operation.Metaclass_Name,
            "",
            Client_Server_Operation.Metaclass_Name,
            "",
            Me.Get_Children_Name())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_op As New Client_Server_Operation(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Operations.Add(new_op)
            Me.Children.Add(new_op)
            Me.Update_Views()
        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        ' Title (Name + stereotype)
        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name,
            Client_Server_Interface.SVG_COLOR, "interface", True)

        ' Description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String) = Split_String(Me.Description, NB_CHARS_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Client_Server_Interface.SVG_COLOR,
            desc_rect_height)

        ' Operation compartment
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)
        Dim op_lines As New List(Of String)
        For Each op In Me.Operations
            Dim op_line As String = "+ " & op.Name & "("
            For Each param In op.Parameters
                Dim type_name As String = "unresolved"
                If type_by_uuid_dict.ContainsKey(param.Type_Ref) Then
                    type_name = type_by_uuid_dict(param.Type_Ref).Name
                End If
                op_line &= " " & param.Get_Short_Direction() & " " & param.Name & " : "
                op_line &= type_name & ","
            Next
            If op.Parameters.Count > 0 Then
                op_line = op_line.Substring(0, op_line.Length - 1)
                op_line &= " "
            End If
            op_line &= ")"
            op_lines.Add(op_line)
        Next
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            op_lines,
            Client_Server_Interface.SVG_COLOR)

        Return svg_content

    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim nb_op_check As New Consistency_Check_Report_Item(Me, Nb_Operation_Rule)
        report.Add_Item(nb_op_check)
        nb_op_check.Set_Compliance(Me.Operations.Count >= 1)

    End Sub


End Class


Public Class Client_Server_Operation
    Inherits Must_Describe_Software_Element

    Public Parameters As New List(Of Operation_Parameter)

    Public Shared ReadOnly Metaclass_Name As String = "Client_Server_Operation"

    Private Shared ReadOnly Context_Menu As New Client_Server_Operation_Context_Menu()

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

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Client_Server_Operation.Context_Menu
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Client_Server_Interface).Operations.Remove(Me)
        CType(new_parent, Client_Server_Interface).Operations.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_if As Client_Server_Interface = CType(Me.Owner, Client_Server_Interface)
        Me.Node.Remove()
        parent_if.Operations.Remove(Me)
    End Sub

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return parent.GetType() = GetType(Client_Server_Interface)
    End Function

    Public Overrides Function Get_Metaclass_Name() As String
        Return Client_Server_Operation.Metaclass_Name
    End Function

    Protected Overrides Function Get_Children() As List(Of Software_Element)
        If Me.Children_Is_Computed = False Then
            Me.Children_Is_Computed = True
            Me.Children.AddRange(Me.Parameters)
        End If
        Return Me.Children
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Parameter()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(type_list)

        Dim creation_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Operation_Parameter.Metaclass_Name,
            "",
            Operation_Parameter.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            type_by_path_dict.Keys(0),
            type_by_path_dict.Keys.ToList(),
            Operation_Parameter.Directions,
            Operation_Parameter.Directions(0))

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim ref_type As Software_Element
            ref_type = type_by_path_dict(creation_form.Get_Ref_Rerenced_Element_Path())

            Dim direction As Operation_Parameter.E_DIRECTION
            [Enum].TryParse(creation_form.Get_Direction(), direction)

            Dim new_param As New Operation_Parameter(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                direction,
                ref_type.Identifier)

            Me.Parameters.Add(new_param)
            Me.Children.Add(new_param)

            Me.Update_Views()

        End If

    End Sub

End Class


Public Class Operation_Parameter
    Inherits Must_Describe_Software_Element

    Public Direction As E_DIRECTION
    Public Type_Ref As Guid

    Public Shared ReadOnly Metaclass_Name As String = "Operation_Parameter"

    Public Shared ReadOnly Directions As String() =
        [Enum].GetNames(GetType(Operation_Parameter.E_DIRECTION))

    Public Enum E_DIRECTION
        INPUT
        OUTPUT
    End Enum

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
            direction As E_DIRECTION,
            type As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Direction = direction
        Me.Type_Ref = type
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Client_Server_Operation).Parameters.Remove(Me)
        CType(new_parent, Client_Server_Operation).Parameters.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_op As Client_Server_Operation = CType(Me.Owner, Client_Server_Operation)
        Me.Node.Remove()
        parent_op.Parameters.Remove(Me)
    End Sub

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return parent.GetType() = GetType(Client_Server_Operation)
    End Function

    Public Overrides Function Get_Metaclass_Name() As String
        Return Operation_Parameter.Metaclass_Name
    End Function

    Protected Overrides Function Get_Path_Separator() As String
        Return "."
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        ' Build the list of possible type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
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

        Dim edition_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Operation_Parameter.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            forbidden_name_list,
            current_referenced_type_path,
            type_by_path_dict.Keys.ToList(),
            Operation_Parameter.Directions,
            Operation_Parameter.Directions(0))

        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Get the referenced type
            Dim new_referenced_type As Software_Element
            new_referenced_type = type_by_path_dict(edition_form.Get_Ref_Rerenced_Element_Path())

            ' Update Me
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Type_Ref = new_referenced_type.Identifier
            [Enum].TryParse(edition_form.Get_Direction(), Me.Direction)

            Me.Update_Views()
        End If

    End Sub

    Public Overrides Sub View()

        ' Build the list of possible type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path
        Dim referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Type_Ref) Then
            referenced_type_path = type_by_uuid_dict(Me.Type_Ref).Get_Path()
        End If

        Dim elmt_view_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Operation_Parameter.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
            referenced_type_path,
            Nothing,
            Operation_Parameter.Directions,
            Me.Direction.ToString())
        elmt_view_form.ShowDialog()

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Specific methods
    ' -------------------------------------------------------------------------------------------- '

    Public Function Get_Short_Direction() As String
        Select Case Me.Direction
            Case E_DIRECTION.INPUT
                Return "IN"
            Case E_DIRECTION.OUTPUT
                Return "OUT"
            Case Else
                Return "IN"
        End Select
    End Function

End Class