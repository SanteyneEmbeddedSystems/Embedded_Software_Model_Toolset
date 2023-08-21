Imports System.IO

Public Class ESMT_Application

    Private Main_Window As ESMT_Main_Window ' bidirectional association
    Private Loaded_Project As Software_Project = Nothing


    ' -------------------------------------------------------------------------------------------- '
    ' Launchers
    ' -------------------------------------------------------------------------------------------- '

    ' Basic launcher when running executable without arguments
    Public Sub Run()
        Me.Main_Window = New ESMT_Main_Window(Me)
        Application.EnableVisualStyles()
        Application.Run(Me.Main_Window)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for menu bar
    ' -------------------------------------------------------------------------------------------- '

    Public Sub Load_Project()

        ' Open a form asking for a project file
        Dim load_prj_dialog = New OpenFileDialog With {
            .Title = "Select software model project file",
            .Filter = "Project file|*" & Software_Project.Project_File_Extension,
            .CheckFileExists = True}
        Dim result As DialogResult = load_prj_dialog.ShowDialog()

        ' Test the result from the form
        If result = DialogResult.OK Then
            ' The user has clicked on "Open"

            Me.Close_Loaded_Project()

            ' Load the project from the file given by user
            Dim project_file_path As String = load_prj_dialog.FileName
            Me.Loaded_Project = Software_Project.Load(
                project_file_path,
                Me.Main_Window.Get_Browser(),
                Me.Main_Window.Get_Diagram_Viewer())

            Me.Loaded_Project.Add_Predefined_Package()

            ' Save 'Last_Known_Name's updates
            Me.Loaded_Project.Save()

        End If

    End Sub


    Public Sub Create_Project()

        ' Open a form asking data for the creation of a project
        Dim prj_creation_form As New Recordable_Element_Form(
            Element_Form.E_Form_Kind.CREATION_FORM,
            Software_Project.Metaclass_Name,
            "",
            Software_Project.Metaclass_Name,
            "A good description is always useful.",
            "",
            Software_Project.Metaclass_Name,
            Software_Project.Project_File_Extension)

        Dim creation_result As DialogResult = prj_creation_form.ShowDialog()

        ' Test the result from the form
        If creation_result = DialogResult.OK Then
            ' The user has clicked on "Create"

            Me.Close_Loaded_Project()

            ' Create a new project using data from the form
            Me.Loaded_Project = New Software_Project(
                prj_creation_form.Get_Element_Name(),
                prj_creation_form.Get_Element_Description(),
                prj_creation_form.Get_File_Full_Path(),
                Me.Main_Window.Get_Browser(),
                Me.Main_Window.Get_Diagram_Viewer())

            Me.Loaded_Project.Add_Predefined_Package()

            Me.Loaded_Project.Save()

        End If

    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Private methods
    ' -------------------------------------------------------------------------------------------- '

    Private Sub Close_Loaded_Project()
        If Not IsNothing(Me.Loaded_Project) Then
            Me.Loaded_Project.Save()
            Me.Loaded_Project = Nothing
            Me.Main_Window.Clear()
        End If
    End Sub

End Class
