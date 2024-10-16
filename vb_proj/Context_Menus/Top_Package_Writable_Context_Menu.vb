﻿Public Class Top_Package_Writable_Context_Menu

    Inherits Top_Package_Context_Menu

    Private WithEvents Menu_Save As New ToolStripMenuItem("Save")
    Private WithEvents Menu_Make_Read_Only As New ToolStripMenuItem("Make read-only")


    Public Sub New()
        Me.BackColor = ESMT_Form.Background_Color
        Me.ForeColor = ESMT_Form.Foreground_Color
        Me.Items.Clear()
        Me.Items.AddRange(New ToolStripItem() {
            Me.Menu_Save,
            Me.Menu_Remove_Top,
            New ToolStripSeparator,
            Me.Menu_Edit,
            Me.Menu_View,
            Me.Menu_Make_Read_Only,
            New ToolStripSeparator,
            Me.Menu_Display_Dependencies,
            Me.Menu_Show_Content_On_Diagram,
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

    Private Sub Save(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Save.Click
        Get_Top_Package(sender).Save()
    End Sub

    Private Sub Make_Read_Only(
            ByVal sender As Object,
            ByVal e As EventArgs) Handles Menu_Make_Read_Only.Click
        Dim pkg_name As String = Get_Top_Package(sender).Name
        Get_Project(sender).Make_Package_Read_Only(pkg_name)
    End Sub

End Class