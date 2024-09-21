' Apply to NOT_FOUND and CORRUPTED Top_Level_Package
Public Class Top_Package_Unloaded_Context_Menu

    Inherits Top_Package_Context_Menu

    Public Sub New()
        Me.BackColor = Background_Color
        Me.ForeColor = Foreground_Color
        Me.Items.Clear()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Remove_Top})
    End Sub

End Class
