Public Class Component_Type
    Inherits Must_Describe_Software_Element

    Public Const Metaclass_Name As String = "Component_Type"

    Public Const SVG_COLOR As String = "rgb(0,0,0)"

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


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for diagrams
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Function Get_SVG_Content(x_pos As Integer, y_pos As Integer) As String
        Dim svg_content As String

        ' Compute Box width
        Dim box_width As Integer = Get_Box_Witdh(SVG_MIN_CHAR_PER_LINE)

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

        Return svg_content

    End Function

End Class
