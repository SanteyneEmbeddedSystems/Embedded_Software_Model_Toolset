Imports System.IO

Module Utilities

    Public Function Make_Relative_Path(from_path As String, to_path As String) As String
        Dim from_uri As Uri
        Dim to_uri As Uri
        Dim relative_path As String = to_path
        from_uri = New Uri(from_path)
        to_uri = New Uri(to_path)
        If from_uri.Scheme = to_uri.Scheme Then
            Dim relative_uri As Uri
            relative_uri = from_uri.MakeRelativeUri(to_uri)
            relative_path = Uri.UnescapeDataString(relative_uri.ToString())
            If to_uri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase) Then
                relative_path = relative_path.Replace(
                                Path.AltDirectorySeparatorChar,
                                Path.DirectorySeparatorChar)
            End If
        End If
        Return relative_path
    End Function


    Public Function Split_String(
            input_string As String,
            nb_char_per_line As Integer) As List(Of String)

        Dim result As New List(Of String)

        Dim remaning_str As String = input_string
        While (remaning_str.Length > nb_char_per_line)
            If Char.IsWhiteSpace(remaning_str(nb_char_per_line)) Then
                result.Add(remaning_str.Substring(0, nb_char_per_line))
                remaning_str = remaning_str.Substring(nb_char_per_line)
            Else
                Dim white_space_idx As Integer = nb_char_per_line - 1
                While Not Char.IsWhiteSpace(remaning_str(white_space_idx))
                    white_space_idx -= 1
                End While
                result.Add(remaning_str.Substring(0, white_space_idx))
                remaning_str = remaning_str.Substring(white_space_idx + 1)
            End If
        End While

        result.Add(remaning_str)

        Return result

    End Function

End Module
