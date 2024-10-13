Public Class Task_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Call_Operation As New ToolStripMenuItem("Add Call_OS_Operation")
    Public Sub New()
        Me.BackColor = ESMT_Form.Background_Color
        Me.ForeColor = ESMT_Form.Foreground_Color
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Call_Operation})
    End Sub

    Private Sub Add_Operation(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Call_Operation.Click
        Dim compo_task As Composition_Task =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Composition_Task)
        compo_task.Add_Call_OS_Operation()
    End Sub

End Class
