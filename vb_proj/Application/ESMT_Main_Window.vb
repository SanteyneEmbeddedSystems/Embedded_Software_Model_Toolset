Public Class ESMT_Main_Window

    Inherits Form

    Private ReadOnly My_Application As ESMT_Application ' bidirectional association

    Private WithEvents Model_Browser As New TreeView
    Private WithEvents Diagram_Page As New WebBrowser

    Private WithEvents Menu_Load_Project As New ToolStripMenuItem
    Private WithEvents Menu_New_Project As New ToolStripMenuItem


    ' -------------------------------------------------------------------------------------------- '
    ' Methods provided to ESMT_Application
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New(an_appli As ESMT_Application)
        Me.My_Application = an_appli

        SuspendLayout()

        Me.Text = "Embedded Software Model Toolset"
        Me.ClientSize = New Size(1000, 750)

        '------------------------------------------------------------------------------------------'
        ' Create menu bar
        Dim main_menu As New MenuStrip
        Me.Controls.Add(main_menu)

        Dim project_menu As ToolStripMenuItem
        project_menu = CType(main_menu.Items.Add("Project"), ToolStripMenuItem)

        With Me.Menu_Load_Project
            .Text = "Load project"
        End With
        project_menu.DropDownItems.Add(Me.Menu_Load_Project)

        With Me.Menu_New_Project
            .Text = "New project"
        End With
        project_menu.DropDownItems.Add(Me.Menu_New_Project)


        '------------------------------------------------------------------------------------------'
        ' Split main window in two parts : browser + diagrams
        Dim vertical_split_container As New SplitContainer With {
            .Dock = DockStyle.Fill}
        vertical_split_container.Panel1.Controls.Add(Me.Model_Browser)
        vertical_split_container.Panel2.Controls.Add(Me.Diagram_Page)
        Me.Controls.Add(vertical_split_container)


        '------------------------------------------------------------------------------------------'
        ' Create browser
        With Me.Model_Browser
            .Dock = DockStyle.Fill
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom _
                Or AnchorStyles.Left Or AnchorStyles.Right
            .Location = New Point(0, 25)
            .AllowDrop = True
        End With

        Me.Model_Browser.ImageList = New ImageList
        With Me.Model_Browser.ImageList.Images
            .Add("Project", My.Resources.Project)
            .Add("Package", My.Resources.Package)
            .Add("Basic_Type", My.Resources.Basic_Type)
            .Add("Array_Type", My.Resources.Array_Type)
            .Add("Enumerated_Type", My.Resources.Enumerated_Type)
            .Add("Fixed_Point_Type", My.Resources.Fixed_Point_Type)
            .Add("Record_Type", My.Resources.Record_Type)
            .Add("Record_Field", My.Resources.Record_Field)
            .Add("Client_Server_Interface", My.Resources.CS_Interface)
            .Add("Client_Server_Operation", My.Resources.CS_Operation)
            .Add("Operation_Parameter", My.Resources.Operation_Parameter)
        End With


        '------------------------------------------------------------------------------------------'
        ' Create diagram panel
        With Me.Diagram_Page
            .Dock = DockStyle.Fill
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom _
                Or AnchorStyles.Left Or AnchorStyles.Right
            .Location = New Point(0, 25)
        End With

        ResumeLayout(False)

    End Sub

    Public Function Get_Browser() As TreeView
        Return Me.Model_Browser
    End Function

    Public Function Get_Diagram_Viewer() As WebBrowser
        Return Me.Diagram_Page
    End Function

    Public Sub Clear()
        Me.Model_Browser.Nodes.Clear()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Menu bar events
    ' -------------------------------------------------------------------------------------------- '

    Private Sub Load_Project_Clicked() Handles Menu_Load_Project.Click
        Me.My_Application.Load_Project()
    End Sub

    Private Sub New_Project_Clicked() Handles Menu_New_Project.Click
        Me.My_Application.Create_Project()
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Model_Browser events
    ' -------------------------------------------------------------------------------------------- '

    Private Sub Browser_Item_Drag(
            ByVal sender As Object,
            ByVal e As ItemDragEventArgs) _
            Handles Model_Browser.ItemDrag
        DoDragDrop(e.Item, DragDropEffects.Move)
    End Sub

    Private Sub Browser_Drag_Enter(
            ByVal sender As Object,
            ByVal e As DragEventArgs) _
            Handles Model_Browser.DragEnter

        ' Check if there is a TreeNode being dragged
        If e.Data.GetDataPresent("System.Windows.Forms.TreeNode", True) Then
            ' TreeNode found : allow move effect
            e.Effect = DragDropEffects.Move Or DragDropEffects.Scroll
        Else
            ' No TreeNode found : prevent move
            e.Effect = DragDropEffects.None
        End If

    End Sub

    Private Sub Browser_Drag_Over(
            ByVal sender As Object,
            ByVal e As DragEventArgs) _
            Handles Model_Browser.DragOver

        ' Check that there is a TreeNode being dragged
        If e.Data.GetDataPresent("System.Windows.Forms.TreeNode", True) = False Then
            Exit Sub
        End If

        ' Get the node under the mouse
        Dim pt As Point = CType(sender, TreeView).PointToClient(New Point(e.X, e.Y))
        Dim targeted_node As TreeNode = Me.Model_Browser.GetNodeAt(pt)

        ' See if the targetNode is currently selected, if so no need to validate again
        If Not (Me.Model_Browser.SelectedNode Is targeted_node) Then

            ' Select the node currently under the cursor
            Me.Model_Browser.SelectedNode = targeted_node

            ' Get dragged node
            Dim dragged_node As TreeNode
            dragged_node = CType(e.Data.GetData("System.Windows.Forms.TreeNode"), TreeNode)

            ' Check that the targeted node is not the current parent of the dragged node
            If dragged_node.Parent Is targeted_node Then
                e.Effect = DragDropEffects.None
                Exit Sub
            End If

            ' Check that the dragged element is not in a read only package
            Dim dragged_element As Software_Element = CType(dragged_node.Tag, Software_Element)
            If Not dragged_element.Get_Top_Package().Is_Writable() Then
                e.Effect = DragDropEffects.None
                Exit Sub
            End If

            ' Check that the targeted element is not in a read only package
            Dim targeted_element As Software_Element = CType(targeted_node.Tag, Software_Element)
            Dim top_pkg As Top_Level_Package = targeted_element.Get_Top_Package()
            If Not IsNothing(top_pkg) Then ' Possible if targeted_element is project
                If Not top_pkg.Is_Writable() Then
                    e.Effect = DragDropEffects.None
                    Exit Sub
                End If
            End If

            ' Check that the targeted element is a possible parent from meta-model point of view
            If Not dragged_element.Is_Allowed_Parent(targeted_element) Then
                e.Effect = DragDropEffects.None
                Exit Sub
            End If

            ' Check that the targeted element does not already aggregate an element named as the
            ' dragged element
            If targeted_element.Get_Children_Name().Contains(dragged_element.Name) Then
                e.Effect = DragDropEffects.None
                Exit Sub
            End If

            ' Check that the targeted node is not the dragged node and also that it is not a child
            ' of the dragged node
            Do Until targeted_node Is Nothing
                If targeted_node Is dragged_node Then
                    e.Effect = DragDropEffects.None
                    Exit Sub
                End If
                targeted_node = targeted_node.Parent
            Loop

            ' Currently selected node is a suitable target
            e.Effect = DragDropEffects.Move
        End If

    End Sub

    Private Sub Browser_Drag_Drop(
            ByVal sender As Object,
            ByVal e As DragEventArgs) _
            Handles Model_Browser.DragDrop

        ' Check that there is a TreeNode being dragged
        If e.Data.GetDataPresent("System.Windows.Forms.TreeNode", True) = False Then
            Exit Sub
        End If

        ' Get the TreeNode being dragged
        Dim dragged_node As TreeNode
        dragged_node = CType(e.Data.GetData("System.Windows.Forms.TreeNode"), TreeNode)

        ' The targeted node should be selected from the DragOver event
        Dim targeted_node As TreeNode = Me.Model_Browser.SelectedNode

        ' Get elements
        Dim targeted_element As Software_Element = CType(targeted_node.Tag, Software_Element)
        Dim dragged_element As Software_Element = CType(dragged_node.Tag, Software_Element)

        ' Move elements
        dragged_element.Move(targeted_element)

    End Sub

    Private Sub Browser_Select(
            ByVal sender As Object,
            ByVal e As TreeViewEventArgs) _
            Handles Model_Browser.AfterSelect

        ' Update/Create SVG file for selected element
        Dim sw_elmt As Software_Element = CType(e.Node.Tag, Software_Element)
        Dim diagram_full_path As String
        diagram_full_path = sw_elmt.Update_SVG_Diagram()

        ' Update diagram area
        Me.Diagram_Page.Navigate(diagram_full_path)

    End Sub


End Class