Public Class Package_Context_Menu

    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Package As New ToolStripMenuItem("Add Package")
    Protected WithEvents Menu_Add_Array_Type As New ToolStripMenuItem("Add Array_Type")
    Protected WithEvents Menu_Add_Enumerated_Type As New ToolStripMenuItem("Add Enumerated_Type")
    Protected WithEvents Menu_Add_Fixed_Point_Type As New ToolStripMenuItem("Add Fixed_Point_Type")
    Protected WithEvents Menu_Add_Record_Type As New ToolStripMenuItem("Add Record_Type")
    Protected WithEvents Menu_Add_CS_Interface As New ToolStripMenuItem("Add Client_Server_Interface")

    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Package,
            Me.Menu_Add_Array_Type,
            Me.Menu_Add_Enumerated_Type,
            Me.Menu_Add_Fixed_Point_Type,
            Me.Menu_Add_CS_Interface})
    End Sub

    Private Sub Add_Package(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Package.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Package()
    End Sub

    Private Sub Add_Array_Type(
        ByVal sender As Object,
        ByVal e As EventArgs) Handles Menu_Add_Array_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Array_Type()
    End Sub

    Private Sub Add_Enumerated_Type(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Enumerated_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Enumerated_Type()
    End Sub

    Private Sub Add_Fixed_Point_Type(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Fixed_Point_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Fixed_Point_Type()
    End Sub

    Private Sub Add_Record_Type(
        ByVal sender As Object,
        ByVal e As EventArgs) Handles Menu_Add_Record_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Record_Type()
    End Sub

    Private Sub AddClient_Server_Interface(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_CS_Interface.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Client_Server_Interface()
    End Sub

End Class
