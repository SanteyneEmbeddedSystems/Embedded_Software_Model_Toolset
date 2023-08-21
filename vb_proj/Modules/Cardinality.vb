Imports System.Text.RegularExpressions

Public Class Cardinality

    Public Text As String

    Private ReadOnly Value_List As New List(Of UInteger) From {1}
    Private Cardinality_Is_Valid As Boolean = True
    Private Is_Computed As Boolean = False

    Private Const ANY As UInteger = UInteger.MaxValue

    Private Const INVALID As String = "invalid"

    Private Shared ReadOnly Single_Regex As New Regex("^(\d+|\*)?$")

    Private Shared ReadOnly Bound_Regex As New Regex("^(?<min>(\d+))\.\.(?<max>(\d+|\*))$")

    Private Shared ReadOnly List_Regex As New Regex("^(\d+)(,(\d+))+")


    Public Sub New(cardinality_string As String)
        Me.Text = cardinality_string
        Me.Compute_From_Text()
    End Sub

    Public Sub New(value As UInteger)
        Me.Is_Computed = True
        If value > 0 Then
            Me.Cardinality_Is_Valid = True
            Me.Text = value.ToString
            Me.Value_List.Add(value)
        Else
            Me.Cardinality_Is_Valid = False
            Me.Text = INVALID
        End If
    End Sub

    Public Sub New()
    End Sub

    Private Sub Compute_From_Text()
        Me.Is_Computed = True
        Me.Value_List.Clear()
        Dim regex_match As Match = Cardinality.Single_Regex.Match(Me.Text)
        If regex_match.Success = True Then
            Me.Cardinality_Is_Valid = True
            If Me.Text = "*" Then
                Me.Value_List.Add(Cardinality.ANY)
            Else
                Me.Value_List.Add(CUInt(Me.Text))
            End If
        Else
            regex_match = Cardinality.Bound_Regex.Match(Me.Text)
            If regex_match.Success = True Then
                ' Get lower bound
                Dim min_value_str As String
                min_value_str = regex_match.Groups.Item("min").Value
                Dim lower_bound As UInteger = CUInt(min_value_str)
                ' Get upper bound
                Dim max_value_str As String
                max_value_str = regex_match.Groups.Item("max").Value
                Dim upper_bound As UInteger
                If max_value_str = "*" Then
                    upper_bound = Cardinality.ANY
                Else
                    upper_bound = CUInt(max_value_str)
                End If
                ' Test bounds
                If lower_bound > upper_bound Then
                    Me.Cardinality_Is_Valid = False
                Else
                    Me.Cardinality_Is_Valid = True
                    Me.Value_List.Add(lower_bound)
                    Me.Value_List.Add(upper_bound)
                End If
            Else
                regex_match = Cardinality.List_Regex.Match(Me.Text)
                If regex_match.Success = True Then
                    Me.Cardinality_Is_Valid = True
                    For Each sub_str In Me.Text.Split(","c)
                        Me.Value_List.Add(CUInt(sub_str))
                    Next
                Else
                    Me.Text = INVALID
                    Me.Cardinality_Is_Valid = False
                End If
            End If
        End If
    End Sub

    Public Function Get_Minimum() As UInteger
        If Not Me.Is_Computed Then
            Me.Compute_From_Text()
        End If
        Return Me.Value_List.First()
    End Function

    Public Function Get_Maximum() As UInteger
        If Not Me.Is_Computed Then
            Me.Compute_From_Text()
        End If
        Return Me.Value_List.Last()
    End Function

    Public Function Is_Cardinality_Valid() As Boolean
        If Not Me.Is_Computed Then
            Me.Compute_From_Text()
        End If
        Return Me.Cardinality_Is_Valid
    End Function

    Public Function Is_One() As Boolean
        If Not Me.Is_Computed Then
            Me.Compute_From_Text()
        End If
        If Me.Cardinality_Is_Valid Then
            Return Me.Value_List.First() = 1
        Else
            Return False
        End If
    End Function

    Public Function Is_Any() As Boolean
        If Not Me.Is_Computed Then
            Me.Compute_From_Text()
        End If
        If Me.Cardinality_Is_Valid Then
            Return Me.Value_List.First() = ANY
        Else
            Return False
        End If
    End Function

    Public Function Get_Value() As String
        If Not Me.Is_Computed Then
            Me.Compute_From_Text()
        End If
        Return Me.Text
    End Function

End Class