Public Class SWCT_Context_Menu
    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Configuration As New ToolStripMenuItem("Add Configuration")
    Protected WithEvents Menu_Add_OS_Operation As New ToolStripMenuItem("Add OS_Operation")
    Protected WithEvents Menu_Add_Provider_Port As New ToolStripMenuItem("Add Provider_Port")
    Protected WithEvents Menu_Add_Requirer_Port As New ToolStripMenuItem("Add Requirer_Port")


    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Configuration,
            Me.Menu_Add_OS_Operation,
            Me.Menu_Add_Provider_Port,
            Me.Menu_Add_Requirer_Port})
    End Sub

    Private Sub Add_Configuration(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Configuration.Click
        Dim swct As Component_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Component_Type)
        swct.Add_Configuration()
    End Sub

    Private Sub Add_OS_Operation(
        ByVal sender As Object,
        ByVal e As EventArgs) Handles Menu_Add_OS_Operation.Click
        Dim swct As Component_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Component_Type)
        swct.Add_OS_Operation()
    End Sub

    Private Sub Add_Provider_Port(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Provider_Port.Click
        Dim swct As Component_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Component_Type)
        swct.Add_Provider_Port()
    End Sub

    Private Sub Add_Requirer_Port(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Requirer_Port.Click
        Dim swct As Component_Type =
            CType(Element_Context_Menu.Get_Selected_Element(sender), Component_Type)
        swct.Add_Requirer_Port()
    End Sub

End Class
