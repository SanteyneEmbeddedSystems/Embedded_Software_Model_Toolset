Public Class Project_Context_Menu

    Inherits Element_Context_Menu

    Private WithEvents Menu_Save As New ToolStripMenuItem("Save")
    Private WithEvents Menu_Add_Existing_Package As New ToolStripMenuItem("Add existing Package")
    Private WithEvents Menu_Add_Existing_Readable_Package As _
        New ToolStripMenuItem("Add existing Package (read only)")
    Private WithEvents Menu_Add_New_Package As New ToolStripMenuItem("Add new Package")

    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Save,
            Me.Menu_Edit,
            Me.Menu_View,
            New ToolStripSeparator,
            Me.Menu_Add_Existing_Package,
            Me.Menu_Add_Existing_Readable_Package,
            Me.Menu_Add_New_Package})
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

    Private Shared Function Get_Project(sender As Object) As Software_Project
        Return CType(Element_Context_Menu.Get_Selected_Element(sender), Software_Project)
    End Function

End Class
