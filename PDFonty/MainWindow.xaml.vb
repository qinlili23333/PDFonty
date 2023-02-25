Imports iText
Imports System.Environment
Imports Microsoft.Win32
Imports System.Windows.Forms
Imports iText.Kernel.Pdf

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
End Class
