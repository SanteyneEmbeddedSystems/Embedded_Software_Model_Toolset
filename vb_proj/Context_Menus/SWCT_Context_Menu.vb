Public Class SWCT_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Configuration As New ToolStripMenuItem("Add Configuration")
    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Configuration})
    End Sub

    Private Sub Add_Configuration(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Configuration.Click
        Dim swct As Component_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Component_Type)
        swct.Add_Configuration()
    End Sub
End Class
