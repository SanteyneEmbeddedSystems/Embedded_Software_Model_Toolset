Public Class Read_Only_Context_Menu

    Inherits Element_Context_Menu

    Public Sub New()
        Me.BackColor = Background_Color
        Me.ForeColor = Foreground_Color
        Me.Items.AddRange(New ToolStripItem() {Me.Menu_View})
    End Sub

End Class