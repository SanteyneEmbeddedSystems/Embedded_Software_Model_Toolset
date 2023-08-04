Module SVG

    Public ReadOnly SVG_STROKE_WIDTH As Integer = 1
    Public ReadOnly SVG_MARGE As Integer = 1
    Public ReadOnly SVG_FONT_SIZE As Integer = 12
    Public ReadOnly SVG_TEXT_LINE_HEIGHT As Integer = SVG_FONT_SIZE + SVG_MARGE
    Public ReadOnly SVG_TITLE_HEIGHT As Integer =
        SVG_TEXT_LINE_HEIGHT * 2 + SVG_MARGE + 2 * SVG_STROKE_WIDTH
    Public ReadOnly SVG_BOX_WIDTH As Integer =
        CInt(Software_Element.NB_CHARS_MAX_FOR_SYMBOL * SVG_FONT_SIZE * 0.6 + SVG_STROKE_WIDTH * 2)
    Public ReadOnly NB_CHARS_PER_LINE As Integer = CInt(SVG_BOX_WIDTH / (SVG_FONT_SIZE * 0.6))


    Public Function Get_Title_Rectangle(
            x_pos As Integer,
            y_pos As Integer,
            element_name As String,
            color As String,
            Optional stereotype_name As String = "") As String

        Dim svg_content As String

        ' Add rectangle
        svg_content =
            "  <rect x=""" & x_pos & "px"" y=""" & y_pos & "px"" " &
                "width=""" & SVG_BOX_WIDTH & "px"" height=""" & SVG_TITLE_HEIGHT & "px""" & vbCrLf &
            "    style=""fill:" & color & ";fill-opacity:0.5;" &
                "stroke:" & color & ";stroke-width:" & SVG_STROKE_WIDTH & "px""/>" & vbCrLf

        ' Add stereotype
        If stereotype_name <> "" Then
            svg_content &=
            "  <text style=""font-size:" & SVG_FONT_SIZE - 2 & "px;text-anchor:middle;"" " &
                "x=""" & x_pos + SVG_BOX_WIDTH / 2 & "px"" " &
                "y=""" & y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT - 3 & "px"">&lt;&lt;" &
                    stereotype_name & "&gt;&gt;</text>" & vbCrLf
        End If

        ' Add Name
        svg_content &=
            "  <text style=""text-anchor:middle;font-weight:bold;"" " &
                "x=""" & x_pos + SVG_BOX_WIDTH / 2 & "px"" " &
                "y=""" & y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT * 2 - 3 & "px"">" &
                element_name & "</text>" & vbCrLf

        svg_content &= ""

        Return svg_content

    End Function


    Public Function Get_Multi_Line_Rectangle(
        x_pos As Integer,
        y_pos As Integer,
        lines As List(Of String),
        color As String,
        Optional ByRef rectangle_height As Integer = 0) As String

        Dim svg_content As String

        rectangle_height = lines.Count * SVG_TEXT_LINE_HEIGHT + SVG_MARGE + 2 * SVG_STROKE_WIDTH
        svg_content =
            "  <rect x=""" & x_pos & "px"" y=""" & y_pos & "px"" " &
                "width=""" & SVG_BOX_WIDTH & "px"" " &
                "height=""" & rectangle_height & "px""" & vbCrLf &
            "    style=""fill:" & color & ";fill-opacity:0.2;" &
                "stroke:" & color & ";stroke-width:" & SVG_STROKE_WIDTH & "px""/>" & vbCrLf

        Dim line_idx As Integer = 1
        For Each line In lines
            svg_content &=
            "  <text x=""" & x_pos + 10 & "px"" " &
                "y=""" & y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT * line_idx - 3 & "px"">" &
                line & "</text>" & vbCrLf
            line_idx += 1
        Next

        Return svg_content

    End Function


End Module
