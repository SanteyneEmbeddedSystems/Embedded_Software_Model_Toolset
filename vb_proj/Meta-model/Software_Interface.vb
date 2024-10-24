﻿Imports System.Math

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


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_Alternative_SVG_Def_Group() As String

        Dim svg_content As String
        svg_content = Me.Get_SVG_Def_Group_Header(True)

        ' Add needed types
        Dim max_dt_height As Integer = 0
        Dim needed_types_list As List(Of Classifier) = Me.Find_Needed_Elements()
        Dim dt_x_position As Integer = 0
        For Each dt In needed_types_list
            svg_content &= dt.Get_SVG_Def_Group()
            max_dt_height = Max(max_dt_height, dt.Get_SVG_Height())
            svg_content &= "  <use xlink:href=""#" & dt.Get_SVG_Id() &
                           """ transform=""translate(" & dt_x_position &
                           "," & 0 & ")"" />" & vbCrLf
            dt_x_position += dt.Get_SVG_Width() + SVG_BOX_MARGIN
        Next
        Dim data_types_width As Integer = dt_x_position - SVG_BOX_MARGIN

        ' Add Me (Interface)
        Dim y_position As Integer
        If max_dt_height <> 0 Then
            y_position = max_dt_height + SVG_BOX_MARGIN
        Else
            y_position = 0
        End If
        svg_content &= Me.Get_SVG_Def_Group()
        Dim x_position As Integer = 0
        If data_types_width > Me.SVG_Width Then
            x_position = (data_types_width - Me.SVG_Width) \ 2
        End If
        svg_content &= "  <use xlink:href=""#" & Me.Get_SVG_Id() &
                           """ transform=""translate(" & x_position &
                           "," & y_position & ")"" />" & vbCrLf

        If max_dt_height > 0 Then ' At least one type needed
            Me.Alt_SVG_Height = max_dt_height + SVG_BOX_MARGIN + Me.SVG_Height
        Else ' No type needed
            Me.Alt_SVG_Height = Me.SVG_Height
        End If

        Me.Alt_SVG_Width = Max(data_types_width, Me.SVG_Width)

        svg_content &= Get_SVG_Def_Group_Footer()
        Return svg_content

    End Function

End Class


Public Class Client_Server_Interface
    Inherits Software_Interface

    Public Operations As New List(Of Client_Server_Operation)

    Public Const Metaclass_Name As String = "Client_Server_Interface"

    Private Shared ReadOnly Context_Menu As New Client_Server_Interface_Context_Menu()

    Public Const SVG_COLOR As String = "rgb(34,177,76)"

    Private Shared ReadOnly Operations_Rule As New Modeling_Rule(
        "Operations",
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

    Protected Overrides Function Compute_Children_For_Post_Treat() As List(Of Software_Element)
        Me.Children.AddRange(Me.Operations)
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
        Me.Needed_Elements.Clear()
        For Each op In Me.Operations
            For Each param In op.Parameters
                Dim data_type As Type
                data_type = CType(Me.Get_Elmt_From_Prj_By_Id(param.Referenced_Type_Id), Type)
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
            "")

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

    Public Sub Show_Dependencies_On_Diagram()
        Me.Get_Project().Update_Alternative_Diagram(Me)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Def_Group() As String

        ' Compute Box width (it depends on the longuest line of the operations compartment)
        ' Build the lines of the operations compartment
        Dim op_lines As New List(Of String)
        For Each op In Me.Operations
            Dim op_line As String
            If op.Parameters.Count = 0 Then
                op_line = "+ " & op.Name & "()"
                op_lines.Add(op_line)
            ElseIf op.Parameters.Count = 1 Then
                Dim param As Operation_Parameter = op.Parameters(0)
                Dim type_name As String = param.Get_Referenced_Type_Name()
                op_line = "+ " & op.Name & "( " & param.Get_Short_Direction() & " " &
                        param.Name & ":" & type_name & " )"
                If op_line.Length <= MAX_NB_OF_CHAR_FOR_OPERATION_LINE Then
                    op_lines.Add(op_line)
                Else
                    op_line = "+ " & op.Name & "("
                    op_lines.Add(op_line)
                    op_line = "      " & param.Get_Short_Direction() & " " &
                        param.Name & ":" & type_name & " )"
                    op_lines.Add(op_line)
                End If
            Else
                ' op.Parameters.Count >= 2
                op_line = "+ " & op.Name & "("
                op_lines.Add(op_line)
                For Each param In op.Parameters
                    Dim type_name As String = param.Get_Referenced_Type_Name()
                    op_line = "      " & param.Get_Short_Direction() & " " &
                        param.Name & ":" & type_name
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
        Dim nb_max_char_per_line As Integer = SVG_MIN_CHAR_PER_LINE
        For Each line In op_lines
            nb_max_char_per_line = Math.Max(nb_max_char_per_line, line.Length)
        Next
        Me.SVG_Width = Get_Box_Width(nb_max_char_per_line)

        Dim padded_op_lines As New List(Of String)
        For Each op_line In op_lines
            padded_op_lines.Add(op_line.Replace("      ", "&#160;&#160;&#160;&#160;&#160;&#160;"))
        Next

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(0, 0, Me.Name,
            Client_Server_Interface.SVG_COLOR, Me.SVG_Width, Metaclass_Name, True)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Client_Server_Interface.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        ' Add operation compartment
        Dim op_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            padded_op_lines,
            Client_Server_Interface.SVG_COLOR,
            Me.SVG_Width,
            op_rect_height,
            True)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + op_rect_height ' - 2 * stroke width
        Return Me.SVG_Content

    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim nb_op_check As New Consistency_Check_Report_Item(Me, Operations_Rule)
        report.Add_Item(nb_op_check)
        nb_op_check.Set_Compliance(Me.Operations.Count >= 1)

    End Sub


End Class


Public Class Client_Server_Operation
    Inherits Described_Element

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

    Protected Overrides Function Compute_Children_For_Post_Treat() As List(Of Software_Element)
        Me.Children.AddRange(Me.Parameters)
        Return Me.Children
    End Function

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of Client_Server_Operation) =
            CType(Me.Owner, Client_Server_Interface).Operations
        Dim my_index As Integer = container.IndexOf(Me)
        If offset = -1 And my_index <> 0 Or offset = 1 And my_index <> container.Count - 1 Then
            container.RemoveAt(my_index)
            container.Insert(my_index + offset, Me)
            Return True
        Else
            Return False
        End If
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Parameter()

        Dim creation_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            "",
            Operation_Parameter.Metaclass_Name,
            "",
            "",
            Me.Get_All_Types_From_Project(),
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
                creation_form.Get_Ref_Element_Identifier(),
                direction)

            Me.Parameters.Add(new_param)
            Me.Children.Add(new_param)
            Me.Get_Project().Add_Element_To_Project(new_param)

            Me.Update_Views()

        End If

    End Sub

End Class


Public Class Operation_Parameter
    Inherits Typed_Element

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

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of Operation_Parameter) =
            CType(Me.Owner, Client_Server_Operation).Parameters
        Dim my_index As Integer = container.IndexOf(Me)
        If offset = -1 And my_index <> 0 Or offset = 1 And my_index <> container.Count - 1 Then
            container.RemoveAt(my_index)
            container.Insert(my_index + offset, Me)
            Return True
        Else
            Return False
        End If
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        Dim edition_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Type_Id),
            Me.Get_All_Types_From_Project(),
            Operation_Parameter.Directions,
            Me.Direction.ToString())

        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Update Me
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Referenced_Type_Id = edition_form.Get_Ref_Element_Identifier()
            [Enum].TryParse(edition_form.Get_Direction(), Me.Direction)

            Me.Update_Views()
        End If

    End Sub

    Public Overrides Sub View()
        Dim elmt_view_form As New Operation_Parameter_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Type_Id),
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

    Protected Overrides Function Compute_Children_For_Post_Treat() As List(Of Software_Element)
        Me.Children.AddRange(Me.Parameters)
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
        Me.Needed_Elements.Clear()
        For Each param In Me.Parameters
            Dim data_type As Type
            data_type = CType(Me.Get_Elmt_From_Prj_By_Id(param.Referenced_Type_Id), Type)
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
            "Type",
            "",
            Me.Get_All_Types_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim new_param As New Event_Parameter(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element_Identifier())

            Me.Parameters.Add(new_param)
            Me.Children.Add(new_param)
            Me.Get_Project().Add_Element_To_Project(new_param)

            Me.Update_Views()

        End If

    End Sub

    Public Sub Show_Dependencies_On_Diagram()
        Me.Get_Project().Update_Alternative_Diagram(Me)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Def_Group() As String

        ' Compute Box width (it depends on the longuest line of the parameters compartment)
        ' Build the lines of the parameters compartment
        Dim param_lines As New List(Of String)
        For Each param In Me.Parameters
            Dim type_name As String = param.Get_Referenced_Type_Name()
            Dim param_line As String = "+ " & param.Name & " : " & type_name
            param_lines.Add(param_line)
        Next
        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(param_lines, SVG_MIN_CHAR_PER_LINE)
        Me.SVG_Width = Get_Box_Width(nb_max_char_per_line)

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(0, 0, Me.Name,
            Event_Interface.SVG_COLOR, Me.SVG_Width, Metaclass_Name, True)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, nb_max_char_per_line)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            Event_Interface.SVG_COLOR,
            Me.SVG_Width,
            desc_rect_height)

        ' Add parameters compartments
        Dim param_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            param_lines,
            Event_Interface.SVG_COLOR,
            Me.SVG_Width,
            param_rect_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Height = SVG_TITLE_HEIGHT + desc_rect_height + param_rect_height ' - 2 * stroke width
        Return Me.SVG_Content

    End Function

End Class


Public Class Event_Parameter
    Inherits Typed_Element

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

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of Event_Parameter) = CType(Me.Owner, Event_Interface).Parameters
        Dim my_index As Integer = container.IndexOf(Me)
        If offset = -1 And my_index <> 0 Or offset = 1 And my_index <> container.Count - 1 Then
            container.RemoveAt(my_index)
            container.Insert(my_index + offset, Me)
            Return True
        Else
            Return False
        End If
    End Function

End Class