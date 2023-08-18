Public Class Composition
    Inherits Classifier

    Public Parts As New List(Of Component_Prototype)
    Public Links As New List(Of Connector)
    Public Tasks As New List(Of Composition_Task)

    Public Const Metaclass_Name As String = "Composition"

    Private Shared ReadOnly Context_Menu As New Composition_Context_Menu()


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
            Me.Children.AddRange(Me.Parts)
            Me.Children.AddRange(Me.Links)
            Me.Children.AddRange(Me.Tasks)
        End If
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
            swct = CType(Me.Get_Element_From_Project_By_Identifier(swc.Element_Ref),
                Component_Type)
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
            Component_Prototype.Metaclass_Name,
            "",
            Component_Prototype.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
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
                creation_form.Get_Ref_Element().Identifier)
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
            Connector.Metaclass_Name,
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
            "",
            Me.Get_Children_Name())
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

End Class


Public Class Component_Prototype
    Inherits Software_Element_Wih_Reference

    Public Const Metaclass_Name As String = "Component_Protoype"

    Public Configuration_Values As New List(Of Configuration_Value)

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
        MyBase.New(name, description, owner, parent_node, component_type_ref)
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
    ' Methods from Software_Element_Wih_Reference
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Function Get_Referenceable_Element_List() As List(Of Software_Element)
        Return Me.Get_All_Component_Types_From_Project()
    End Function

    Protected Overrides Function Get_Referenceable_Element_Kind() As String
        Return "Component_Type"
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
        swct = CType(Me.Get_Element_From_Project_By_Identifier(Me.Element_Ref), Component_Type)
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
            Me.Get_Metaclass_Name(),
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description,
            Me.Get_Forbidden_Name_List(),
            Me.Get_Referenced_Element_Path(),
            Me.Get_All_Component_Types_From_Project(),
            config_table)
        Dim edit_result As DialogResult
        edit_result = edit_form.ShowDialog()
        If edit_result = DialogResult.OK Then
            Me.Name = edit_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edit_form.Get_Element_Description()
            Me.Element_Ref = edit_form.Get_Ref_Element().Identifier
            Me.Update_Configurations_Value(edit_form.Get_Config_Data())
            Me.Update_Views()
        End If
    End Sub

    Public Overrides Sub view()
        Dim config_table As DataTable = Me.Create_Config_Data_Table()
        Dim view_form As New Component_Prototype_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Me.Get_Metaclass_Name(),
            Me.Identifier.ToString(),
            Me.Name,
            Me.Description,
            Nothing,
            Me.Get_Referenced_Element_Path(),
            Me.Get_All_Component_Types_From_Project(),
            config_table)
        view_form.ShowDialog()
    End Sub


End Class


Public Class Connector
    Inherits Software_Element

    Public Provider_Component_Ref As Guid
    Public Provider_Port_Ref As Guid
    Public Requirer_Component_Ref As Guid
    Public Requirer_Port_Ref As Guid

    Public Const Metaclass_Name As String = "Connector"


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
            Connector.Metaclass_Name,
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
            Connector.Metaclass_Name,
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

End Class


Public Class Composition_Task
    Inherits Must_Describe_Software_Element

    Public Const Metaclass_Name As String = "Task"


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

End Class