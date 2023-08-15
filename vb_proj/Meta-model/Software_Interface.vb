Public MustInherit Class Software_Interface
    Inherits Classifier


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

End Class


Public Class Client_Server_Interface
    Inherits Software_Interface

    Public Operations As New List(Of Client_Server_Operation)

    Public Const Metaclass_Name As String = "Client_Server_Interface"

    Private Shared ReadOnly Context_Menu As New Client_Server_Interface_Context_Menu()

    Public Const SVG_COLOR As String = "rgb(34,177,76)"

    Private Shared ReadOnly Nb_Operation_Rule As New Modeling_Rule(
        "Number_Of_Operations",
        "Shall aggregate at least one Operation.")

    Private Const MAX_NB_OF_CHAR_FOR_OPERATION_LINE As Integer = 77


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
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '
    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements = New List(Of Classifier)
        For Each op In Me.Operations
            For Each param In op.Parameters
                Dim data_type As Type
                data_type = CType(Me.Get_Element_From_Project_By_Identifier(param.Type_Ref), Type)
                If Not IsNothing(data_type) Then
                    If Not Me.Needed_Elements.Contains(data_type) Then
                        Me.Needed_Elements.Add(data_type)
                    End If
                End If
            Next
        Next
        Return Me.Needed_Elements
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
            Me.Get_Project().Add_Element_To_Project(new_op)
            Me.Update_Views()
        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String
        Dim x_pos As Integer = 0
        Dim y_pos As Integer = 0

        ' Compute Box width (it depends on the longuest line of the operations compartment)
        ' Build the lines of the operations compartment
        Dim op_lines As New List(Of String)
        Dim indent_used As Boolean = False
        For Each op In Me.Operations
            Dim op_line As String
            If op.Parameters.Count = 0 Then
                op_line = "+ " & op.Name & "()"
                op_lines.Add(op_line)
            ElseIf op.Parameters.Count = 1 Then
                Dim param As Operation_Parameter = op.Parameters(0)
                Dim type_name As String = param.Get_Type_Name()
                op_line = "+ " & op.Name & "( " & param.Get_Short_Direction() & " " &
                        param.Name & ":" & type_name & " )"
                If op_line.Length <= MAX_NB_OF_CHAR_FOR_OPERATION_LINE Then
                    op_lines.Add(op_line)
                Else
                    op_line = "+ " & op.Name & "("
                    op_lines.Add(op_line)
                    op_line = "&#160;&#160;&#160;&#160;&#160;&#160;" &
                        param.Get_Short_Direction() & " " &
                        param.Name & ":" & type_name & " )"
                    indent_used = True
                    op_lines.Add(op_line)
                End If
            Else
                ' op.Parameters.Count >= 2
                op_line = "+ " & op.Name & "("
                op_lines.Add(op_line)
                For Each param In op.Parameters
                    Dim type_name As String = param.Get_Type_Name()
                    op_line = "&#160;&#160;&#160;&#160;&#160;&#160;" &
                        param.Get_Short_Direction() & " " &
                        param.Name & ":" & type_name
                    indent_used = True
                    If param Is op.Parameters.Last Then
                        op_line &= " )"
                    Else
                        op_line &= ","
                    End If
                    op_lines.Add(op_line)
                Next
            End If
        Next
        ' Get the longuest line
        Dim nb_max_char_per_line As Integer = 0
        For Each line In op_lines
            nb_max_char_per_line = Math.Max(nb_max_char_per_line, line.Length)
        Next
        If indent_used = True Then
            ' -30 to not count &#160;&#160;&#160;&#160;&#160;&#160; = 36 char but add 6 real spaces
            nb_max_char_per_line = Math.Max(nb_max_char_per_line - 30, SVG_MIN_CHAR_PER_LINE)
        Else
            nb_max_char_per_line = Math.Max(nb_max_char_per_line, SVG_MIN_CHAR_PER_LINE)
        End If
        Dim box_width As Integer = Get_Box_Witdh(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(x_pos, y_pos, Me.Name,
            Client_Server_Interface.SVG_COLOR, box_width, Metaclass_Name, True)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Client_Server_Interface.SVG_COLOR,
            box_width,
            desc_rect_height)

        ' Add operation compartment
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            op_lines,
            Client_Server_Interface.SVG_COLOR,
            box_width)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Return Me.SVG_Content

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

    Public Const Metaclass_Name As String = "Client_Server_Operation"

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

        Dim creation_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Operation_Parameter.Metaclass_Name,
            "",
            Operation_Parameter.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "",
            Me.Get_All_Types_Path_From_Project(),
            Operation_Parameter.Directions,
            Operation_Parameter.Directions(0))

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim direction As Operation_Parameter.E_DIRECTION
            [Enum].TryParse(creation_form.Get_Direction(), direction)

            Dim new_param As New Operation_Parameter(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                Me.Get_Type_From_Project_By_Path(creation_form.Get_Ref_Element_Path()).Identifier,
                direction)

            Me.Parameters.Add(new_param)
            Me.Children.Add(new_param)
            Me.Get_Project().Add_Element_To_Project(new_param)

            Me.Update_Views()

        End If

    End Sub

End Class


Public Class Operation_Parameter
    Inherits Typed_Software_Element

    Public Direction As E_DIRECTION

    Public Const Metaclass_Name As String = "Operation_Parameter"

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
            type As Guid,
            direction As E_DIRECTION)
        MyBase.New(name, description, owner, parent_node, type)
        Me.Direction = direction
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

        Dim edition_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Operation_Parameter.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Forbidden_Name_List(),
            Me.Get_Type_Path(),
            Me.Get_All_Types_Path_From_Project(),
            Operation_Parameter.Directions,
            Operation_Parameter.Directions(0))

        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Update Me
            Dim old_path As String = Me.Get_Path()
            Me.Name = edition_form.Get_Element_Name()
            Update_Project(old_path)
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Type_Ref = Me.Get_Type_From_Project_By_Path(edition_form.Get_Ref_Element_Path()) _
                .Identifier
            [Enum].TryParse(edition_form.Get_Direction(), Me.Direction)

            Me.Update_Views()
        End If

    End Sub

    Public Overrides Sub View()
        Dim elmt_view_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Operation_Parameter.Metaclass_Name,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
            Me.Get_Type_Path(),
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


Public Class Event_Interface
    Inherits Software_Interface

    Public Parameters As New List(Of Event_Parameter)

    Public Const Metaclass_Name As String = "Event_Interface"

    Private Shared ReadOnly Context_Menu As New Event_Interface_Context_Menu()

    Public Const SVG_COLOR As String = "rgb(163,73,164)"


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
            Me.Children.AddRange(Me.Parameters)
        End If
        Return Me.Children
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Event_Interface.Context_Menu
    End Function

    Public Overrides Function Get_Metaclass_Name() As String
        Return Event_Interface.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '
    
    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements = New List(Of Classifier)
        For Each param In Me.Parameters
            Dim data_type As Type
            data_type = CType(Me.Get_Element_From_Project_By_Identifier(param.Type_Ref), Type)
            If Not IsNothing(data_type) Then
                If Not Me.Needed_Elements.Contains(data_type) Then
                    Me.Needed_Elements.Add(data_type)
                End If
            End If
        Next
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Parameter()

        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Event_Parameter.Metaclass_Name,
            "",
            Event_Parameter.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Type",
            "",
            Me.Get_All_Types_Path_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim new_param As New Event_Parameter(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                Me.Get_Type_From_Project_By_Path(creation_form.Get_Ref_Element_Path()).Identifier)

            Me.Parameters.Add(new_param)
            Me.Children.Add(new_param)
            Me.Get_Project().Add_Element_To_Project(new_param)

            Me.Update_Views()

        End If

    End Sub

    Public Overrides Function Compute_SVG_Content() As String
        Dim x_pos As Integer = 0
        Dim y_pos As Integer = 0

        ' Compute Box width (it depends on the longuest line of the parameters compartment)
        ' Build the lines of the parameters compartment
        Dim param_lines As New List(Of String)
        For Each param In Me.Parameters
            Dim type_name As String = param.Get_Type_Name()
            Dim param_line As String = "+ " & param.Name & " : " & type_name
            param_lines.Add(param_line)
        Next
        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(param_lines, SVG_MIN_CHAR_PER_LINE)
        Dim box_width As Integer = Get_Box_Witdh(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(x_pos, y_pos, Me.Name,
            Event_Interface.SVG_COLOR, box_width, Metaclass_Name, True)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Event_Interface.SVG_COLOR,
            box_width,
            desc_rect_height)

        ' Add parameters compartments
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            param_lines,
            Event_Interface.SVG_COLOR,
            box_width)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Return Me.SVG_Content

    End Function

End Class


Public Class Event_Parameter
    Inherits Typed_Software_Element

    Public Shared ReadOnly Metaclass_Name As String = "Event_Parameter"

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
            type As Guid)
        MyBase.New(name, description, owner, parent_node, type)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Event_Interface).Parameters.Remove(Me)
        CType(new_parent, Event_Interface).Parameters.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_if As Event_Interface = CType(Me.Owner, Event_Interface)
        Me.Node.Remove()
        parent_if.Parameters.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Event_Parameter.Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return parent.GetType() = GetType(Event_Interface)
    End Function

End Class