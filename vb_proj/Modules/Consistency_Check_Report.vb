Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization


Public Enum E_Report_File_Format
    CSV
    XHTML
End Enum

Public Class Consistency_Check_Report

    Private ReadOnly Model_Name As String

    ' Dictionary of ( dictionary of Report_Item by rule ID) by elment ID
    Private ReadOnly Items As _
        New Dictionary(Of Guid, Dictionary(Of String, Consistency_Check_Report_Item))


    Public Sub New(name As String)
        Me.Model_Name = name
    End Sub

    Public Function Generate_Report(
            format As E_Report_File_Format,
            directory As String,
            show_only_not_compliant_checkings As Boolean,
            add_rules_description As Boolean) As String
        Select Case format
            Case E_Report_File_Format.CSV
                Return Me.Generate_CSV_Report(
                    directory,
                    show_only_not_compliant_checkings,
                    add_rules_description)
            Case E_Report_File_Format.XHTML
                Return Me.Generate_XHTML_Report(
                    directory,
                    show_only_not_compliant_checkings,
                    add_rules_description)
            Case Else
                Return ""
        End Select
    End Function

    Private Function Generate_CSV_Report(
            directory As String,
            show_only_not_compliant_checkings As Boolean,
            add_rules_description As Boolean) As String

        ' Compute report file path
        Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")
        Dim file_name As String = Me.Model_Name & "_Consistency_Check_Report_" & date_str & ".csv"
        Dim file_path As String = directory & "\" & file_name

        Dim file_stream As New StreamWriter(file_path, False)

        ' Write header
        If add_rules_description = False Then
            file_stream.WriteLine(
                "Path" & vbTab & "Meta-class" & vbTab &
                "Rule ID" & vbTab & "Compliance" & vbTab & "Message")
        Else
            file_stream.WriteLine(
                "Path" & vbTab & "Meta-class" & vbTab &
                "Rule ID" & vbTab & "Compliance" & vbTab & "Rule description" & vbTab & "Message")
        End If

        ' Write items
        For Each items_by_rule In Me.Items.Values
            For Each item In items_by_rule.Values
                item.Write_In_CSV(
                file_stream,
                show_only_not_compliant_checkings,
                add_rules_description)
            Next
        Next
        file_stream.Close()

        Return file_path

    End Function

    Private Function Generate_XHTML_Report(
            directory As String,
            show_only_not_compliant_checkings As Boolean,
            add_rules_description As Boolean) As String

        ' Compute report file path
        Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")
        Dim file_name As String = Me.Model_Name & "_Consistency_Check_Report_" & date_str & ".html"
        Dim file_path As String = directory & "\" & file_name

        Dim html_report As New XmlDocument

        Dim html_root As XmlElement = html_report.CreateElement("html")
        html_report.AppendChild(html_root)

        Dim html_head As XmlElement = html_report.CreateElement("head")
        html_root.AppendChild(html_head)

        Dim html_title As XmlElement = html_report.CreateElement("title")
        html_title.AppendChild(html_report.CreateTextNode("Consistency report"))
        html_head.AppendChild(html_title)

        Dim html_body As XmlElement = html_report.CreateElement("body")
        html_root.AppendChild(html_body)

        Dim html_table As XmlElement = html_report.CreateElement("table")
        html_body.AppendChild(html_table)

        ' Write header
        Dim html_row As XmlElement = html_report.CreateElement("tr")
        html_table.AppendChild(html_row)

        Dim path_cell As XmlElement = html_report.CreateElement("th")
        path_cell.AppendChild(html_report.CreateTextNode("Path"))
        html_row.AppendChild(path_cell)

        Dim metaclass_cell As XmlElement = html_report.CreateElement("th")
        metaclass_cell.AppendChild(html_report.CreateTextNode("Meta-class"))
        html_row.AppendChild(metaclass_cell)

        Dim rule_id_cell As XmlElement = html_report.CreateElement("th")
        rule_id_cell.AppendChild(html_report.CreateTextNode("Rule ID"))
        html_row.AppendChild(rule_id_cell)

        Dim compliance_cell As XmlElement = html_report.CreateElement("th")
        compliance_cell.AppendChild(html_report.CreateTextNode("Compliance"))
        html_row.AppendChild(compliance_cell)

        If add_rules_description = True Then
            Dim rule_desc_cell As XmlElement = html_report.CreateElement("th")
            rule_desc_cell.AppendChild(html_report.CreateTextNode("Rule description"))
            html_row.AppendChild(rule_desc_cell)
        End If

        Dim message_cell As XmlElement = html_report.CreateElement("th")
        message_cell.AppendChild(html_report.CreateTextNode("Message"))
        html_row.AppendChild(message_cell)

        ' Write items
        For Each items_by_rule In Me.Items.Values
            For Each item In items_by_rule.Values
                item.Write_In_HTML(
                html_report,
                html_table,
                show_only_not_compliant_checkings,
                add_rules_description)
            Next
        Next

        ' Genrate HTML file
        Dim ser As New XmlSerializer(GetType(XmlNode))
        Dim writer As TextWriter = New StreamWriter(file_path)
        ser.Serialize(writer, html_report)
        writer.Close()

        Return file_path

    End Function

    Public Sub Add_Item(report_item As Consistency_Check_Report_Item)

        Dim elmt_id As Guid = report_item.Get_Element_Id()
        Dim rule_id As String = report_item.Get_Rule_Id()

        Dim items_by_rule As Dictionary(Of String, Consistency_Check_Report_Item)

        If Me.Items.ContainsKey(elmt_id) Then
            items_by_rule = Me.Items(elmt_id)
            items_by_rule.Add(rule_id, report_item)
        Else
            items_by_rule = New Dictionary(Of String, Consistency_Check_Report_Item) From {
                {rule_id, report_item}}
            Me.Items.Add(elmt_id, items_by_rule)
        End If

    End Sub

    Public Sub Get_Consistency_Check_Result(
            element_id As Guid,
            rule_id As String,
            ByRef check_found As Boolean,
            ByRef check_result As Boolean)

        check_found = False

        If Me.Items.ContainsKey(element_id) Then
            Dim items_by_rule As Dictionary(Of String, Consistency_Check_Report_Item)
            items_by_rule = Me.Items(element_id)
            If items_by_rule.ContainsKey(rule_id) Then
                check_found = True
                Dim item As Consistency_Check_Report_Item = items_by_rule(rule_id)
                check_result = item.Get_Compliance()
            End If
        End If

    End Sub

End Class


Public Class Modeling_Rule

    Private ReadOnly Identifier As String
    Private ReadOnly Description As String


    Public Sub New(id As String, desc As String)
        Me.Identifier = id
        Me.Description = desc
    End Sub

    Public Function Get_Identifier() As String
        Return Me.Identifier
    End Function

    Public Function Get_Description() As String
        Return Me.Description
    End Function

End Class


Public Class Consistency_Check_Report_Item

    Private ReadOnly Element As Software_Element
    Private ReadOnly Rule As Modeling_Rule
    Private Rule_Complied As Boolean
    Private Message As String


    Public Sub New(element As Software_Element, rule As Modeling_Rule)
        Me.Element = element
        Me.Rule = rule
        Me.Rule_Complied = False
    End Sub

    Public Sub Write_In_CSV(
            file_stream As StreamWriter,
            show_only_not_compliant_checkings As Boolean,
            add_rules_description As Boolean)

        If show_only_not_compliant_checkings = True And Me.Rule_Complied = True Then
            Exit Sub
        End If

        file_stream.Write(Me.Element.Get_Path() & vbTab)
        file_stream.Write(Me.Element.Get_Metaclass_Name() & vbTab)
        file_stream.Write(Me.Rule.Get_Identifier() & vbTab)
        file_stream.Write(Me.Rule_Complied & vbTab)
        If add_rules_description = True Then
            file_stream.Write(Me.Rule.Get_Description() & vbTab)
        End If
        file_stream.Write(Me.Message)
        file_stream.WriteLine()

    End Sub

    Public Sub Write_In_HTML(
            report As XmlDocument,
            table As XmlElement,
            show_only_not_compliant_checkings As Boolean,
            add_rules_description As Boolean)

        If show_only_not_compliant_checkings = True And Me.Rule_Complied = True Then
            Exit Sub
        End If

        Dim html_row As XmlElement = report.CreateElement("tr")
        table.AppendChild(html_row)

        Dim path_cell As XmlElement = report.CreateElement("td")
        path_cell.AppendChild(report.CreateTextNode(Me.Element.Get_Path()))
        html_row.AppendChild(path_cell)

        Dim metaclass_cell As XmlElement = report.CreateElement("td")
        metaclass_cell.AppendChild(report.CreateTextNode(Me.Element.Get_Metaclass_Name()))
        html_row.AppendChild(metaclass_cell)

        Dim rule_id_cell As XmlElement = report.CreateElement("td")
        rule_id_cell.AppendChild(report.CreateTextNode(Me.Rule.Get_Identifier()))
        html_row.AppendChild(rule_id_cell)

        Dim compliance_cell As XmlElement = report.CreateElement("td")
        compliance_cell.AppendChild(report.CreateTextNode(Me.Rule_Complied.ToString()))
        html_row.AppendChild(compliance_cell)

        If add_rules_description = True Then
            Dim rule_desc_cell As XmlElement = report.CreateElement("td")
            rule_desc_cell.AppendChild(report.CreateTextNode(Me.Rule.Get_Description()))
            html_row.AppendChild(rule_desc_cell)
        End If

        Dim message_cell As XmlElement = report.CreateElement("td")
        message_cell.AppendChild(report.CreateTextNode(Me.Message))
        html_row.AppendChild(message_cell)

    End Sub

    Public Sub Set_Compliance(rule_compliance As Boolean)
        Me.Rule_Complied = rule_compliance
    End Sub

    Public Function Get_Compliance() As Boolean
        Return Me.Rule_Complied
    End Function

    Public Sub Set_Message(message As String)
        Me.Message = message
    End Sub

    Public Function Get_Element_Id() As Guid
        Return Me.Element.Identifier
    End Function

    Public Function Get_Rule_Id() As String
        Return Me.Rule.Get_Identifier()
    End Function

End Class