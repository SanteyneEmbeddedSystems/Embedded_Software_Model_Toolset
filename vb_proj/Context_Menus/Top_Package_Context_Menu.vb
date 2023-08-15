Public MustInherit Class Top_Package_Context_Menu

    Inherits Package_Context_Menu

    Protected WithEvents Menu_Remove_Top As New ToolStripMenuItem("Remove from project")
    Protected WithEvents Menu_Display_Dependencies As New ToolStripMenuItem("Display dependencies")

    Private Sub Remove(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Remove_Top.Click
        Dim pkg_name As String = Get_Top_Package(sender).Name
        Get_Project(sender).Remove_Package(pkg_name)
    End Sub

    Private Sub Display_Dependencies(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Display_Dependencies.Click
        Dim pkg_name As String = Get_Top_Package(sender).Name
        Get_Top_Package(sender).Display_Dependencies()
    End Sub

    Protected Shared Function Get_Top_Package(sender As Object) As Top_Level_Package
        Return CType(Element_Context_Menu.Get_Selected_Element(sender), Top_Level_Package)
    End Function

    Protected Shared Function Get_Project(sender As Object) As Software_Project
        Return Get_Top_Package(sender).Project
    End Function

End Class