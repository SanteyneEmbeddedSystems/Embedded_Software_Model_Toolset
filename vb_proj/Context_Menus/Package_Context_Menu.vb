﻿Public Class Package_Context_Menu

    Inherits Element_Context_Menu

    Protected WithEvents Menu_Add_Package As New ToolStripMenuItem("Add Package")
    Protected WithEvents Menu_Add_Array_Type As New ToolStripMenuItem("Add Array_Type")
    Protected WithEvents Menu_Add_Enumerated_Type As New ToolStripMenuItem("Add Enumerated_Type")
    Protected WithEvents Menu_Add_Fixed_Point_Type As New ToolStripMenuItem("Add Fixed_Point_Type")
    Protected WithEvents Menu_Add_Record_Type As New ToolStripMenuItem("Add Record_Type")
    Protected WithEvents Menu_Add_CS_Interface As New ToolStripMenuItem("Add Client_Server_Interface")
    Protected WithEvents Menu_Add_Event_Interface As New ToolStripMenuItem("Add Event_Interface")
    Protected WithEvents Menu_Add_Component_Type As New ToolStripMenuItem("Add Component_Type")
    Protected WithEvents Menu_Add_Composition As New ToolStripMenuItem("Add Composition")

    Public Sub New()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Remove,
            New ToolStripSeparator,
            Me.Menu_Add_Package,
            New ToolStripSeparator,
            Me.Menu_Add_Array_Type,
            Me.Menu_Add_Enumerated_Type,
            Me.Menu_Add_Fixed_Point_Type,
            Me.Menu_Add_Record_Type,
            New ToolStripSeparator,
            Me.Menu_Add_CS_Interface,
            Me.Menu_Add_Event_Interface,
            New ToolStripSeparator,
            Me.Menu_Add_Component_Type,
            Me.Menu_Add_Composition})
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

    Private Sub Add_Client_Server_Interface(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_CS_Interface.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Client_Server_Interface()
    End Sub

    Private Sub Add_Event_Interface(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Event_Interface.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Event_Interface()
    End Sub

    Private Sub Add_Component_Type(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Component_Type.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Component_Type()
    End Sub


    Private Sub Add_Composition(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Add_Composition.Click
        Dim pkg As Package = CType(Element_Context_Menu.Get_Selected_Element(sender), Package)
        pkg.Add_Composition()
    End Sub

End Class
