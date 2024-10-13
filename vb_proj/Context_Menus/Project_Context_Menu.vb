Public Class Project_Context_Menu

    Inherits ContextMenuStrip

    Private WithEvents Menu_Save As New ToolStripMenuItem("Save")
    Protected WithEvents Menu_Edit As New ToolStripMenuItem("Edit")
    Protected WithEvents Menu_View As New ToolStripMenuItem("View")
    Private WithEvents Menu_Add_Existing_Package As New ToolStripMenuItem("Add existing Package")
    Private WithEvents Menu_Add_Existing_Readable_Package As _
        New ToolStripMenuItem("Add existing Package (read only)")
    Private WithEvents Menu_Add_New_Package As New ToolStripMenuItem("Add new Package")
    Private WithEvents Menu_Check_Model As New ToolStripMenuItem("Check model")

    Public Sub New()
        Me.BackColor = ESMT_Form.Background_Color
        Me.ForeColor = ESMT_Form.Foreground_Color
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Save,
            Me.Menu_Edit,
            Me.Menu_View,
            New ToolStripSeparator,
            Me.Menu_Add_Existing_Package,
            Me.Menu_Add_Existing_Readable_Package,
            Me.Menu_Add_New_Package,
            New ToolStripSeparator,
            Me.Menu_Check_Model})
    End Sub

    Private Sub Edit(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Edit.Click
        Get_Project(sender).Edit()
    End Sub

    Private Sub View(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_View.Click
        Get_Project(sender).View()
    End Sub

    Private Sub Save(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Save.Click
        Get_Project(sender).Save()
    End Sub

    Private Sub Add_Existing_Package(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Existing_Package.Click
        Get_Project(sender).Load_Package(True)
    End Sub

    Private Sub Add_Existing_Readable_Package(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Existing_Readable_Package.Click
        Get_Project(sender).Load_Package(False)
    End Sub

    Private Sub Add_New_Package(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_New_Package.Click
        Get_Project(sender).Create_Package()
    End Sub

    Private Sub Check_Model(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Check_Model.Click
        Get_Project(sender).Check_Model()
    End Sub

    Private Shared Function Get_Project(sender As Object) As Software_Project
        Dim tsmi As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        Dim cms As ContextMenuStrip = CType(tsmi.Owner, ContextMenuStrip)
        Dim tv As TreeView = CType(cms.SourceControl, TreeView)
        Dim tn As TreeNode = tv.GetNodeAt(tv.PointToClient(cms.Location))
        Return CType(tn.Tag, Software_Project)
    End Function

End Class
