Public Class Client_Server_Operation_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Parameter As New ToolStripMenuItem("Add Parameter")
    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Parameter})
    End Sub

    Private Sub Add_Parameter(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Parameter.Click
        Dim cs_op As Client_Server_Operation =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Client_Server_Operation)
        cs_op.Add_Parameter()
    End Sub

End Class
