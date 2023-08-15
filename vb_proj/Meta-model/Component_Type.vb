Public Class Component_Type
    Inherits Classifier

    Public Configurations As New List(Of Configuration_Parameter)
    Public Operations As New List(Of OS_Operation)
    Public Provider_Ports As New List(Of Provider_Port)
    Public Requirer_Ports As New List(Of Requirer_Port)

    Public Const Metaclass_Name As String = "Component_Type"

    Public Const SVG_COLOR As String = "rgb(0,0,0)"
    Private Const PORT_SPACE As Integer = 60
    Private Const PORT_SIDE As Integer = 16
    Private Const LOLLIPOP_RADIUS As Integer = PORT_SIDE \ 2
    Private Const PORT_LINE_LENGTH As Integer = 10

    Private Shared ReadOnly Context_Menu As New SWCT_Context_Menu()

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
            Me.Children.AddRange(Me.Configurations)
            Me.Children.AddRange(Me.Operations)
            Me.Children.AddRange(Me.Provider_Ports)
            Me.Children.AddRange(Me.Requirer_Ports)
        End If
        Return Me.Children
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Package).Component_Types.Remove(Me)
        CType(new_parent, Package).Component_Types.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Component_Types.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Component_Type.Metaclass_Name
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Component_Type.Context_Menu
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        For Each port In Me.Provider_Ports
            Dim sw_if As Software_Interface
            sw_if = CType(Me.Get_Element_From_Project_By_Identifier(port.Interface_Ref),
                Software_Interface)
            If Not IsNothing(sw_if) Then
                If Not Me.Needed_Elements.Contains(sw_if) Then
                    Me.Needed_Elements.Add(sw_if)
                End If
            End If
        Next
        For Each port In Me.Requirer_Ports
            Dim sw_if As Software_Interface
            sw_if = CType(Me.Get_Element_From_Project_By_Identifier(port.Interface_Ref),
                Software_Interface)
            If Not IsNothing(sw_if) Then
                If Not Me.Needed_Elements.Contains(sw_if) Then
                    Me.Needed_Elements.Add(sw_if)
                End If
            End If
        Next
        For Each conf In Me.Configurations
            Dim data_type As Type
            data_type = CType(Me.Get_Element_From_Project_By_Identifier(conf.Type_Ref), Type)
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

    Public Sub Add_Configuration()

        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Configuration_Parameter.Metaclass_Name,
            "",
            Configuration_Parameter.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Type",
            "",
            Me.Get_All_Types_Path_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim new_config As New Configuration_Parameter(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                Me.Get_Type_From_Project_By_Path(creation_form.Get_Ref_Element_Path()).Identifier)

            Me.Configurations.Add(new_config)
            Me.Children.Add(new_config)
            Me.Get_Project().Add_Element_To_Project(new_config)

            Me.Update_Views()

        End If

    End Sub

    Public Sub Add_OS_Operation()
        Dim creation_form As New Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            OS_Operation.Metaclass_Name,
            "",
            OS_Operation.Metaclass_Name,
            "",
            Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_op As New OS_Operation(
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

    Public Sub Add_Provider_Port()

        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Provider_Port.Metaclass_Name,
            "",
            Provider_Port.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Interface",
            "",
            Me.Get_All_Interfaces_Path_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_port As New Provider_Port(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                Me.Get_Interface_From_Project_By_Path(creation_form.Get_Ref_Element_Path()) _
                    .Identifier)
            Me.Provider_Ports.Add(new_port)
            Me.Children.Add(new_port)
            Me.Get_Project().Add_Element_To_Project(new_port)
            Me.Update_Views()
        End If

    End Sub

    Public Sub Add_Requirer_Port()

        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Requirer_Port.Metaclass_Name,
            "",
            Requirer_Port.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Interface",
            "",
            Me.Get_All_Interfaces_Path_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_port As New Requirer_Port(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                Me.Get_Interface_From_Project_By_Path(creation_form.Get_Ref_Element_Path()) _
                    .Identifier)
            Me.Requirer_Ports.Add(new_port)
            Me.Children.Add(new_port)
            Me.Get_Project().Add_Element_To_Project(new_port)
            Me.Update_Views()
        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String
        Dim x_pos As Integer = 0
        Dim y_pos As Integer = 0

        ' ---------------------------------------------------------------------------------------- '
        ' Compute Box width (it depends on the longuest line of the configurations compartment)
        ' Build the lines of the configurations compartment
        Dim config_lines As New List(Of String)
        For Each config In Me.Configurations
            Dim config_line As String = "+ " & config.Name & " : " & config.Get_Type_Name()
            config_lines.Add(config_line)
        Next
        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(config_lines, SVG_MIN_CHAR_PER_LINE)
        Dim box_width As Integer = Get_Box_Witdh(nb_max_char_per_line)

        ' ---------------------------------------------------------------------------------------- '
        ' Compute other boxes lines
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, nb_max_char_per_line)

        Dim op_lines As New List(Of String)
        For Each op In Me.Operations
            Dim op_line As String = "+ " & op.Name & "()"
            op_lines.Add(op_line)
        Next

        ' ---------------------------------------------------------------------------------------- '
        ' Compute box height
        ' do not count title lines
        Dim box_nb_line As Integer = split_description.Count + config_lines.Count + op_lines.Count
        Dim text_box_height As Integer = SVG_TITLE_HEIGHT + box_nb_line * SVG_TEXT_LINE_HEIGHT _
            + SVG_STROKE_WIDTH * 4 + SVG_VERTICAL_MARGIN * 3
        Dim port_box_height As Integer
        port_box_height = Math.Max(Me.Provider_Ports.Count, Me.Requirer_Ports.Count) * PORT_SPACE
        Dim box_height As Integer = Math.Max(text_box_height, port_box_height)

        ' ---------------------------------------------------------------------------------------- '
        ' Compute box x_pos offset (it depends on pport longuest name)
        Dim nb_char_offset = 0
        For Each pp In Me.Provider_Ports
            nb_char_offset = Math.Max(
                nb_char_offset,
                pp.Name.Length + pp.Get_Interface_Name().Length + 1)
        Next

        ' ---------------------------------------------------------------------------------------- '
        ' Add compartments and ports
        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        Dim rectangle_x_pos As Integer = x_pos
        If nb_char_offset <> 0 Then
            rectangle_x_pos += Get_Text_Witdh(nb_char_offset) + SVG_TEXT_MARGIN
        End If
        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(rectangle_x_pos, y_pos, Me.Name,
            Component_Type.SVG_COLOR, box_width, Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        If text_box_height < port_box_height Then
            desc_rect_height = Get_SVG_Retangle_Height(split_description.Count) _
                + (port_box_height - text_box_height)
        End If

        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            rectangle_x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Component_Type.SVG_COLOR,
            box_width,
            desc_rect_height)

        ' Add configurations compartement
        Dim conf_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            rectangle_x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            config_lines,
            Component_Type.SVG_COLOR,
            box_width,
            conf_rect_height)

        ' Add operations compartement
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            rectangle_x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height + conf_rect_height,
            op_lines,
            Component_Type.SVG_COLOR,
            box_width)

        ' Add ports
        Dim port_idx As Integer = 0
        Dim port_rect_x_pos As Integer = rectangle_x_pos - PORT_SIDE
        For Each pp In Me.Provider_Ports
            Dim port_y_pos As Integer = y_pos + PORT_SPACE \ 2 + port_idx * PORT_SPACE

            Me.SVG_Content &= Get_SVG_Rectangle(
                port_rect_x_pos,
                port_y_pos,
                PORT_SIDE,
                PORT_SIDE,
                Component_Type.SVG_COLOR,
                "0.6")

            Dim line_y_pos As Integer = port_y_pos + PORT_SIDE \ 2
            Me.SVG_Content &= Get_SVG_Horizontal_Line(
                port_rect_x_pos - PORT_LINE_LENGTH,
                line_y_pos,
                PORT_LINE_LENGTH,
                Component_Type.SVG_COLOR)

            Me.SVG_Content &= Get_SVG_Circle(
                port_rect_x_pos - PORT_LINE_LENGTH - LOLLIPOP_RADIUS,
                line_y_pos,
                LOLLIPOP_RADIUS,
                Component_Type.SVG_COLOR,
                "0.6")

            Me.SVG_Content &= Get_SVG_Text(
                rectangle_x_pos - SVG_TEXT_MARGIN,
                port_y_pos - SVG_VERTICAL_MARGIN,
                pp.Name & ":" & pp.Get_Interface_Name(),
                SVG_FONT_SIZE,
                False,
                False,
                E_Text_Anchor.ANCHOR_END)

            port_idx += 1
        Next

        port_idx = 0
        port_rect_x_pos = port_rect_x_pos + PORT_SIDE + box_width
        For Each rp In Me.Requirer_Ports
            Dim port_y_pos As Integer = y_pos + PORT_SPACE \ 2 + port_idx * PORT_SPACE

            Me.SVG_Content &= Get_SVG_Rectangle(
                port_rect_x_pos,
                port_y_pos,
                PORT_SIDE,
                PORT_SIDE,
                Component_Type.SVG_COLOR,
                "0.6")

            Dim line_y_pos As Integer = port_y_pos + PORT_SIDE \ 2
            Me.SVG_Content &= Get_SVG_Horizontal_Line(
                port_rect_x_pos + PORT_SIDE,
                line_y_pos,
                PORT_LINE_LENGTH,
                Component_Type.SVG_COLOR)

            Me.SVG_Content &= Get_SVG_Haf_Moon(
                port_rect_x_pos + PORT_SIDE + PORT_LINE_LENGTH,
                line_y_pos,
                LOLLIPOP_RADIUS,
                Component_Type.SVG_COLOR)

            Me.SVG_Content &= Get_SVG_Text(
                port_rect_x_pos + SVG_TEXT_MARGIN,
                port_y_pos - SVG_VERTICAL_MARGIN,
                rp.Name & ":" & rp.Get_Interface_Name(),
                SVG_FONT_SIZE,
                False,
                False)

            port_idx += 1
        Next

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Return Me.SVG_Content

    End Function

End Class


Public Class Configuration_Parameter
    Inherits Typed_Software_Element

    Public Const Metaclass_Name As String = "Configuration_Parameter"

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
        CType(Me.Owner, Component_Type).Configurations.Remove(Me)
        CType(new_parent, Component_Type).Configurations.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_swct As Component_Type = CType(Me.Owner, Component_Type)
        Me.Node.Remove()
        parent_swct.Configurations.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Configuration_Parameter.Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Component_Type
    End Function

End Class


Public Class OS_Operation
    Inherits Must_Describe_Software_Element

    Public Const Metaclass_Name As String = "OS_Operation"

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
        CType(Me.Owner, Component_Type).Operations.Remove(Me)
        CType(new_parent, Component_Type).Operations.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_swct As Component_Type = CType(Me.Owner, Component_Type)
        Me.Node.Remove()
        parent_swct.Operations.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return OS_Operation.Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Component_Type
    End Function

End Class


Public MustInherit Class Port
    Inherits Software_Element

    Public Interface_Ref As Guid


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
            interface_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Interface_Ref = interface_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Component_Type
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
            Me.Get_Forbidden_Name_List(),
            "Interface",
            Me.Get_Interface_Path(),
            Me.Get_All_Interfaces_Path_From_Project())
        Dim edition_form_result As DialogResult = edit_form.ShowDialog()
        If edition_form_result = DialogResult.OK Then
            Dim old_path As String = Me.Get_Path()
            Me.Name = edit_form.Get_Element_Name()
            Update_Project(old_path)
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Interface_Ref =
                Get_Interface_From_Project_By_Path(edit_form.Get_Ref_Element_Path()).Identifier
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
            Nothing, ' Forbidden name list, useless for View
            "Interface",
            Me.Get_Interface_Path(),
            Nothing)
        elmt_view_form.ShowDialog()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Specific methods
    '----------------------------------------------------------------------------------------------'

    Public Function Get_Interface_Name() As String
        Dim if_name As String = "unresolved"
        Dim sw_if As Software_Element = Me.Get_Element_From_Project_By_Identifier(Me.Interface_Ref)
        If Not IsNothing(sw_if) Then
            if_name = sw_if.Name
        End If
        Return if_name
    End Function

    Public Function Get_Interface_Path() As String
        Dim if_path As String = "unresolved"
        Dim sw_if As Software_Element = Me.Get_Element_From_Project_By_Identifier(Me.Interface_Ref)
        If Not IsNothing(sw_if) Then
            if_path = sw_if.Get_Path()
        End If
        Return if_path
    End Function

End Class


Public Class Provider_Port
    Inherits Port

    Public Const Metaclass_Name As String = "Provider_Port"


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
            interface_ref As Guid)
        MyBase.New(name, description, owner, parent_node, interface_ref)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Component_Type).Provider_Ports.Remove(Me)
        CType(new_parent, Component_Type).Provider_Ports.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_swct As Component_Type = CType(Me.Owner, Component_Type)
        Me.Node.Remove()
        parent_swct.Provider_Ports.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Provider_Port.Metaclass_Name
    End Function

End Class


Public Class Requirer_Port
    Inherits Port

    Public Const Metaclass_Name As String = "Requirer_Port"

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
            interface_ref As Guid)
        MyBase.New(name, description, owner, parent_node, interface_ref)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Component_Type).Requirer_Ports.Remove(Me)
        CType(new_parent, Component_Type).Requirer_Ports.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_swct As Component_Type = CType(Me.Owner, Component_Type)
        Me.Node.Remove()
        parent_swct.Requirer_Ports.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Requirer_Port.Metaclass_Name
    End Function

End Class