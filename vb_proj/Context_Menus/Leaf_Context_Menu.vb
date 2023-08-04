Public Class Leaf_Context_Menu

    Inherits Element_Context_Menu

    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove})
    End Sub

End Class