Public Class Call_OS_Operation_Form
    Inherits Element_Form

    Private ReadOnly Swc_By_Name As New Dictionary(Of String, Component_Prototype)
    Private ReadOnly Op_By_Name As New Dictionary(Of String, OS_Operation)
    Private WithEvents Component_ComboBox As New ComboBox
    Private WithEvents Operation_ComboBox As New ComboBox
    Private WithEvents Priority_TextBox As New TextBox

    Public Sub New(
        form_kind As E_Form_Kind,
        default_uuid As String,
        default_name As String,
        default_description As String,
        component_prototype_list As List(Of Component_Prototype),
        default_swc_name As String,
        default_os_operation_name As String,
        default_priority As String)

        MyBase.New(
            form_kind,
            Call_OS_Operation.Metaclass_Name,
            default_uuid,
            default_name,
            default_description)

        Me.Name_TextBox.ReadOnly = True

        If Not IsNothing(component_prototype_list) Then
            For Each swc In component_prototype_list
                Me.Swc_By_Name.Add(swc.Name, swc)
                Me.Component_ComboBox.Items.Add(swc.Name)
            Next
        End If

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add call panel
        Dim call_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(call_panel)

        Dim call_label As New Label With {
            .Text = "Called OS_Operation",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        call_panel.Controls.Add(call_label)
        inner_item_y_pos += call_label.Height + ESMT_Form.Marge

        Dim swc_label As New Label With {
            .Text = "Component_Prototype :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        call_panel.Controls.Add(swc_label)
        With Me.Component_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_swc_name
            .Location = New Point(ESMT_Form.Marge + swc_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        call_panel.Controls.Add(Me.Component_ComboBox)
        inner_item_y_pos += Me.Component_ComboBox.Height + ESMT_Form.Marge

        Dim op_label As New Label With {
            .Text = "OS_Operation :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        call_panel.Controls.Add(op_label)
        With Me.Operation_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_os_operation_name
            .Location = New Point(ESMT_Form.Marge + op_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        call_panel.Controls.Add(Me.Operation_ComboBox)
        inner_item_y_pos += Me.Operation_ComboBox.Height + ESMT_Form.Marge

        Dim priority_label As New Label With {
            .Text = "Priority :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        call_panel.Controls.Add(priority_label)
        Me.Priority_TextBox = New TextBox With {
            .Text = default_priority,
            .Location = New Point(ESMT_Form.Marge + priority_label.Width, inner_item_y_pos),
            .Size = ESMT_Form.Field_Value_Size}
        call_panel.Controls.Add(Me.Priority_TextBox)
        inner_item_y_pos += Me.Priority_TextBox.Height + ESMT_Form.Marge

        call_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += call_panel.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Main_Button
        Me.Main_Button.Location = New Point((Form_Width - Button_Width) \ 2, item_y_pos)
        item_y_pos += Me.Main_Button.Height + ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' (Re)design Form
        Me.ClientSize = New Size(Form_Width, item_y_pos)

    End Sub


    Protected Overrides Sub Set_Fields_Read_Only()
        MyBase.Set_Fields_Read_Only()
        Me.Component_ComboBox.Enabled = False
        Me.Operation_ComboBox.Enabled = False
        Me.Priority_TextBox.ReadOnly = True
    End Sub

    Private Sub Update_Name()
        Dim swc_name As String = "undefined"
        Dim op_name As String = "undefined"

        Dim swc As Component_Prototype = Get_Swc()
        If Not IsNothing(swc) Then
            swc_name = swc.Name
        End If
        Dim op As OS_Operation = Get_Op()
        If Not IsNothing(op) Then
            op_name = op.Name
        End If

        Me.Name_TextBox.Text = swc_name & "__" & op_name
    End Sub

    Private Function Get_Swc() As Component_Prototype
        If Me.Swc_By_Name.ContainsKey(Component_ComboBox.Text) Then
            Return Me.Swc_By_Name(Component_ComboBox.Text)
        Else
            Return Nothing
        End If
    End Function

    Private Function Get_Op() As OS_Operation
        If Me.Op_By_Name.ContainsKey(Operation_ComboBox.Text) Then
            Return Me.Op_By_Name(Operation_ComboBox.Text)
        Else
            Return Nothing
        End If
    End Function

    Private Sub Update_Operation_ComboBox_Items()
        Me.Operation_ComboBox.Items.Clear()
        Me.Op_By_Name.Clear()
        Dim swc As Component_Prototype = Get_Swc()
        If Not IsNothing(swc) Then
            Dim swct As Component_Type
            swct = CType(swc.Get_Elmt_From_Prj_By_Id(swc.Element_Ref), Component_Type)
            If Not IsNothing(swct) Then
                For Each op In swct.Operations
                    Me.Op_By_Name.Add(op.Name, op)
                    Me.Operation_ComboBox.Items.Add(op.Name)
                Next
            End If
        End If
    End Sub

    Private Sub Swc_Changed() Handles Component_ComboBox.SelectedValueChanged
        Me.Update_Operation_ComboBox_Items()
        Me.Update_Name()
    End Sub

    Private Sub Op_Changed() Handles Operation_ComboBox.SelectedValueChanged
        Me.Update_Name()
    End Sub

    Public Function Get_Component_Prototype_Identifier() As Guid
        Dim swc As Component_Prototype = Me.Get_Swc()
        If Not IsNothing(swc) Then
            Return swc.Identifier
        Else
            Return Guid.Empty
        End If
    End Function

    Public Function Get_OS_Operation_Identifier() As Guid
        If Op_By_Name.ContainsKey(Me.Operation_ComboBox.Text) Then
            Return Op_By_Name(Me.Operation_ComboBox.Text).Identifier
        Else
            Return Guid.Empty
        End If
    End Function

    Public Function Get_Priority() As UInteger
        Return CUInt(Me.Priority_TextBox.Text)
    End Function

End Class
