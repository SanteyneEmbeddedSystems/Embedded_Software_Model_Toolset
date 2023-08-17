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
        Dim creation_form As New Element_With_Ref_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Component_Prototype.Metaclass_Name,
            "",
            Component_Prototype.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Component_Type",
            "",
            Me.Get_All_Component_Types_From_Project())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_swc As New Component_Prototype(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element().Identifier)
            Me.Parts.Add(new_swc)
            Me.Children.Add(new_swc)
            Me.Get_Project().Add_Element_To_Project(new_swc)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Connector()

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
            parent_node As TreeNode)
        MyBase.New(name, description, owner, parent_node)
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