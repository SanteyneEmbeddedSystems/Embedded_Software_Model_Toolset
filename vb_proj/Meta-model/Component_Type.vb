Imports System.Math

Public Class Component_Type
    Inherits Classifier

    Public Configurations As New List(Of Configuration_Parameter)
    Public Operations As New List(Of OS_Operation)
    Public Provider_Ports As New List(Of Provider_Port)
    Public Requirer_Ports As New List(Of Requirer_Port)

    Public Const Metaclass_Name As String = "Component_Type"

    Public Const SVG_COLOR As String = "rgb(255,255,255)"
    Public Const PORT_SPACE As Integer = Port.PORT_SIDE + SVG_TEXT_LINE_HEIGHT _
        + SVG_VERTICAL_MARGIN * 3

    Private Shared ReadOnly Context_Menu As New SWCT_Context_Menu()
    Private Shared ReadOnly RO_Context_Menu As New Read_Only_SWCT_Context_Menu()

    Private Shared ReadOnly Nb_Ports_Rule As New Modeling_Rule(
        "Nb_Ports",
        "Shall aggregate at least one Port.")


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
        Me.Children.AddRange(Me.Configurations)
        Me.Children.AddRange(Me.Operations)
        Me.Children.AddRange(Me.Provider_Ports)
        Me.Children.AddRange(Me.Requirer_Ports)
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

    Protected Overrides Function Get_Read_Only_Context_Menu() As ContextMenuStrip
        Return Component_Type.RO_Context_Menu
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements.Clear()
        For Each port In Me.Provider_Ports
            Dim sw_if As Software_Interface
            sw_if = CType(Me.Get_Elmt_From_Prj_By_Id(port.Referenced_Interface_Id),
                Software_Interface)
            If Not IsNothing(sw_if) Then
                If Not Me.Needed_Elements.Contains(sw_if) Then
                    Me.Needed_Elements.Add(sw_if)
                End If
            End If
        Next
        For Each port In Me.Requirer_Ports
            Dim sw_if As Software_Interface
            sw_if = CType(Me.Get_Elmt_From_Prj_By_Id(port.Referenced_Interface_Id),
                Software_Interface)
            If Not IsNothing(sw_if) Then
                If Not Me.Needed_Elements.Contains(sw_if) Then
                    Me.Needed_Elements.Add(sw_if)
                End If
            End If
        Next
        For Each conf In Me.Configurations
            Dim data_type As Type
            data_type = CType(Me.Get_Elmt_From_Prj_By_Id(conf.Referenced_Type_Id), Type)
            If Not IsNothing(data_type) Then
                If Not Me.Needed_Elements.Contains(data_type) Then
                    Me.Needed_Elements.Add(data_type)
                End If
            End If
        Next
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Specific methods 
    ' -------------------------------------------------------------------------------------------- '

    Public Function Get_Port_By_Name(port_name As String) As Port
        For Each port In Me.Provider_Ports
            If port.Name = port_name Then
                Return port
            End If
        Next
        For Each port In Me.Requirer_Ports
            If port.Name = port_name Then
                Return port
            End If
        Next
        Return Nothing
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
            "Type",
            "",
            Me.Get_All_Types_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim new_config As New Configuration_Parameter(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element_Identifier())

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
            "")
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
            "Interface",
            "",
            Me.Get_All_Interfaces_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_port As New Provider_Port(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element_Identifier())
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
            "Interface",
            "",
            Me.Get_All_Interfaces_From_Project())

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then
            Dim new_port As New Requirer_Port(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element_Identifier())
            Me.Requirer_Ports.Add(new_port)
            Me.Children.Add(new_port)
            Me.Get_Project().Add_Element_To_Project(new_port)
            Me.Update_Views()
        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Def_Group() As String

        ' ---------------------------------------------------------------------------------------- '
        ' Compute Box width (it depends on the longuest line of the configurations compartment)
        ' Build the lines of the configurations compartment
        Dim config_lines As New List(Of String)
        For Each config In Me.Configurations
            Dim config_line As String = "+ " & config.Name & " : " &
                config.Get_Referenced_Type_Name()
            config_lines.Add(config_line)
        Next
        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(config_lines, SVG_MIN_CHAR_PER_LINE)
        Dim box_width As Integer = Get_Box_Width(nb_max_char_per_line)

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
        Dim box_nb_line As Integer =
            split_description.Count _
            + Max(config_lines.Count, 1) _
            + Max(op_lines.Count, 1)
        Dim text_box_height As Integer = SVG_TITLE_HEIGHT + box_nb_line * SVG_TEXT_LINE_HEIGHT _
            + SVG_STROKE_WIDTH * 4 + SVG_VERTICAL_MARGIN * 3
        Dim port_box_height As Integer
        port_box_height = (Max(Me.Provider_Ports.Count, Me.Requirer_Ports.Count) + 1) * PORT_SPACE
        Me.SVG_Height = Max(text_box_height, port_box_height)


        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' ---------------------------------------------------------------------------------------- '
        ' Add provider ports
        Dim max_width As Integer = 0
        For Each pp In Me.Provider_Ports
            Me.SVG_Content &= pp.Get_SVG_Def_Group()
            max_width = Max(max_width, pp.Get_SVG_Width())
        Next
        Dim port_idx As Integer = 0
        For Each pp In Me.Provider_Ports
            Dim port_x_pos As Integer = max_width - pp.Get_SVG_Width()
            Dim port_y_pos As Integer = PORT_SPACE \ 2 + port_idx * PORT_SPACE
            Me.SVG_Content &= "  <use xlink:href=""#" & pp.Get_SVG_Id() &
                                  """ transform=""translate(" & port_x_pos &
                                  "," & port_y_pos & ")"" />" & vbCrLf
            port_idx += 1
        Next

        ' ---------------------------------------------------------------------------------------- '
        ' Add compartments
        Dim rectangle_x_pos As Integer = max_width
        ' Add title (Name + stereotype) compartment
        Me.SVG_Content &= Get_Title_Rectangle(rectangle_x_pos, 0, Me.Name,
            Component_Type.SVG_COLOR, box_width, Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        If text_box_height < port_box_height Then
            desc_rect_height = Get_SVG_Rectangle_Height(split_description.Count) _
                + (port_box_height - text_box_height)
        End If

        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            rectangle_x_pos,
            SVG_TITLE_HEIGHT,
            split_description,
            Component_Type.SVG_COLOR,
            box_width,
            desc_rect_height)

        ' Add configurations compartement
        Dim conf_rect_height As Integer = 0
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            rectangle_x_pos,
            SVG_TITLE_HEIGHT + desc_rect_height,
            config_lines,
            Component_Type.SVG_COLOR,
            box_width,
            conf_rect_height)

        ' Add operations compartement
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            rectangle_x_pos,
            SVG_TITLE_HEIGHT + desc_rect_height + conf_rect_height,
            op_lines,
            Component_Type.SVG_COLOR,
            box_width)

        ' ---------------------------------------------------------------------------------------- '
        ' Add requirer ports
        max_width = 0
        port_idx = 0
        For Each rp In Me.Requirer_Ports
            Me.SVG_Content &= rp.Get_SVG_Def_Group()
            max_width = Max(max_width, rp.Get_SVG_Width())
            Dim port_x_pos As Integer = rectangle_x_pos + box_width
            Dim port_y_pos As Integer = PORT_SPACE \ 2 + port_idx * PORT_SPACE
            Me.SVG_Content &= "  <use xlink:href=""#" & rp.Get_SVG_Id() &
                                  """ transform=""translate(" & port_x_pos &
                                  "," & port_y_pos & ")"" />" & vbCrLf
            port_idx += 1
        Next

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Width = rectangle_x_pos + box_width + max_width

        Return Me.SVG_Content

    End Function

    Public Overrides Function Get_Alternative_SVG_Def_Group() As String
        Dim svg_content As String
        svg_content = Me.Get_SVG_Def_Group_Header(True)

        ' Add provided interfaces
        Dim added_interfaces_list As New List(Of Software_Element)
        Dim max_interface_width As Integer = 0
        Dim y_position As Integer = 0
        For Each pp In Me.Provider_Ports
            Dim sw_if As Software_Element = Me.Get_Elmt_From_Prj_By_Id(pp.Referenced_Interface_Id)
            If Not added_interfaces_list.Contains(sw_if) Then
                added_interfaces_list.Add(sw_if)
                svg_content &= sw_if.Get_SVG_Def_Group()
                max_interface_width = Max(max_interface_width, sw_if.Get_SVG_Width())
                svg_content &= "  <use xlink:href=""#" & sw_if.Get_SVG_Id() &
                           """ transform=""translate(" & 0 &
                           "," & y_position & ")"" />" & vbCrLf
                y_position += sw_if.Get_SVG_Height + SVG_BOX_MARGIN
            End If
        Next
        Me.Alt_SVG_Height = y_position - SVG_BOX_MARGIN

        ' Add Component_Type (Me)
        Dim swct_x_position As Integer = max_interface_width + SVG_BOX_MARGIN
        If max_interface_width = 0 Then
            swct_x_position = 0
        End If
        svg_content &= Me.Get_SVG_Def_Group()
        svg_content &= "  <use xlink:href=""#" & Me.Get_SVG_Id() &
                           """ transform=""translate(" & swct_x_position &
                           "," & 0 & ")"" />" & vbCrLf
        Me.Alt_SVG_Height = Max(Me.Alt_SVG_Height, Me.SVG_Height)

        ' Add required interfaces
        y_position = 0
        max_interface_width = 0
        Dim req_if_x_position As Integer = swct_x_position + Me.Get_SVG_Width() + SVG_BOX_MARGIN
        For Each rp In Me.Requirer_Ports
            Dim sw_if As Software_Element = Me.Get_Elmt_From_Prj_By_Id(rp.Referenced_Interface_Id)
            If Not added_interfaces_list.Contains(sw_if) Then
                added_interfaces_list.Add(sw_if)
                svg_content &= sw_if.Get_SVG_Def_Group()
                max_interface_width = Max(max_interface_width, sw_if.Get_SVG_Width())
                svg_content &= "  <use xlink:href=""#" & sw_if.Get_SVG_Id() &
                           """ transform=""translate(" & req_if_x_position &
                           "," & y_position & ")"" />" & vbCrLf
                y_position += sw_if.Get_SVG_Height + SVG_BOX_MARGIN
            End If
        Next

        Me.Alt_SVG_Height = Max(Me.Alt_SVG_Height, y_position - SVG_BOX_MARGIN)
        If max_interface_width <> 0 Then
            Me.Alt_SVG_Width = req_if_x_position + max_interface_width
        Else
            Me.Alt_SVG_Width = req_if_x_position - SVG_BOX_MARGIN
        End If

        svg_content &= Get_SVG_Def_Group_Footer()
        Return svg_content
    End Function

    Public Sub Show_Dependencies_On_Diagram()
        Me.Get_Project().Update_Alternative_Diagram(Me)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim nb_ports_check As New Consistency_Check_Report_Item(Me, Nb_Ports_Rule)
        report.Add_Item(nb_ports_check)
        nb_ports_check.Set_Compliance(Me.Provider_Ports.Count > 0 Or Me.Requirer_Ports.Count > 0)

    End Sub

End Class


Public Class Configuration_Parameter
    Inherits Typed_Element

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
            type_ref As Guid)
        MyBase.New(name, description, owner, parent_node, type_ref)
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

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of Configuration_Parameter) =
            CType(Me.Owner, Component_Type).Configurations
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


Public Class OS_Operation
    Inherits Described_Element

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

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of OS_Operation) = CType(Me.Owner, Component_Type).Operations
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


Public MustInherit Class Port
    Inherits Named_Element

    Public Referenced_Interface_Id As Guid

    Public Const PORT_SIDE As Integer = 16
    Protected Const LOLLIPOP_RADIUS As Integer = PORT_SIDE \ 2
    Protected Const PORT_LINE_LENGTH As Integer = 10
    Public Const PORT_BLOCK_WITDH As Integer = PORT_SIDE + 2 * LOLLIPOP_RADIUS + PORT_LINE_LENGTH

    Private Shared ReadOnly Interface_Rule As New Modeling_Rule(
        "Interface",
        "Shall reference one Interface.")


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
        Me.Referenced_Interface_Id = interface_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Component_Type
    End Function


    Protected Function Get_Referenced_Interface_Name() As String
        Return Me.Get_Elmt_Name_From_Proj_By_Id(Me.Referenced_Interface_Id)
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
            "Interface",
            Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Interface_Id),
            Me.Get_All_Interfaces_From_Project())

        Dim edition_form_result As DialogResult = edit_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Update Me
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Referenced_Interface_Id = edit_form.Get_Ref_Element_Identifier()

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
            "Interface",
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Referenced_Interface_Id),
            Nothing)
        elmt_view_form.ShowDialog()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim interface_check As New Consistency_Check_Report_Item(Me, Interface_Rule)
        report.Add_Item(interface_check)
        Dim sw_if As Software_Element = Me.Get_Elmt_From_Prj_By_Id(Me.Referenced_Interface_Id)
        interface_check.Set_Compliance(TypeOf sw_if Is Software_Interface)
    End Sub


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

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of Provider_Port) = CType(Me.Owner, Component_Type).Provider_Ports
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
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Def_Group() As String

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        Dim port_text As String = Me.Name & ":" & Me.Get_Referenced_Interface_Name()
        Dim port_text_x_pos As Integer = Get_Text_Width(port_text.Count) + SVG_TEXT_MARGIN
        Dim port_text_y_pos As Integer = SVG_TEXT_LINE_HEIGHT

        Me.SVG_Content &= Get_SVG_Text(
            port_text_x_pos,
            port_text_y_pos,
            port_text,
            SVG_FONT_SIZE,
            False,
            False,
            E_Text_Anchor.ANCHOR_END)

        Dim port_rect_y_pos As Integer = port_text_y_pos + SVG_VERTICAL_MARGIN
        Me.SVG_Content &= Get_SVG_Rectangle(
            port_text_x_pos,
            port_rect_y_pos,
            PORT_SIDE,
            PORT_SIDE,
            Component_Type.SVG_COLOR,
            "0.2")

        Dim line_y_pos As Integer = port_rect_y_pos + PORT_SIDE \ 2
        Me.SVG_Content &= Get_SVG_Horizontal_Line(
            port_text_x_pos - PORT_LINE_LENGTH,
            line_y_pos,
            PORT_LINE_LENGTH,
            Component_Type.SVG_COLOR)

        Me.SVG_Content &= Get_SVG_Circle(
            port_text_x_pos - PORT_LINE_LENGTH - LOLLIPOP_RADIUS,
            line_y_pos,
            LOLLIPOP_RADIUS,
            Component_Type.SVG_COLOR,
            "0.2")

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()

        Me.SVG_Width = port_text_x_pos + PORT_SIDE
        Me.SVG_Height = port_rect_y_pos + PORT_SIDE

        Return Me.SVG_Content
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

    Protected Overrides Function Move_In_My_Owner(offset As Integer) As Boolean
        Dim container As List(Of Requirer_Port) = CType(Me.Owner, Component_Type).Requirer_Ports
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
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Def_Group() As String

        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        Dim port_text As String = Me.Name & ":" & Me.Get_Referenced_Interface_Name()
        Dim port_text_y_pos As Integer = SVG_TEXT_LINE_HEIGHT

        Me.SVG_Content &= Get_SVG_Text(
                PORT_SIDE,
                port_text_y_pos,
                port_text,
                SVG_FONT_SIZE,
                False,
                False)

        Dim port_rect_y_pos As Integer = port_text_y_pos + SVG_VERTICAL_MARGIN
        Me.SVG_Content &= Get_SVG_Rectangle(
                0,
                port_rect_y_pos,
                PORT_SIDE,
                PORT_SIDE,
                Component_Type.SVG_COLOR,
                "0.2")

        Dim line_y_pos As Integer = port_rect_y_pos + PORT_SIDE \ 2
        Me.SVG_Content &= Get_SVG_Horizontal_Line(
                PORT_SIDE,
                line_y_pos,
                PORT_LINE_LENGTH,
                Component_Type.SVG_COLOR)

        Me.SVG_Content &= Get_SVG_Half_Moon(
                PORT_SIDE + PORT_LINE_LENGTH,
                line_y_pos,
                LOLLIPOP_RADIUS,
                Component_Type.SVG_COLOR)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()

        Me.SVG_Width = PORT_SIDE + Get_Text_Width(port_text.Count)
        Me.SVG_Height = port_rect_y_pos + PORT_SIDE

        Return Me.SVG_Content
    End Function

End Class