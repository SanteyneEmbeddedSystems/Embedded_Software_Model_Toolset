Module SVG

    Public Const SVG_STROKE_WIDTH As Integer = 1
    Public Const SVG_VERTICAL_MARGIN As Integer = 3
    Public Const SVG_FONT_SIZE As Integer = 12
    Public Const SVG_TEXT_LINE_HEIGHT As Integer = SVG_FONT_SIZE + SVG_VERTICAL_MARGIN
    Public Const SVG_TEXT_MARGIN As Integer = 10
    Public Const SVG_TITLE_HEIGHT As Integer =
        SVG_TEXT_LINE_HEIGHT * 2 + SVG_VERTICAL_MARGIN + 2 * SVG_STROKE_WIDTH


    Public Structure SVG_POINT
        Public X_Pos As Integer
        Public Y_Pos As Integer
    End Structure

    Public Function Get_Box_Width(nb_char As Integer) As Integer
        Return Get_Text_Width(nb_char) + SVG_TEXT_MARGIN * 2 + SVG_STROKE_WIDTH * 2
    End Function

    Public Function Get_Text_Width(nb_char As Integer) As Integer
        Return CInt(nb_char * 6.5)
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
        svg_content = Get_SVG_Rectangle(
            x_pos,
            y_pos,
            rectangle_witdh,
            SVG_TITLE_HEIGHT,
            color,
            "0.5",
            SVG_STROKE_WIDTH)

        ' Add stereotype
        If stereotype_name <> "" Then
            svg_content &= Get_SVG_Text(
                x_pos + rectangle_witdh \ 2,
                y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT - 3,
                "&lt;&lt;" & stereotype_name & "&gt;&gt;",
                SVG_FONT_SIZE - 2,
                False,
                False,
                E_Text_Anchor.ANCHOR_MIDDLE)
        End If

        ' Add Name
        svg_content &= Get_SVG_Text(
            x_pos + rectangle_witdh \ 2,
            y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT * 2 - 3,
            element_name,
            SVG_FONT_SIZE,
            True,
            is_interface,
            E_Text_Anchor.ANCHOR_MIDDLE)

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

        If rectangle_height = 0 Then
            rectangle_height = Get_SVG_Retangle_Height(lines.Count)
        End If
        svg_content = Get_SVG_Rectangle(
            x_pos,
            y_pos,
            rectangle_witdh,
            rectangle_height,
            color,
            "0.2",
            SVG_STROKE_WIDTH)

        Dim line_idx As Integer = 1
        For Each line In lines
            svg_content &= Get_SVG_Text(
            x_pos + SVG_TEXT_MARGIN,
            y_pos + SVG_STROKE_WIDTH + SVG_TEXT_LINE_HEIGHT * line_idx - 3,
            line,
            SVG_FONT_SIZE,
            False,
            False)
            line_idx += 1
        Next

        Return svg_content

    End Function

    Public Function Get_SVG_Retangle_Height(nb_lines As Integer) As Integer
        Return nb_lines * SVG_TEXT_LINE_HEIGHT + SVG_VERTICAL_MARGIN + 2 * SVG_STROKE_WIDTH
    End Function

    Public Function Get_SVG_Rectangle(
            x_pos As Integer,
            y_pos As Integer,
            width As Integer,
            height As Integer,
            color As String,
            opacity As String,
            Optional stroke_width As Integer = 1) As String
        Dim svg_content As String =
            "  <rect x=""" & x_pos & "px""" &
            " y=""" & y_pos & "px""" &
            " width=""" & width & "px"" height=""" & height & "px""" & vbCrLf &
            "    style=""fill:" & color & ";fill-opacity:" & opacity & ";" &
                "stroke:" & color & ";stroke-width:" & stroke_width & "px""/>" & vbCrLf
        Return svg_content
    End Function

    Public Function Get_SVG_Line(
            x1_pos As Integer,
            y1_pos As Integer,
            x2_pos As Integer,
            y2_pos As Integer,
            color As String,
            Optional is_dashed As Boolean = False,
            Optional marker_id As String = "",
            Optional stroke_width As Integer = 1) As String
        Dim dash_array As String = ""
        If is_dashed Then
            dash_array = "4,4"
        End If
        Dim marker As String = ""
        If marker_id <> "" Then
            marker = "marker-end:url(#" & marker_id & ");"
        End If
        Dim svg_content As String =
            "  <line x1=""" & x1_pos & "px""" &
                " y1=""" & y1_pos & "px""" &
                " x2=""" & x2_pos & "px""" &
                " y2=""" & y2_pos & "px""" & vbCrLf &
                "    style=""stroke:" & color &
                ";stroke-width:" & stroke_width & "px" &
                ";stroke-dasharray:" & dash_array & ";" &
                marker & """" &
                "/>" & vbCrLf
        Return svg_content
    End Function

    Public Function Get_SVG_Horizontal_Line(
            x1_pos As Integer,
            y_pos As Integer,
            length As Integer,
            color As String,
            Optional stroke_width As Integer = 1) As String
        Dim svg_content As String =
            "  <line x1=""" & x1_pos & "px""" &
                " y1=""" & y_pos & "px""" &
                " x2=""" & x1_pos + length & "px""" &
                " y2=""" & y_pos & "px""" & vbCrLf &
                "    style=""stroke:" & color & ";stroke-width:" & stroke_width & "px""/>" & vbCrLf
        Return svg_content
    End Function

    Public Function Get_SVG_Circle(
            x_pos As Integer,
            y_pos As Integer,
            radius As Integer,
            color As String,
            opacity As String,
            Optional stroke_width As Integer = 1) As String
        Dim svg_content As String =
            "  <circle cx=""" & x_pos & "px""" &
                " cy=""" & y_pos & "px""" &
                " r=""" & radius & "px""" & vbCrLf &
                "    style=""fill:" & color & ";fill-opacity:" & opacity & ";" &
                "stroke:" & color & ";stroke-width:" & stroke_width & "px""/>" & vbCrLf
        Return svg_content
    End Function

    Public Function Get_SVG_Haf_Moon(
            x_pos As Integer,
            y_pos As Integer,
            radius As Integer,
            color As String,
            Optional stroke_width As Integer = 1) As String
        Dim svg_content As String =
            "  <path d=""M " &
            x_pos + radius & " " &
            y_pos - radius & " " &
            "A " & radius & " " & radius & " 0 0 0 " &
            x_pos + radius & " " &
            y_pos + radius & " """ &
            " style=""stroke:" & color & ";fill:none;stroke-width:" & stroke_width & ";""/>" &
            vbCrLf
        Return svg_content
    End Function

    Public Function Get_Open_Arrow_Marker() As String
        Dim svg_content As String _
            = "  <marker id=""open_arrow"" style=""overflow:visible"" orient=""auto"">" & vbCrLf &
                "  <line style=""stroke:currentColor"" stroke-dasharray=""1,0"" " &
                    "x1=""0px"" y1=""0px"" x2=""-10px"" y2=""-5px""/>" & vbCrLf &
                "  <line style=""stroke:currentColor"" stroke-dasharray=""1,0"" " &
                    "x1=""0px"" y1=""0px"" x2=""-10px"" y2=""5px""/>" & vbCrLf &
                "  </marker>" & vbCrLf
        Return svg_content
    End Function

    Public Enum E_Text_Anchor
        ANCHOR_START
        ANCHOR_MIDDLE
        ANCHOR_END
    End Enum

    Public Function Get_SVG_Text(
            x_pos As Integer,
            y_pos As Integer,
            text As String,
            font_size As Integer,
            is_bold As Boolean,
            is_italic As Boolean,
            Optional anchor As E_Text_Anchor = E_Text_Anchor.ANCHOR_START) As String
        Dim font_style As String = ""
        If is_italic Then
            font_style = "font-style:italic;"
        End If
        Dim font_weight As String = ""
        If is_bold Then
            font_weight = "font-weight:bold;"
        End If
        Dim text_anchor As String = ""
        Select Case anchor
            Case E_Text_Anchor.ANCHOR_MIDDLE
                text_anchor = "text-anchor:middle;"
            Case E_Text_Anchor.ANCHOR_END
                text_anchor = "text-anchor:end;"
        End Select
        Dim svg_content As String =
            "  <text style=""font-size:" & font_size & "px;" &
                text_anchor & font_weight & font_style & """" &
                " x=""" & x_pos & "px""" &
                " y=""" & y_pos & "px"">" &
                text & "</text>" & vbCrLf
        Return svg_content
    End Function

End Module
