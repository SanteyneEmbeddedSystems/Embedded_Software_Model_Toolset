Public Class Top_Package_Writable_Context_Menu

    Inherits Top_Package_Context_Menu

    Private WithEvents Menu_Save As New ToolStripMenuItem("Save")
    Private WithEvents Menu_Make_Read_Only As New ToolStripMenuItem("Make read-only")
    Protected WithEvents Menu_Add_Package As New ToolStripMenuItem("Add Package")
    Protected WithEvents Menu_Add_Array_Type As New ToolStripMenuItem("Add Array_Type")
    Protected WithEvents Menu_Add_Enumerated_Type As New ToolStripMenuItem("Add Enumerated_Type")
    Protected WithEvents Menu_Add_Fixed_Point_Type As New ToolStripMenuItem("Add Fixed_Point_Type")
    Protected WithEvents Menu_Add_Record_Type As New ToolStripMenuItem("Add Record_Type")

    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Save,
            Me.Menu_Remove_Top,
            Me.Menu_Display_Path,
            New ToolStripSeparator,
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Make_Read_Only,
            New ToolStripSeparator,
            Me.Menu_Add_Package,
            Me.Menu_Add_Array_Type,
            Me.Menu_Add_Enumerated_Type,
            Me.Menu_Add_Fixed_Point_Type,
            Me.Menu_Add_Record_Type})
    End Sub

    Private Sub Save(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Save.Click
        Get_Top_Package(sender).Save()
    End Sub

    Private Sub Make_Read_Only(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Make_Read_Only.Click
        Dim pkg_name As String = Get_Top_Package(sender).Name
        Get_Project(sender).Make_Package_Read_Only(pkg_name)
    End Sub

    Private Sub Add_Package(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Package.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Package()
    End Sub


    Private Sub Add_Array_Type(
        ByVal sender As Object,
        ByVal e As EventArgs) Handles Menu_Add_Array_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Array_Type()
    End Sub

    Private Sub Add_Enumerated_Type(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Enumerated_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Enumerated_Type()
    End Sub

    Private Sub Add_Fixed_Point_Type(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Fixed_Point_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Fixed_Point_Type()
    End Sub

    Private Sub Add_Record_Type(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Record_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Record_Type()
    End Sub

End Class