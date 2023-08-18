Public Class Component_Prototype_Form
    Inherits Element_With_Ref_Form

    Private WithEvents Config_Values_Table_View As New DataGridView

    Public Sub New(
        form_kind As E_Form_Kind,
        element_metaclass_name As String,
        default_uuid As String,
        default_name As String,
        default_description As String,
        forbidden_name_list As List(Of String),
        default_ref_element_path As String,
        ref_element_list As List(Of Software_Element),
        default_config_data As DataTable)

        MyBase.New(
            form_kind,
            element_metaclass_name,
            default_uuid,
            default_name,
            default_description,
            forbidden_name_list,
            "Component_Type",
            default_ref_element_path,
            ref_element_list)

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add configuration values panel
        Dim configurations_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(configurations_panel)

        Dim configurations_label As New Label With {
            .Text = "Configuration values",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        configurations_panel.Controls.Add(configurations_label)
        inner_item_y_pos += configurations_label.Height + ESMT_Form.Marge

        Me.Config_Values_Table_View = New DataGridView With {
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = New Size(ESMT_Form.Label_Width, 150),
            .DataSource = default_config_data,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .ScrollBars = ScrollBars.Vertical,
            .ColumnHeadersVisible = True,
            .RowHeadersVisible = False,
            .BackgroundColor = Color.FromName("Control"),
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill}
        configurations_panel.Controls.Add(Me.Config_Values_Table_View)
        inner_item_y_pos += Me.Config_Values_Table_View.Height + ESMT_Form.Marge

        configurations_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += configurations_panel.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub

    Public Function Get_Config_Data() As DataTable
        Return CType(Me.Config_Values_Table_View.DataSource, DataTable)
    End Function

    Private Sub Load_Form(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not IsNothing(Me.Config_Values_Table_View.DataSource) Then
            Me.Config_Values_Table_View.Columns(0).ReadOnly = True
            Me.Config_Values_Table_View.Columns(2).Visible = False
        End If
    End Sub

    Private Sub Ref_Swct_Changed() Handles Referenced_Element_ComboBox.SelectedIndexChanged
        Dim config_table As DataTable
        Dim swct As Component_Type = CType(Me.Get_Ref_Element(), Component_Type)
        config_table = Component_Prototype.Create_Config_Data_Table(swct)
        If Not IsNothing(Me.Config_Values_Table_View) Then
            Me.Config_Values_Table_View.DataSource = config_table
            Me.Config_Values_Table_View.Columns(0).ReadOnly = True
            Me.Config_Values_Table_View.Columns(2).Visible = False
        End If
    End Sub

    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Config_Values_Table_View.ReadOnly = True
        Dim style As New DataGridViewCellStyle With {
            .BackColor = Color.LightGray}
        Me.Config_Values_Table_View.DefaultCellStyle = style
    End Sub

End Class
