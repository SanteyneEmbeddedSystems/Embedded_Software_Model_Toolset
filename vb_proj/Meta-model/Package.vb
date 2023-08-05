Imports System.Xml.Serialization

Public Class Package
    Inherits Software_Element

    Public Packages As New List(Of Package)

    <XmlArrayItemAttribute(GetType(Basic_Integer_Type)),
     XmlArrayItemAttribute(GetType(Basic_Boolean_Type)),
     XmlArrayItemAttribute(GetType(Basic_Floating_Point_Type)),
     XmlArrayItemAttribute(GetType(Array_Type)),
     XmlArrayItemAttribute(GetType(Enumerated_Type)),
     XmlArray("Types")>
    Public Types As New List(Of Type)

    Private Shared Context_Menu As New Package_Context_Menu()

    Public Shared ReadOnly Metaclass_Name As String = "Package"


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
        Me.Packages = New List(Of Package)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Function Get_Children() As List(Of Software_Element)
        If Me.Children_Is_Computed = False Then
            Me.Children_Is_Computed = True
            Me.Children.AddRange(Me.Packages)
            Me.Children.AddRange(Me.Types)
        End If
        Return Me.Children
    End Function

    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Package",
            .SelectedImageKey = "Package",
            .ContextMenuStrip = Package.Context_Menu,
            .Tag = Me}
    End Sub

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Dim is_allowed As Boolean = False
        If parent.GetType() = GetType(Top_Level_Package) _
            Or parent.GetType() = GetType(Package) Then
            is_allowed = True
        End If
        Return is_allowed
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Package).Packages.Remove(Me)
        CType(new_parent, Package).Packages.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Packages.Remove(Me)
    End Sub

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Package.Context_Menu
    End Function

    Public Overrides Function Get_Metaclass_Name() As String
        Return Package.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Add_Package()
        Dim creation_form As New Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Package.Metaclass_Name,
            "",
            Package.Metaclass_Name,
            "",
            Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_pkg As New Package(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Packages.Add(new_pkg)
            Me.Children.Add(new_pkg)
            Me.Display_Package_Modified()
        End If
    End Sub

    Public Sub Add_Array_Type()
        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(type_list)

        ' Display a creation form
        Dim creation_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Array_Type.Metaclass_Name,
            "",
            Array_Type.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "Base Type",
            type_by_path_dict.Keys(0),
            type_by_path_dict.Keys.ToList(),
            Array_Type.Multiplicity_Minimum_Value.ToString())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        ' Treat creation form result
        If creation_form_result = DialogResult.OK Then

            ' Get the type referenced by the array
            Dim ref_type As Software_Element = Nothing
            ref_type = type_by_path_dict(creation_form.Get_Ref_Rerenced_Element_Path())

            ' Create the array type
            Dim new_array_type As New Array_Type(
                    creation_form.Get_Element_Name(),
                    creation_form.Get_Element_Description(),
                    Me,
                    Me.Node,
                    CUInt(creation_form.Get_Multiplicity()),
                    ref_type.UUID)

            ' Add array type to its package
            Me.Types.Add(new_array_type)
            Me.Children.Add(new_array_type)

            Me.Display_Package_Modified()
        End If

    End Sub

    Public Sub Add_Enumerated_Type()

        Dim enumerals_table As New DataTable
        With enumerals_table
            .Columns.Add("Name", GetType(String))
            .Columns.Add("Description", GetType(String))
        End With

        Dim creation_form As New Enumerated_Type_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Enumerated_Type.Metaclass_Name,
            "",
            Enumerated_Type.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            enumerals_table)

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_enumeration As New Enumerated_Type(
                    creation_form.Get_Element_Name(),
                    creation_form.Get_Element_Description(),
                    Me,
                    Me.Node,
                    enumerals_table)
            Me.Types.Add(new_enumeration)
            Me.Children.Add(new_enumeration)
            Me.Display_Package_Modified()
        End If
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for model management
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Complete_Type_List(ByRef type_list As List(Of Type))
        type_list.AddRange(Me.Types)
        For Each pkg In Me.Packages
            pkg.Complete_Type_List(type_list)
        Next
    End Sub

End Class