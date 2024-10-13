Public Class Enumerated_Type_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Enumeral As New ToolStripMenuItem("Add Enumeral")

    Public Sub New()
        Me.BackColor = ESMT_Form.Background_Color
        Me.ForeColor = ESMT_Form.Foreground_Color
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Enumeral})
    End Sub

    Private Sub Add_Enumeral(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Enumeral.Click
        Dim enum_type As Enumerated_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Enumerated_Type)
        enum_type.Add_Enumeral()
    End Sub

End Class