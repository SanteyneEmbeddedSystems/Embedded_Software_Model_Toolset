Public Class Client_Server_Interface_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Operation As New ToolStripMenuItem("Add Operation")
    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Operation})
    End Sub

    Private Sub Add_Operation(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Operation.Click
        Dim cs_if As Client_Server_Interface =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Client_Server_Interface)
        cs_if.Add_Operation()
    End Sub

End Class
