Imports System.Xml.Serialization

Public Class Package
    Inherits Must_Describe_Software_Element

    Public Packages As New List(Of Package)

    <XmlArrayItemAttribute(GetType(Basic_Integer_Type)),
     XmlArrayItemAttribute(GetType(Basic_Boolean_Type)),
     XmlArrayItemAttribute(GetType(Basic_Floating_Point_Type)),
     XmlArrayItemAttribute(GetType(Basic_Character_Type)),
     XmlArrayItemAttribute(GetType(Array_Type)),
     XmlArrayItemAttribute(GetType(Enumerated_Type)),
     XmlArrayItemAttribute(GetType(Fixed_Point_Type)),
     XmlArrayItemAttribute(GetType(Record_Type)),
     XmlArray("Types")>
    Public Types As New List(Of Type)

    <XmlArrayItemAttribute(GetType(Client_Server_Interface)),
     XmlArrayItemAttribute(GetType(Event_Interface)),
     XmlArray("Interfaces")>
    Public Interfaces As New List(Of Software_Interface)

    Public Component_Types As New List(Of Component_Type)

    Public Compositions As New List(Of Composition)

    Private Shared ReadOnly Context_Menu As New Package_Context_Menu()

    Public Const Metaclass_Name As String = "Package"

    Private Shared ReadOnly Package_Not_Empty As New Modeling_Rule(
        "Package_Not_Empty",
        "Shall aggregate a least one element.")

    Private Const SVG_COLOR As String = "rgb(0,162,232)"
    Private Const SVG_NB_CHARS_PKG_DESC As Integer = CInt(1.5 * SVG_MIN_CHAR_PER_LINE)
    Public Shared SVG_PKG_BOX_WIDTH As Integer = Get_Box_Width(SVG_NB_CHARS_PKG_DESC)


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
            Me.Children.AddRange(Me.Packages)
            Me.Children.AddRange(Me.Types)
            Me.Children.AddRange(Me.Interfaces)
            Me.Children.AddRange(Me.Component_Types)
            Me.Children.AddRange(Me.Compositions)
        End If
        Return Me.Children
    End Function

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
            Me.Get_Project().Add_Element_To_Project(new_pkg)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Array_Type()

        ' Display a creation form
        Dim creation_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Array_Type.Metaclass_Name,
            "",
            Array_Type.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "",
            Me.Get_All_Types_From_Project(),
            Array_Type.Multiplicity_Minimum_Value.ToString())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        ' Treat creation form result
        If creation_form_result = DialogResult.OK Then

            ' Create the array type
            Dim new_array_type As New Array_Type(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                CUInt(creation_form.Get_Multiplicity()),
                creation_form.Get_Ref_Element().Identifier)

            ' Add array type to its package and project
            Me.Types.Add(new_array_type)
            Me.Children.Add(new_array_type)
            Me.Get_Project().Add_Element_To_Project(new_array_type)

            Me.Update_Views()

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
            Me.Get_Project().Add_Element_To_Project(new_enumeration)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Fixed_Point_Type()

        Dim creation_form As New Fixed_Point_Type_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Fixed_Point_Type.Metaclass_Name,
            "",
            Fixed_Point_Type.Metaclass_Name,
            "",
            Me.Get_Children_Name(),
            "",
            Me.Get_All_Basic_Int_From_Project(),
            "-",
            "1",
            "0")

        Dim creation_form_result As DialogResult = creation_form.ShowDialog()

        If creation_form_result = DialogResult.OK Then

            Dim new_fixed_point As New Fixed_Point_Type(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node,
                creation_form.Get_Ref_Element().Identifier,
                creation_form.Get_Unit(),
                creation_form.Get_Resolution(),
                creation_form.Get_Offset())

            ' Add fixed point type to its package and project
            Me.Types.Add(new_fixed_point)
            Me.Children.Add(new_fixed_point)
            Me.Get_Project().Add_Element_To_Project(new_fixed_point)
            Me.Update_Views()

        End If
    End Sub

    Public Sub Add_Record_Type()
        Dim creation_form As New Element_Form(
             Element_Form.E_Form_Kind.CREATION_FORM,
             Record_Type.Metaclass_Name,
             "",
             Record_Type.Metaclass_Name,
             "",
             Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_record As New Record_Type(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Types.Add(new_record)
            Me.Children.Add(new_record)
            Me.Get_Project().Add_Element_To_Project(new_record)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Client_Server_Interface()
        Dim creation_form As New Element_Form(
             Element_Form.E_Form_Kind.CREATION_FORM,
             Client_Server_Interface.Metaclass_Name,
             "",
             Client_Server_Interface.Metaclass_Name,
             "",
             Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_cs_if As New Client_Server_Interface(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Interfaces.Add(new_cs_if)
            Me.Children.Add(new_cs_if)
            Me.Get_Project().Add_Element_To_Project(new_cs_if)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Event_Interface()
        Dim creation_form As New Element_Form(
             Element_Form.E_Form_Kind.CREATION_FORM,
             Event_Interface.Metaclass_Name,
             "",
             Event_Interface.Metaclass_Name,
             "",
             Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_ev_if As New Event_Interface(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Interfaces.Add(new_ev_if)
            Me.Children.Add(new_ev_if)
            Me.Get_Project().Add_Element_To_Project(new_ev_if)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Component_Type()
        Dim creation_form As New Element_Form(
             Element_Form.E_Form_Kind.CREATION_FORM,
             Component_Type.Metaclass_Name,
             "",
             Component_Type.Metaclass_Name,
             "",
             Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_swct As New Component_Type(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Component_Types.Add(new_swct)
            Me.Children.Add(new_swct)
            Me.Update_Views()
        End If
    End Sub

    Public Sub Add_Composition()
        Dim creation_form As New Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Composition.Metaclass_Name,
            "",
            Composition.Metaclass_Name,
            "",
            Me.Get_Children_Name())
        Dim creation_form_result As DialogResult = creation_form.ShowDialog()
        If creation_form_result = DialogResult.OK Then
            Dim new_compo As New Composition(
                creation_form.Get_Element_Name(),
                creation_form.Get_Element_Description(),
                Me,
                Me.Node)
            Me.Compositions.Add(new_compo)
            Me.Children.Add(new_compo)
            Me.Get_Project().Add_Element_To_Project(new_compo)
            Me.Update_Views()
        End If
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Compute_SVG_Content() As String
        Me.SVG_Content = Me.Get_SVG_Def_Group_Header()

        ' Add Name compartment
        Dim title_rectangle_witdh As Integer = Get_Box_Width(Me.Name.Length)
        Dim title_rectangle_height As Integer = SVG_TEXT_LINE_HEIGHT + SVG_VERTICAL_MARGIN _
            + 2 * SVG_STROKE_WIDTH
        Me.SVG_Content &= Get_SVG_Rectangle(
            0,
            0,
            title_rectangle_witdh,
            title_rectangle_height,
            Package.SVG_COLOR,
            "0.5")
        Me.SVG_Content &= Get_SVG_Text(
            SVG_TEXT_MARGIN,
            SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT - 3,
            Me.Name,
            SVG_FONT_SIZE,
            False,
            False)

        ' Add description compartment
        Dim split_description As List(Of String) =
            Split_String(Me.Description, SVG_NB_CHARS_PKG_DESC)
        Dim description_rectangle_height =
            Math.Max(Get_SVG_Retangle_Height(split_description.Count), SVG_TEXT_LINE_HEIGHT * 4)
        Me.SVG_Content &= Get_Multi_Line_Rectangle(
            0,
            title_rectangle_height,
            split_description,
            Package.SVG_COLOR,
            SVG_PKG_BOX_WIDTH,
            description_rectangle_height)

        Me.SVG_Content &= Get_SVG_Def_Group_Footer()
        Me.SVG_Width = SVG_PKG_BOX_WIDTH
        Me.SVG_Height = description_rectangle_height + title_rectangle_height
        Return Me.SVG_Content
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model consistency checking
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Check_Own_Consistency(report As Consistency_Check_Report)
        MyBase.Check_Own_Consistency(report)
        Dim empty_check As New Consistency_Check_Report_Item(Me, Package.Package_Not_Empty)
        report.Add_Item(empty_check)
        empty_check.Set_Compliance(Me.Children.Count > 0)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Specific methods
    ' -------------------------------------------------------------------------------------------- '

    Protected Sub Get_All_Sub_Packages(ByRef pkg_list As List(Of Package))
        For Each pkg In Me.Packages
            pkg_list.Add(pkg)
            pkg.Get_All_Sub_Packages(pkg_list)
        Next
    End Sub

End Class