Public Class Component_Type
    Inherits Must_Describe_Software_Element

    Public Configurations As New List(Of Configuration_Parameter)

    Public Const Metaclass_Name As String = "Component_Type"

    Public Const SVG_COLOR As String = "rgb(0,0,0)"

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

    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Dim is_allowed As Boolean = False
        If parent.GetType() = GetType(Top_Level_Package) _
            Or parent.GetType() = GetType(Package) Then
            is_allowed = True
        End If
        Return is_allowed
    End Function

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Component_Type.Context_Menu
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


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

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

        ' Add title (Name + stereotype) compartment
        svg_content = Get_Title_Rectangle(x_pos, y_pos, Me.Name,
            Component_Type.SVG_COLOR, box_width, Metaclass_Name)

        ' Add description compartment
        Dim desc_rect_height As Integer = 0
        Dim split_description As List(Of String)
        split_description = Split_String(Me.Description, SVG_MIN_CHAR_PER_LINE)
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT,
            split_description,
            Component_Type.SVG_COLOR,
            box_width,
            desc_rect_height)

        ' Add configurations compartement
        svg_content &= Get_Multi_Line_Rectangle(
            x_pos,
            y_pos + SVG_TITLE_HEIGHT + desc_rect_height,
            config_lines,
            Component_Type.SVG_COLOR,
            box_width)

        Return svg_content

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