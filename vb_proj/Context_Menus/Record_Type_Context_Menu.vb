Public Class Record_Type_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Field As New ToolStripMenuItem("Add Field")

    Public Sub New()
        Me.BackColor = ESMT_Form.Background_Color
        Me.ForeColor = ESMT_Form.Foreground_Color
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Field})
    End Sub

    Private Sub Add_Field(
    ByVal sender As Object,
    ByVal e As EventArgs) Handles Menu_Add_Field.Click
        Dim record As Record_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Record_Type)
        record.Add_Field()
    End Sub

End Class
