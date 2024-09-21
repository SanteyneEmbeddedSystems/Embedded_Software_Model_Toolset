Public Class Composition_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Prototype As New ToolStripMenuItem("Add Component_Prototype")
    Protected WithEvents Menu_Add_Connector As New ToolStripMenuItem("Add Connector")
    Protected WithEvents Menu_Add_Task As New ToolStripMenuItem("Add Task")

    Public Sub New()
        Me.BackColor = Background_Color
        Me.ForeColor = Foreground_Color
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Prototype,
            Menu_Add_Connector,
            Menu_Add_Task})
    End Sub

    Private Sub Add_Prototype(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Prototype.Click
        Dim compo As Composition = CType(Get_Selected_Element(sender), Composition)
        compo.Add_Prototype()
    End Sub

    Private Sub Add_Connector(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Connector.Click
        Dim compo As Composition = CType(Get_Selected_Element(sender), Composition)
        compo.Add_Connector()
    End Sub

    Private Sub Add_Task(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Task.Click
        Dim compo As Composition = CType(Get_Selected_Element(sender), Composition)
        compo.Add_Task()
    End Sub

End Class
