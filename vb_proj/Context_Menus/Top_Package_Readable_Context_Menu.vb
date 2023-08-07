Public Class Top_Package_Readable_Context_Menu

    Inherits Top_Package_Context_Menu

    Private WithEvents Menu_Make_Writable As New ToolStripMenuItem("Make writable")

    Public Sub New()
        Me.Items.Clear()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Remove_Top,
            Me.Menu_Display_Path,
            New ToolStripSeparator,
            Me.Menu_View,
            Me.Menu_Make_Writable})
    End Sub

    Private Sub Make_Writable(
        ByVal sender As Object,
        ByVal e As EventArgs) Handles Menu_Make_Writable.Click
        Dim pkg_name As String = Get_Top_Package(sender).Name
        Get_Project(sender).Make_Package_Writable(pkg_name)
    End Sub

End Class
