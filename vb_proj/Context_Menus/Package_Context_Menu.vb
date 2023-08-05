Public Class Package_Context_Menu

    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Package As New ToolStripMenuItem("Add Package")
    Protected WithEvents Menu_Add_Array_Type As New ToolStripMenuItem("Add Array_Type")
    Protected WithEvents Menu_Add_Enumerated_Type As New ToolStripMenuItem("Add Enumerated_Type")


    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Package,
            Me.Menu_Add_Array_Type,
            Me.Menu_Add_Enumerated_Type})
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

End Class
