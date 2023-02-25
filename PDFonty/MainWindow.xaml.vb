﻿Imports iText
Imports System.Environment
Imports Microsoft.Win32
Imports System.Windows.Forms
Imports iText.Kernel.Pdf
Imports Microsoft.VisualBasic.Logging
Imports System.Text.RegularExpressions

Class MainWindow
    Dim CurrentFile As String = ""
    Dim PDFDocument As PdfDocument
    Private Sub Open_Click(sender As Object, e As RoutedEventArgs) Handles Open.Click
        Dim fd As New Windows.Forms.OpenFileDialog() With {
                                .CheckFileExists = True,
                                .CheckPathExists = True,
                                .Title = "选择一个PDF文件",
                                .Multiselect = False,
                                .RestoreDirectory = True,
                                .InitialDirectory = CurrentDirectory,
                                .Filter = "PDF文件|*.pdf"
                                }
        If fd.ShowDialog() = Windows.Forms.DialogResult.OK Then
            CurrentFile = fd.FileName
            FileName.Content = CurrentFile
            ScanPDF()
        Else
            Status.Content = "没有选择文件"
        End If
    End Sub
    Private Function ScanPDF()
        Status.Content = "正在扫描文件"
        PDFDocument = New PdfDocument(New PdfReader(CurrentFile))
        Dim Pages = PDFDocument.GetNumberOfPages
        Status.Content = "共计" + Pages.ToString + "页"
        Return Nothing
    End Function

    Private Sub AntiAcrobat_Click(sender As Object, e As RoutedEventArgs) Handles AntiAcrobat.Click
        Dim result As DialogResult = MessageBox.Show("你确定这个PDF需要字体去重吗？若查看PDF字体列表所有字体名称前都有6或6的整数倍个随机字符，才需要执行该操作。对不需要该操作的PDF执行可能会导致PDF显示异常。", "PDFonty", MessageBoxButtons.YesNo)
        If Not result = System.Windows.Forms.DialogResult.Yes Then
            Return
        End If
        If CurrentFile = "" Then
            Status.Content = "未选择文件"
            Return
        End If
        Status.Content = "正在读取字体列表"
        Dim FontStatistic As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
        Dim OriginFontStatistic As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
        Dim ObjectNum = PDFDocument.GetNumberOfPdfObjects
        Dim CurrentObjectNum As Integer = 1
        While CurrentObjectNum <= ObjectNum
            Dim CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum)
            If CurrentObject IsNot Nothing AndAlso CurrentObject.GetObjectType() = 3 Then
                Dim Dict As PdfDictionary = CurrentObject
                If Dict.GetAsName(PdfName.Type) Is PdfName.Font Then
                    '得到原始的字体名称
                    Dim FontName = Dict.GetAsName(PdfName.BaseFont).ToString()
                    '格式化为标准字体名称
                    FontName = FontName.Substring(FontName.IndexOf("+") + 1)
                    '统计去除前字符后的字体名
                    '一定有傻逼往这个输入框里填不是整数的东西，所以做一个检测，去酒吧点炒饭的有一个算一个都是傻逼
                    If Not New Regex("^[0-9]*$").IsMatch(AcroFontLength.Text) Then
                        Status.Content = "你在只允许数字的输入框里输了什么寄吧玩意"
                        Return
                    End If
                    Dim SubLength = Int32.Parse(AcroFontLength.Text)
                    If FontName.Length > SubLength Then
                        Dim OriginFontName = FontName.Substring(SubLength)
                        If Not FontStatistic.ContainsKey(FontName) Then
                            FontStatistic(FontName) = 1
                            If Not OriginFontStatistic.ContainsKey(OriginFontName) Then
                                OriginFontStatistic(OriginFontName) = 1
                            Else
                                OriginFontStatistic(OriginFontName) += 1
                            End If
                        Else
                            FontStatistic(FontName) += 1
                        End If
                    End If
                End If
            End If
            CurrentObjectNum += 1
        End While
        Status.Content = "统计完成，开始施工"
        Dim ToReplaceFonts
        For Each Font In OriginFontStatistic
            If Font.Value > 1 Then

            End If
        Next
        Status.Content = "处理完成"
    End Sub
End Class
