Imports iText
Imports System.Environment
Imports Microsoft.Win32
Imports System.Windows.Forms

Class MainWindow
    Dim CurrentFile = ""
    Private Sub Open_Click(sender As Object, e As RoutedEventArgs) Handles Open.Click
        Dim fd As New Windows.Forms.OpenFileDialog() With {
                                .CheckFileExists = True,
                                .CheckPathExists = True,
                                .Title = "选择一个PDF文件",
                                .Multiselect = False,
                                .RestoreDirectory = True,
                                .InitialDirectory = CurrentDirectory
                                }
        If fd.ShowDialog() = Windows.Forms.DialogResult.OK Then
            CurrentFile = fd.FileName
            FileName.Content = CurrentFile
        Else
        End If
    End Sub

End Class
