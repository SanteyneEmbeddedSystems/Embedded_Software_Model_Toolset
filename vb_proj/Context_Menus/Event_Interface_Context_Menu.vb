Public Class Event_Interface_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Parameter As New ToolStripMenuItem("Add Parameter")
    Public Sub New()
        Me.BackColor = ESMT_Form.Background_Color
        Me.ForeColor = ESMT_Form.Foreground_Color
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
        Dim ev_if As Event_Interface =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Event_Interface)
        ev_if.Add_Parameter()
    End Sub

End Class
