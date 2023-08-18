Imports System.Text.RegularExpressions

Public Class Cardinality

    Public Text As String

    Private ReadOnly Value_List As New List(Of UInteger) From {1}
    Private ReadOnly Kind As Cardinality_Kind
    Private ReadOnly Cardinality_Is_Valid As Boolean = True

    Private Const ANY As UInteger = UInteger.MaxValue

    Private Shared ReadOnly Single_Regex As New Regex("^(\d+|\*)?$")

    Private Shared ReadOnly Bound_Regex As New Regex("^(?<min>(\d+))\.\.(?<max>(\d+|\*))$")

    Private Shared ReadOnly List_Regex As New Regex("^(\d+)(,(\d+))+")

    Private Enum Cardinality_Kind
        SINGLE_VALUE
        BOUNDS
        LIST_OF_VALUES
    End Enum

    Public Sub New(cardinality_string As String)
        Me.Value_List.Clear()
        Dim regex_match As Match = Cardinality.Single_Regex.Match(cardinality_string)
        If regex_match.Success = True Then
            Me.Kind = Cardinality_Kind.SINGLE_VALUE
            Me.Cardinality_Is_Valid = True
            Me.Text = cardinality_string
            If cardinality_string = "*" Then
                Me.Value_List.Add(Cardinality.ANY)
            Else
                Me.Value_List.Add(CUInt(cardinality_string))
            End If
        Else
            regex_match = Cardinality.Bound_Regex.Match(cardinality_string)
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
                    Me.Text = cardinality_string
                    Me.Value_List.Add(lower_bound)
                    Me.Value_List.Add(upper_bound)
                End If
            Else
                regex_match = Cardinality.List_Regex.Match(cardinality_string)
                If regex_match.Success = True Then
                    Me.Cardinality_Is_Valid = True
                    Me.Text = cardinality_string
                    For Each sub_str In cardinality_string.Split(","c)
                        Me.Value_List.Add(CUInt(sub_str))
                    Next
                Else
                    Me.Text = "invalid"
                    Me.Cardinality_Is_Valid = False
                End If
            End If
        End If
    End Sub

    Public Sub New(value As UInteger)
        Me.Cardinality_Is_Valid = True
        Me.Value_List.Add(value)
    End Sub

    Public Sub New()

    End Sub

    Public Function Get_Minimum() As UInteger
        Return Me.Value_List.First()
    End Function

    Public Function Get_Maximum() As UInteger
        Return Me.Value_List.Last()
    End Function

    Public Function Is_Cardinality_Valid() As Boolean
        Return Me.Cardinality_Is_Valid
    End Function

    Public Function Is_One() As Boolean
        If Me.Cardinality_Is_Valid Then
            Return Me.Value_List.First() = 1
        Else
            Return False
        End If
    End Function

    Public Function Get_Value() As String
        Return Me.Text
    End Function

End Class