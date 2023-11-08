Public Class Connector_Form
    Inherits Element_Form

    Private ReadOnly Swc_By_Name As New Dictionary(Of String, Component_Prototype)
    Private WithEvents Prov_Component_ComboBox As New ComboBox
    Private WithEvents Prov_Port_ComboBox As New ComboBox
    Private WithEvents Req_Component_ComboBox As New ComboBox
    Private WithEvents Req_Port_ComboBox As New ComboBox


    Public Sub New(
        form_kind As E_Form_Kind,
        default_uuid As String,
        default_name As String,
        default_description As String,
        component_prototype_list As List(Of Component_Prototype),
        default_prov_swc_path As String,
        default_prov_port_name As String,
        default_req_swc_path As String,
        default_req_port_name As String)

        MyBase.New(
            form_kind,
            Connector.Metaclass_Name,
            default_uuid,
            default_name,
            default_description)

        Me.Name_TextBox.ReadOnly = True

        If Not IsNothing(component_prototype_list) Then
            For Each swc In component_prototype_list
                Me.Swc_By_Name.Add(swc.Name, swc)
                Me.Prov_Component_ComboBox.Items.Add(swc.Name)
                Me.Req_Component_ComboBox.Items.Add(swc.Name)
            Next
        End If

        ' Get the current y position of Main_Button
        Dim item_y_pos As Integer = Me.ClientSize.Height - ESMT_Form.Marge - Button_Height

        Dim inner_item_y_pos As Integer = ESMT_Form.Marge

        '------------------------------------------------------------------------------------------'
        ' Add connection panel
        Dim connection_panel As New Panel With {
            .Location = New Point(ESMT_Form.Marge, item_y_pos),
            .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(connection_panel)

        Dim connection_label As New Label With {
            .Text = "Connection definition",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Label_Size}
        connection_panel.Controls.Add(connection_label)
        inner_item_y_pos += connection_label.Height + ESMT_Form.Marge

        Dim prov_swc_label As New Label With {
            .Text = "Provider component :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        connection_panel.Controls.Add(prov_swc_label)
        With Me.Prov_Component_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_prov_swc_path
            .Location = New Point(ESMT_Form.Marge + prov_swc_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        connection_panel.Controls.Add(Me.Prov_Component_ComboBox)
        inner_item_y_pos += Me.Prov_Component_ComboBox.Height + ESMT_Form.Marge

        Dim prov_port_label As New Label With {
            .Text = "Provider port :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        connection_panel.Controls.Add(prov_port_label)
        With Me.Prov_Port_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_prov_port_name
            .Location = New Point(ESMT_Form.Marge + prov_port_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        connection_panel.Controls.Add(Me.Prov_Port_ComboBox)
        inner_item_y_pos += Me.Prov_Port_ComboBox.Height + ESMT_Form.Marge

        Dim req_swc_label As New Label With {
            .Text = "Requirer component :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        connection_panel.Controls.Add(req_swc_label)
        With Me.Req_Component_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_req_swc_path
            .Location = New Point(ESMT_Form.Marge + req_swc_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        connection_panel.Controls.Add(Me.Req_Component_ComboBox)
        inner_item_y_pos += Me.Req_Component_ComboBox.Height + ESMT_Form.Marge

        Dim req_port_label As New Label With {
            .Text = "Requirer port :",
            .Location = New Point(ESMT_Form.Marge, inner_item_y_pos),
            .Size = ESMT_Form.Field_Label_Size}
        connection_panel.Controls.Add(req_port_label)
        With Me.Req_Port_ComboBox
            .DropDownStyle = ComboBoxStyle.DropDownList
            .Text = default_req_port_name
            .Location = New Point(ESMT_Form.Marge + req_port_label.Width, inner_item_y_pos)
            .Size = ESMT_Form.Field_Value_Size
        End With
        connection_panel.Controls.Add(Me.Req_Port_ComboBox)
        inner_item_y_pos += Me.Req_Port_ComboBox.Height + ESMT_Form.Marge

        connection_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += connection_panel.Height + ESMT_Form.Marge

        Me.Update_Name()

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
        Me.Prov_Component_ComboBox.Enabled = False
        Me.Prov_Port_ComboBox.Enabled = False
        Me.Req_Component_ComboBox.Enabled = False
        Me.Req_Port_ComboBox.Enabled = False
    End Sub

    Private Sub Update_Name()
        Dim prov_swc_name As String = "undefined"
        Dim prov_port_name As String = "undefined"
        Dim req_swc_name As String = "undefined"
        Dim req_port_name As String = "undefined"

        Dim prov_swc As Component_Prototype = Get_Provider_Swc()
        If Not IsNothing(prov_swc) Then
            prov_swc_name = prov_swc.Name
        End If
        Dim prov_port As Port = Get_Provider_Port()
        If Not IsNothing(prov_port) Then
            prov_port_name = prov_port.Name
        End If
        Dim req_swc As Component_Prototype = Get_Requirer_Swc()
        If Not IsNothing(req_swc) Then
            req_swc_name = req_swc.Name
        End If
        Dim req_port As Port = Get_Requirer_Port()
        If Not IsNothing(req_port) Then
            req_port_name = req_port.Name
        End If

        Me.Name_TextBox.Text = prov_swc_name & "__" & prov_port_name & "__" &
            req_swc_name & "__" & req_port_name
    End Sub

    Private Function Get_Provider_Swc() As Component_Prototype
        If Me.Swc_By_Name.ContainsKey(Prov_Component_ComboBox.Text) Then
            Return Me.Swc_By_Name(Prov_Component_ComboBox.Text)
        Else
            Return Nothing
        End If
    End Function

    Private Function Get_Provider_Port() As Port
        Dim prov_swc As Component_Prototype = Get_Provider_Swc()
        If IsNothing(prov_swc) Then
            Return Nothing
        Else
            Dim prov_swct As Component_Type
            prov_swct = CType(prov_swc.Get_Elmt_From_Prj_By_Id(prov_swc.Ref_Component_Type_Id),
                Component_Type)
            If IsNothing(prov_swct) Then
                Return Nothing
            Else
                Return prov_swct.Get_Port_By_Name(Me.Prov_Port_ComboBox.Text)
            End If
        End If
    End Function

    Private Function Get_Requirer_Swc() As Component_Prototype
        If Me.Swc_By_Name.ContainsKey(Req_Component_ComboBox.Text) Then
            Return Me.Swc_By_Name(Req_Component_ComboBox.Text)
        Else
            Return Nothing
        End If
    End Function

    Private Function Get_Requirer_Port() As Port
        Dim req_swc As Component_Prototype = Get_Requirer_Swc()
        If IsNothing(req_swc) Then
            Return Nothing
        Else
            Dim req_swct As Component_Type
            req_swct = CType(req_swc.Get_Elmt_From_Prj_By_Id(req_swc.Ref_Component_Type_Id),
                Component_Type)
            If IsNothing(req_swct) Then
                Return Nothing
            Else
                Return req_swct.Get_Port_By_Name(Me.Req_Port_ComboBox.Text)
            End If
        End If
    End Function

    Private Sub Update_Prov_Port_ComboBox_Items()
        Me.Prov_Port_ComboBox.Items.Clear()
        Dim prov_swc As Component_Prototype = Get_Provider_Swc()
        If Not IsNothing(prov_swc) Then
            Dim prov_swct As Component_Type
            prov_swct = CType(prov_swc.Get_Elmt_From_Prj_By_Id(prov_swc.Ref_Component_Type_Id),
                Component_Type)
            If Not IsNothing(prov_swct) Then
                For Each prov_port In prov_swct.Provider_Ports
                    Me.Prov_Port_ComboBox.Items.Add(prov_port.Name)
                Next
            End If
        End If
    End Sub

    Private Sub Update_Req_Port_ComboBox_Items()
        Me.Req_Port_ComboBox.Items.Clear()
        Dim req_swc As Component_Prototype = Get_Requirer_Swc()
        If Not IsNothing(req_swc) Then
            Dim req_swct As Component_Type
            req_swct = CType(req_swc.Get_Elmt_From_Prj_By_Id(req_swc.Ref_Component_Type_Id),
                Component_Type)
            If Not IsNothing(req_swct) Then
                For Each req_port In req_swct.Requirer_Ports
                    Me.Req_Port_ComboBox.Items.Add(req_port.Name)
                Next
            End If
        End If
    End Sub

    Private Sub Prov_Swc_Changed() Handles Prov_Component_ComboBox.SelectedValueChanged
        Me.Update_Prov_Port_ComboBox_Items()
        Me.Update_Name()
    End Sub

    Private Sub Prov_Port_Changed() Handles Prov_Port_ComboBox.SelectedValueChanged
        Me.Update_Name()
    End Sub

    Private Sub Req_Swc_Changed() Handles Req_Component_ComboBox.SelectedValueChanged
        Me.Update_Req_Port_ComboBox_Items()
        Me.Update_Name()
    End Sub

    Private Sub Req_Port_Changed() Handles Req_Port_ComboBox.SelectedValueChanged
        Me.Update_Name()
    End Sub

    Public Function Get_Provider_Swc_Identifier() As Guid
        Dim prov_swc As Component_Prototype = Me.Get_Provider_Swc()
        If Not IsNothing(prov_swc) Then
            Return prov_swc.Identifier
        Else
            Return Guid.Empty
        End If
    End Function

    Public Function Get_Provider_Port_Identifier() As Guid
        Dim prov_port As Port = Get_Provider_Port()
        If IsNothing(prov_port) Then
            Return Guid.Empty
        Else
            Return prov_port.Identifier
        End If
    End Function

    Public Function Get_Requirer_Swc_Identifier() As Guid
        Dim req_swc As Component_Prototype = Me.Get_Requirer_Swc()
        If Not IsNothing(req_swc) Then
            Return req_swc.Identifier
        Else
            Return Guid.Empty
        End If
    End Function

    Public Function Get_Requirer_Port_Identifier() As Guid
        Dim req_port As Port = Get_Requirer_Port()
        If IsNothing(req_port) Then
            Return Guid.Empty
        Else
            Return req_port.Identifier
        End If
    End Function

End Class
