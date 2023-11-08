Public Class Composition
    Inherits Classifier

    Public Parts As New List(Of Component_Prototype)
    Public Links As New List(Of Connector)
    Public Tasks As New List(Of Composition_Task)

    Public Const Metaclass_Name As String = "Composition"

    Private Shared ReadOnly Context_Menu As New Composition_Context_Menu()

    Private Shared ReadOnly Parts_Rule As New Modeling_Rule(
        "Parts",
        "Shall Aggregate at least two Component_Prototypes.")
    Private Shared ReadOnly Links_Rule As New Modeling_Rule(
        "Links",
        "Shall Aggregate at least one Connextor.")
    Private Shared ReadOnly Tasks_Rule As New Modeling_Rule(
        "Tasks",
        "Shall Aggregate at least one Task.")


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
        Me.Children.AddRange(Me.Parts)
        Me.Children.AddRange(Me.Links)
        Me.Children.AddRange(Me.Tasks)
        Return Me.Children
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Package).Compositions.Remove(Me)
        CType(new_parent, Package).Compositions.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Compositions.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Composition.Metaclass_Name
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Composition.Context_Menu
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Classifier
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier)
        Me.Needed_Elements.Clear()
        For Each swc In Me.Parts
            Dim swct As Component_Type
            swct = CType(Me.Get_Elmt_From_Prj_By_Id(swc.Ref_Component_Type_Id), Component_Type)
            If Not IsNothing(swct) Then
                If Not Me.Needed_Elements.Contains(swct) Then
                    Me.Needed_Elements.Add(swct)
                End If
            End If
        Next
        Return Me.Needed_Elements
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Prototype()
        Dim creation_form As New Component_Prototype_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            "",
            Component_Prototype.Metaclass_Name,
            "",
            "",
            Me.Get_All_Component_Types_From_Project(),
            Nothing)
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_swc As New Component_Prototype(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element_Identifier())
            new_swc.Update_Configurations_Value(creation_form.Get_Config_Data())
            Me.Parts.Add(new_swc)
            Me.Children.Add(new_swc)
            Me.Get_Project().Add_Element_To_Project(new_swc)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Connector()
        Dim creation_form As New Connector_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            "",
            Connector.Metaclass_Name,
            "",
            Me.Parts,
            "",
            "",
            "",
            "")
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_connector As New Connector(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Provider_Swc_Identifier(),
                creation_form.Get_Provider_Port_Identifier(),
                creation_form.Get_Requirer_Swc_Identifier(),
                creation_form.Get_Requirer_Port_Identifier())
            Me.Links.Add(new_connector)
            Me.Children.Add(new_connector)
            Me.Get_Project().Add_Element_To_Project(new_connector)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Task()
        Dim creation_form As New Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Composition_Task.Metaclass_Name,
            "",
            Composition_Task.Metaclass_Name,
            "")
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_task As New Composition_Task(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Tasks.Add(new_task)
            Me.Children.Add(new_task)
            Me.Get_Project().Add_Element_To_Project(new_task)
            Me.Update_Views()
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim parts_check As New Consistency_Check_Report_Item(Me, Parts_Rule)
        report.Add_Item(parts_check)
        parts_check.Set_Compliance(Me.Parts.Count >= 2)
        Dim links_check As New Consistency_Check_Report_Item(Me, Links_Rule)
        report.Add_Item(links_check)
        links_check.Set_Compliance(Me.Links.Count >= 1)
        Dim tasks_check As New Consistency_Check_Report_Item(Me, Tasks_Rule)
        report.Add_Item(tasks_check)
        tasks_check.Set_Compliance(Me.Tasks.Count >= 1)
    End Sub

End Class


Public Class Component_Prototype
    Inherits Described_Element

    Public Ref_Component_Type_Id As Guid

    Public Const Metaclass_Name As String = "Component_Prototype"

    Public Configuration_Values As New List(Of Configuration_Value)

    Private Const SVG_COLOR As String = "rgb(127,127,127)"

    Private Const SVG_CONNECTOR_WIDTH = 600 ' ~ 4 * Get_Text_Width(NB_CHARS_MAX_FOR_SYMBOL)
    Private Const SVG_SWC_MARGIN = 40

    Private Shared ReadOnly List_Of_Configurations_Rule As New Modeling_Rule(
        "List_Of_Configurations",
        "Shall aggregate a Configuration_Value for each Configuration_Parameter aggregated by
        the referenced Component_Type.")

    Private Shared ReadOnly Configuration_Rule As New Modeling_Rule(
            "Configuration",
            "Configuration_Value shall reference one Configuration_Parameter.")

    Private Shared ReadOnly Value_Content_Rule As New Modeling_Rule(
            "Value_Content",
            "Value of Configuration_Value shall be compatible with the Type of the referenced
            Configuration_Parameter.")

    Private Shared ReadOnly Configuration_Parent_Rule As New Modeling_Rule(
            "Configuration_Parent",
            "Configuration_Parameter referenced by Configuration_Value shall belong to the
            referenced Component_Type.")


    Public Class Configuration_Value

        Public Configuration_Ref As Guid = Nothing
        Public Value As String

        Public Sub New()
        End Sub

        Public Sub New(conf_ref As Guid, value As String)
            Me.Configuration_Ref = conf_ref
            Me.Value = value
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
            component_type_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Ref_Component_Type_Id = component_type_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Composition).Parts.Remove(Me)
        CType(new_parent, Composition).Parts.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Me.Node.Remove()
        CType(Me.Owner, Composition).Parts.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Composition
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Specific methods
    ' -------------------------------------------------------------------------------------------- '

    Public Shared Function Create_Config_Data_Table(swct As Component_Type) As DataTable
        Dim config_table As New DataTable
        With config_table
            .Columns.Add("Configuration", GetType(String))
            .Columns.Add("Value", GetType(String))
            .Columns.Add("Reserved", GetType(Configuration_Parameter))
        End With
        If Not IsNothing(swct) Then
            For Each config In swct.Configurations
                config_table.Rows.Add(config.Name, "", config)
            Next
        End If
        Return config_table
    End Function

    Public Function Create_Config_Data_Table() As DataTable
        Dim swct As Component_Type
        swct = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Ref_Component_Type_Id), Component_Type)
        Dim config_table As New DataTable
        With config_table
            .Columns.Add("Configuration", GetType(String))
            .Columns.Add("Value", GetType(String))
            .Columns.Add("Reserved", GetType(Configuration_Parameter))
        End With
        If Not IsNothing(swct) Then
            For Each config In swct.Configurations
                config_table.Rows.Add(config.Name, Me.Get_Config_Value(config.Identifier), config)
            Next
        End If
        Return config_table
    End Function

    Public Sub Update_Configurations_Value(config_table As DataTable)
        Me.Configuration_Values.Clear()
        Dim row As DataRow
        For Each row In config_table.Rows
            Dim conf_id As Guid
            If Not IsDBNull(row("Reserved")) Then
                conf_id = CType(row("Reserved"), Configuration_Parameter).Identifier
            End If
            Dim value As String = ""
            If Not IsDBNull(row("Value")) Then
                value = CStr(row("Value"))
            End If
            Me.Configuration_Values.Add(New Configuration_Value(conf_id, value))
        Next
    End Sub

    Public Function Get_Config_Value(config_param_id As Guid) As String
        For Each config In Me.Configuration_Values
            If config.Configuration_Ref = config_param_id Then
                Return config.Value
            End If
        Next
        Return "unknown"
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()
        Dim config_table As DataTable = Me.Create_Config_Data_Table()
        Dim edit_form As New Component_Prototype_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description,
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Ref_Component_Type_Id),
            Me.Get_All_Component_Types_From_Project(),
            config_table)
        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Ref_Component_Type_Id = edit_form.Get_Ref_Element_Identifier()
            Me.Update_Configurations_Value(edit_form.Get_Config_Data())
            Me.Update_Views()
        End If
    End Sub

    Public Overrides Sub View()
        Dim config_table As DataTable = Me.Create_Config_Data_Table()
        Dim view_form As New Component_Prototype_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description,
            Me.Get_Elmt_Path_From_Proj_By_Id(Me.Ref_Component_Type_Id),
            Me.Get_All_Component_Types_From_Project(),
            config_table)
        view_form.ShowDialog()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String

        Dim compo As Composition = CType(Me.Owner, Composition)

        ' ---------------------------------------------------------------------------------------- '
        ' Compute requirer swc (Me) Box width (it depends on the longuest line of the
        ' configurations values compartment)
        ' Build the lines of the configurations values compartment
        Dim config_lines As New List(Of String)
        For Each config_val In Me.Configuration_Values
            Dim config_param As Configuration_Parameter = CType(
                Me.Get_Elmt_From_Prj_By_Id(config_val.Configuration_Ref),
                Configuration_Parameter)
            config_lines.Add("+ " & config_param.Name & " = " & config_val.Value)
        Next
        Dim nb_max_char_per_line As Integer
        nb_max_char_per_line = Get_Max_Nb_Of_Char_Per_Line(config_lines, SVG_MIN_CHAR_PER_LINE)
        Dim req_swc_box_width As Integer = Get_Box_Width(nb_max_char_per_line)


        ' ---------------------------------------------------------------------------------------- '
        ' Get the list of provider Component_Prototypes and Portss to draw
        Dim prov_swc_list As New List(Of Component_Prototype)
        Dim prov_port_list_by_swc As New Dictionary(Of Component_Prototype, List(Of Provider_Port))
        Dim req_port_list_by_swc As New Dictionary(Of Component_Prototype, List(Of Requirer_Port))
        For Each link In compo.Links
            If link.Requirer_Component_Ref = Me.Identifier Then
                Dim prov_swc As Component_Prototype
                prov_swc = CType(Me.Get_Elmt_From_Prj_By_Id(link.Provider_Component_Ref),
                    Component_Prototype)
                Dim prov_port As Provider_Port
                prov_port = CType(Me.Get_Elmt_From_Prj_By_Id(link.Provider_Port_Ref),
                    Provider_Port)
                Dim req_port As Requirer_Port
                req_port = CType(Me.Get_Elmt_From_Prj_By_Id(link.Requirer_Port_Ref),
                    Requirer_Port)
                If Not prov_port_list_by_swc.ContainsKey(prov_swc) Then
                    prov_port_list_by_swc.Add(prov_swc, New List(Of Provider_Port))
                    req_port_list_by_swc.Add(prov_swc, New List(Of Requirer_Port))
                End If
                prov_port_list_by_swc(prov_swc).Add(prov_port)
                req_port_list_by_swc(prov_swc).Add(req_port)
                If Not prov_swc_list.Contains(prov_swc) Then
                    prov_swc_list.Add(prov_swc)
                End If
            End If
        Next

        ' ---------------------------------------------------------------------------------------- '
        Dim split_description As List(Of String)
        Dim desc_rect_height As Integer = 0
        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()
        ' Draw provider Component_Prototypes
        Dim prov_swc_box_width As Integer = Get_Box_Width(SVG_MIN_CHAR_PER_LINE)
        Dim prov_swc_x_pos As Integer = req_swc_box_width + SVG_CONNECTOR_WIDTH _
            + 2 * Port.PORT_BLOCK_WITDH
        Me.SVG_Width = prov_swc_x_pos + prov_swc_box_width
        Dim prov_swc_y_pos As Integer = 0
        For Each swc In prov_swc_list
            Dim swc_height As Integer
            split_description = Split_String(swc.Description, SVG_MIN_CHAR_PER_LINE)
            Dim text_box_height As Integer = SVG_TITLE_HEIGHT _
                + split_description.Count * SVG_TEXT_LINE_HEIGHT _
                + SVG_STROKE_WIDTH + SVG_VERTICAL_MARGIN
            Dim port_box_height As Integer
            port_box_height = (prov_port_list_by_swc(swc).Count + 1) * Component_Type.PORT_SPACE
            swc_height = Math.Max(text_box_height, port_box_height)

            ' Draw title
            Me.SVG_Content &= Get_Title_Rectangle(prov_swc_x_pos, prov_swc_y_pos, swc.Name,
                SVG_COLOR, prov_swc_box_width, Metaclass_Name)

            ' Draw description compartment
            If text_box_height < port_box_height Then
                desc_rect_height = Get_SVG_Rectangle_Height(split_description.Count) _
                + (port_box_height - text_box_height)
            End If
            Me.SVG_Content &= Get_Multi_Line_Rectangle(
                prov_swc_x_pos,
                prov_swc_y_pos + SVG_TITLE_HEIGHT,
                split_description,
                SVG_COLOR,
                prov_swc_box_width,
                desc_rect_height)

            ' Draw ports and connectors
            Dim port_idx As Integer = 0
            For Each pp In prov_port_list_by_swc(swc)
                ' Draw provider port
                Me.SVG_Content &= pp.Compute_SVG_Content()
                Dim port_x_pos As Integer = prov_swc_x_pos - pp.Get_SVG_Width()
                Dim port_y_pos As Integer = prov_swc_y_pos + Component_Type.PORT_SPACE \ 2 _
                    + port_idx * Component_Type.PORT_SPACE
                Me.SVG_Content &= "  <use xlink:href=""#" & pp.Get_SVG_Id() &
                                      """ transform=""translate(" & port_x_pos &
                                      "," & port_y_pos & ")"" />" & vbCrLf

                ' Draw requirer port
                Dim rp As Requirer_Port = req_port_list_by_swc(swc).Item(port_idx)
                Me.SVG_Content &= rp.Compute_SVG_Content()
                Me.SVG_Content &= "  <use xlink:href=""#" & rp.Get_SVG_Id() &
                                      """ transform=""translate(" & req_swc_box_width &
                                      "," & port_y_pos & ")"" />" & vbCrLf

                ' Draw connector
                Me.SVG_Content &= Get_SVG_Horizontal_Line(
                    req_swc_box_width + Port.PORT_BLOCK_WITDH,
                    port_y_pos + SVG_TEXT_LINE_HEIGHT + SVG_VERTICAL_MARGIN + Port.PORT_SIDE \ 2,
                    SVG_CONNECTOR_WIDTH,
                    SVG_COLOR)

                port_idx += 1
            Next

            prov_swc_y_pos += swc_height + SVG_SWC_MARGIN
        Next


        ' ---------------------------------------------------------------------------------------- '
        ' Draw requirer Component_Prototype (Me)
        ' The minimum height of the box of the requirer swc (Me) shall be the y position of the last
        ' provider swc + its height
        Dim req_swc_min_height As Integer = prov_swc_y_pos - SVG_SWC_MARGIN

        ' Add title
        Me.SVG_Content &= Get_Title_Rectangle(0, 0, Me.Name,
                SVG_COLOR, req_swc_box_width, Metaclass_Name)

        ' Add description compartment
        split_description = Split_String(Me.Description, nb_max_char_per_line)
        Dim req_swc_nominal_height = SVG_TITLE_HEIGHT _
            + Get_SVG_Rectangle_Height(split_description.Count) _
            + Get_SVG_Rectangle_Height(config_lines.Count)

        If req_swc_min_height > req_swc_nominal_height Then
            Me.SVG_Height = req_swc_min_height
            desc_rect_height = Get_SVG_Rectangle_Height(split_description.Count) _
                + req_swc_min_height - req_swc_nominal_height
        Else
            Me.SVG_Height = req_swc_nominal_height
            desc_rect_height = 0
        End If
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT,
            split_description,
            SVG_COLOR,
            req_swc_box_width,
            desc_rect_height)

        ' Add configuration values compartment
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            SVG_TITLE_HEIGHT + desc_rect_height,
            config_lines,
            SVG_COLOR,
            req_swc_box_width)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()

        Return Me.SVG_Content
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim list_of_conf_check As New Consistency_Check_Report_Item(Me, List_Of_Configurations_Rule)
        report.Add_Item(list_of_conf_check)
        Dim swct As Component_Type
        swct = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Ref_Component_Type_Id), Component_Type)
        If IsNothing(swct) Then
            list_of_conf_check.Set_Status(Consistency_Check_Report_Item.Test_Status.NOT_AVAILABLE)
            list_of_conf_check.Set_Message("Component_Type not found.")
        Else
            Dim message As String = "Value not found for : "
            Dim all_found As Boolean = True
            For Each conf_param In swct.Configurations
                Dim found As Boolean = False
                For Each conf_value In Me.Configuration_Values
                    If conf_value.Configuration_Ref = conf_param.Identifier Then
                        found = True
                        Exit For
                    End If
                Next
                If found = False Then
                    all_found = False
                    list_of_conf_check.Set_Compliance(False)
                    message &= conf_param.Name & " "
                End If
            Next
            If all_found = False Then
                list_of_conf_check.Set_Message(message)
            Else
                list_of_conf_check.Set_Compliance(True)
            End If
        End If

        Dim conf_check As New Consistency_Check_Report_Item(Me, Configuration_Rule)
        report.Add_Item(conf_check)

        Dim value_check As New Consistency_Check_Report_Item(Me, Value_Content_Rule)
        report.Add_Item(value_check)
        Dim value_check_message As String = "Configurations with wrong value : "
        Dim all_values_ok As Boolean = True

        Dim conf_parent_check As New Consistency_Check_Report_Item(Me, Configuration_Parent_Rule)
        report.Add_Item(conf_parent_check)

        For Each conf_value In Me.Configuration_Values
            ' Look for the referenced Configuration_Parameter
            Dim ref_conf_param As Software_Element
            ref_conf_param = Owner.Get_Elmt_From_Prj_By_Id(conf_value.Configuration_Ref)

            If TypeOf ref_conf_param Is Configuration_Parameter Then
                conf_check.Set_Compliance(True)

                ' Look for the type of the referenced Configuration_Parameter
                Dim conf_param As Configuration_Parameter =
                    CType(ref_conf_param, Configuration_Parameter)
                Dim conf_type As Type =
                    CType(Owner.Get_Elmt_From_Prj_By_Id(conf_param.Referenced_Type_Id), Type)

                If Not IsNothing(conf_type) Then
                    ' Check value of configurations
                    If conf_type.Is_Value_Valid(conf_value.Value) Then
                        value_check.Set_Compliance(True)
                    Else
                        value_check.Set_Compliance(False)
                        value_check_message &= ref_conf_param.Name & " "
                        all_values_ok = False
                    End If

                    ' Check parent of configuration
                    conf_parent_check.Set_Compliance(conf_param.Get_Owner() Is swct)
                End If

            Else
                conf_check.Set_Compliance(False)
            End If
        Next

        If all_values_ok = False Then
            value_check.Set_Message(value_check_message)
        End If

    End Sub

End Class


Public Class Connector
    Inherits Software_Element

    Public Provider_Component_Ref As Guid
    Public Provider_Port_Ref As Guid
    Public Requirer_Component_Ref As Guid
    Public Requirer_Port_Ref As Guid

    Public Const Metaclass_Name As String = "Connector"

    Private Shared ReadOnly If_Consistency_Rule As New Modeling_Rule(
        "Interface_Consistency",
        "Shall connect 2 ports referencing the same Interface.")

    Private Shared ReadOnly Components_Different_Rule As New Modeling_Rule(
        "Components_Different",
        "Shall connect different Component_Prototypes.")


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
            prov_swc_ref As Guid,
            prov_port_ref As Guid,
            req_swc_ref As Guid,
            req_port_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Provider_Component_Ref = prov_swc_ref
        Me.Provider_Port_Ref = prov_port_ref
        Me.Requirer_Component_Ref = req_swc_ref
        Me.Requirer_Port_Ref = req_port_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Composition).Links.Remove(Me)
        CType(new_parent, Composition).Links.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Me.Node.Remove()
        CType(Me.Owner, Composition).Links.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Composition
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()
        Dim edition_form As New Connector_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            CType(Me.Owner, Composition).Parts,
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Provider_Component_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Provider_Port_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Requirer_Component_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Requirer_Port_Ref))
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()
        If edition_form_result = DialogResult.OK Then
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Provider_Component_Ref = edition_form.Get_Provider_Swc_Identifier()
            Me.Provider_Port_Ref = edition_form.Get_Provider_Port_Identifier()
            Me.Requirer_Component_Ref = edition_form.Get_Requirer_Swc_Identifier()
            Me.Requirer_Port_Ref = edition_form.Get_Requirer_Port_Identifier()
            Me.Update_Views()
        End If
    End Sub

    Public Overrides Sub View()
        Dim view_form As New Connector_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description,
            CType(Me.Owner, Composition).Parts,
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Provider_Component_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Provider_Port_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Requirer_Component_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Requirer_Port_Ref))
        view_form.ShowDialog()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim test_is_feasable As Boolean = True
        Dim if_consistency_check As New Consistency_Check_Report_Item(Me, If_Consistency_Rule)
        report.Add_Item(if_consistency_check)
        Dim prov_port As Provider_Port
        prov_port = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Provider_Port_Ref), Provider_Port)
        Dim req_port As Requirer_Port
        req_port = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Requirer_Port_Ref), Requirer_Port)
        If IsNothing(prov_port) Then
            if_consistency_check.Set_Status(Consistency_Check_Report_Item.Test_Status.NOT_AVAILABLE)
            if_consistency_check.Set_Message("Provider_Port is not found.")
            test_is_feasable = False
        Else
            If prov_port.Referenced_Interface_Id = Guid.Empty Then
                if_consistency_check.Set_Status(
                    Consistency_Check_Report_Item.Test_Status.NOT_AVAILABLE)
                if_consistency_check.Set_Message("Provider_Port has not Interface.")
                test_is_feasable = False
            End If
        End If
        If IsNothing(req_port) Then
            if_consistency_check.Set_Status(Consistency_Check_Report_Item.Test_Status.NOT_AVAILABLE)
            if_consistency_check.Set_Message("Requirer_Port is not found.")
            test_is_feasable = False
        Else
            If req_port.Referenced_Interface_Id = Guid.Empty Then
                if_consistency_check.Set_Status(
                    Consistency_Check_Report_Item.Test_Status.NOT_AVAILABLE)
                if_consistency_check.Set_Message("Requirer_Port has not Interface.")
                test_is_feasable = False
            End If
        End If
        If test_is_feasable Then
            if_consistency_check.Set_Compliance(
                prov_port.Referenced_Interface_Id = req_port.Referenced_Interface_Id)
        End If

        Dim swc_diff_check As New Consistency_Check_Report_Item(Me, Components_Different_Rule)
        report.Add_Item(swc_diff_check)
        swc_diff_check.Set_Compliance(Me.Provider_Component_Ref <> Me.Requirer_Component_Ref)

    End Sub

End Class


Public Class Composition_Task
    Inherits Described_Element

    Public Calls As New List(Of Call_OS_Operation)

    Public Const Metaclass_Name As String = "Task"

    Private Shared ReadOnly Context_Menu As New Task_Context_Menu()

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
        Me.Children.AddRange(Me.Calls)
        Return Me.Children
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Composition).Tasks.Remove(Me)
        CType(new_parent, Composition).Tasks.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Me.Node.Remove()
        CType(Me.Owner, Composition).Tasks.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Composition
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Composition_Task.Context_Menu
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Call_OS_Operation()
        Dim creation_form As New Call_OS_Operation_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            "",
            Call_OS_Operation.Metaclass_Name,
            "",
            CType(Me.Owner, Composition).Parts,
            "",
            "",
            "")
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_call As New Call_OS_Operation(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Component_Prototype_Identifier(),
                creation_form.Get_OS_Operation_Identifier(),
                creation_form.Get_Priority())
            Me.Calls.Add(new_call)
            Me.Children.Add(new_call)
            Me.Get_Project().Add_Element_To_Project(new_call)
            Me.Update_Views()
        End If
    End Sub


End Class


Public Class Call_OS_Operation
    Inherits Software_Element

    Public Component_Prototype_Ref As Guid
    Public OS_Operation_Ref As Guid
    Public Priority As UInteger

    Public Const Metaclass_Name As String = "Call_OS_Operation"

    Private Shared ReadOnly Operation_Owner_Rule As New Modeling_Rule(
        "Operation_Owner",
        "The referenced operation shall belong to the referenced component.")

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
            component_ref As Guid,
            operation_ref As Guid,
            priority As UInteger)
        MyBase.New(name, description, owner, parent_node)
        Me.Component_Prototype_Ref = component_ref
        Me.OS_Operation_Ref = operation_ref
        Me.Priority = priority
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Composition_Task).Calls.Remove(Me)
        CType(new_parent, Composition_Task).Calls.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Me.Node.Remove()
        CType(Me.Owner, Composition_Task).Calls.Remove(Me)
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Metaclass_Name
    End Function

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Return TypeOf parent Is Composition_Task
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()
        Dim edition_form As New Call_OS_Operation_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            CType(Me.Owner.Get_Owner(), Composition).Parts,
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Component_Prototype_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.OS_Operation_Ref),
            Me.Priority.ToString)
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()
        If edition_form_result = DialogResult.OK Then
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Component_Prototype_Ref = edition_form.Get_Component_Prototype_Identifier()
            Me.OS_Operation_Ref = edition_form.Get_OS_Operation_Identifier()
            Me.Priority = edition_form.Get_Priority()
            Me.Update_Views()
        End If
    End Sub

    Public Overrides Sub View()
        Dim view_form As New Call_OS_Operation_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Identifier.ToString,
            Me.Name,
            Me.Description,
            CType(Me.Owner.Get_Owner(), Composition).Parts,
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.Component_Prototype_Ref),
            Me.Get_Elmt_Name_From_Proj_By_Id(Me.OS_Operation_Ref),
            Me.Priority.ToString)
        Dim edition_form_result As DialogResult = view_form.ShowDialog()
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    '----------------------------------------------------------------------------------------------'

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)

        Dim op_owner_check As New Consistency_Check_Report_Item(Me, Operation_Owner_Rule)
        report.Add_Item(op_owner_check)
        If Me.OS_Operation_Ref <> Guid.Empty Then
            Dim swc As Component_Prototype
            swc = CType(Me.Get_Elmt_From_Prj_By_Id(Me.Component_Prototype_Ref), Component_Prototype)
            If Not IsNothing(swc) Then
                Dim swct As Component_Type
                swct = CType(Me.Get_Elmt_From_Prj_By_Id(swc.Ref_Component_Type_Id), Component_Type)
                If Not IsNothing(swct) Then
                    Dim found As Boolean = False
                    For Each op In swct.Operations
                        If op.Identifier = Me.OS_Operation_Ref Then
                            found = True
                            Exit For
                        End If
                    Next
                    op_owner_check.Set_Compliance(found)
                End If
            Else
                op_owner_check.Set_Status(Consistency_Check_Report_Item.Test_Status.NOT_AVAILABLE)
                op_owner_check.Set_Message("Component_Prototype not found.")
            End If
        Else
            ' TODO
        End If

    End Sub

End Class