Module SVG

    Public Const SVG_STROKE_WIDTH As Integer = 1
    Public Const SVG_VERTICAL_MARGIN As Integer = 1
    Public Const SVG_FONT_SIZE As Integer = 12
    Public Const SVG_TEXT_LINE_HEIGHT As Integer = SVG_FONT_SIZE + SVG_VERTICAL_MARGIN
    Public Const SVG_TEXT_MARGIN As Integer = 10
    Public Const SVG_TITLE_HEIGHT As Integer =
        SVG_TEXT_LINE_HEIGHT * 2 + SVG_VERTICAL_MARGIN + 2 * SVG_STROKE_WIDTH

    Public Function Get_Box_Witdh(nb_char As Integer) As Integer
        Return CInt(nb_char * 6.5 + SVG_TEXT_MARGIN * 2 + SVG_STROKE_WIDTH * 2)
    End Function

    Public Function Get_Title_Rectangle(
            x_pos As Integer,
            y_pos As Integer,
            element_name As String,
            color As String,
            rectangle_witdh As Integer,
            Optional stereotype_name As String = "",
            Optional is_interface As Boolean = False) As String

        Dim svg_content As String

        ' Add rectangle
        svg_content =
            "  <rect x=""" & x_pos & "px"" y=""" & y_pos & "px"" " &
                "width=""" & rectangle_witdh & "px"" height=""" & SVG_TITLE_HEIGHT & "px""" & vbCrLf &
            "    style=""fill:" & color & ";fill-opacity:0.5;" &
                "stroke:" & color & ";stroke-width:" & SVG_STROKE_WIDTH & "px""/>" & vbCrLf

        ' Add stereotype
        If stereotype_name <> "" Then
            svg_content &=
            "  <text style=""font-size:" & SVG_FONT_SIZE - 2 & "px;text-anchor:middle;"" " &
                "x=""" & x_pos + rectangle_witdh \ 2 & "px"" " &
                "y=""" & y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT - 3 & "px"">&lt;&lt;" &
                    stereotype_name & "&gt;&gt;</text>" & vbCrLf
        End If

        ' Add Name
        Dim font_style As String = ""
        If is_interface = True Then
            font_style = "font-style:italic;"
        End If
        svg_content &=
            "  <text style=""text-anchor:middle;font-weight:bold;" & font_style & """ " &
                "x=""" & x_pos + rectangle_witdh \ 2 & "px"" " &
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
        rectangle_witdh As Integer,
        Optional ByRef rectangle_height As Integer = 0) As String

        Dim svg_content As String

        rectangle_height = lines.Count * SVG_TEXT_LINE_HEIGHT _
            + SVG_VERTICAL_MARGIN + 2 * SVG_STROKE_WIDTH
        svg_content =
            "  <rect x=""" & x_pos & "px"" y=""" & y_pos & "px"" " &
                "width=""" & rectangle_witdh & "px"" " &
                "height=""" & rectangle_height & "px""" & vbCrLf &
            "    style=""fill:" & color & ";fill-opacity:0.2;" &
                "stroke:" & color & ";stroke-width:" & SVG_STROKE_WIDTH & "px""/>" & vbCrLf

        Dim line_idx As Integer = 1
        For Each line In lines
            svg_content &=
            "  <text x=""" & x_pos + SVG_TEXT_MARGIN & "px"" " &
                "y=""" & y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT * line_idx - 3 & "px"">" &
                line & "</text>" & vbCrLf
            line_idx += 1
        Next

        Return svg_content

    End Function


End Module
