Public Class Read_Only_SWCT_Context_Menu

    Inherits SWCT_Context_Menu

    Public Sub New()
        Me.Items.Clear()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_View,
            New ToolStripSeparator,
            Me.Menu_Show_Dependencies_On_Diagram})
    End Sub

End Class
