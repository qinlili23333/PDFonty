using System;
using System.Collections.Generic;
using static System.Environment;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using PDFWV2;

namespace PDFonty
{

    public partial class MainWindow
    {
        private string CurrentFile = "";
        private PdfDocument PDFDocument;
        private PDFEngine WV2Engine;
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "选择一个PDF文件",
                Multiselect = false,
                RestoreDirectory = true,
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "PDF文件|*.pdf"
            };
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CurrentFile = fd.FileName;
                this.FileName.Content = CurrentFile;
                ScanPDF();
            }
            else
            {
                this.Status.Content = "没有选择文件";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.SaveFileDialog()
            {
                CheckPathExists = true,
                Title = "选择一个PDF文件",
                RestoreDirectory = true,
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "PDF文件|*.pdf",
                FileName = System.IO.Path.GetFileName(CurrentFile)
            };
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var Writer = new PdfWriter(fd.FileName);
                var NewPdf = new PdfDocument(Writer);
                PDFDocument.CopyPagesTo(1, PDFDocument.GetNumberOfPages(), NewPdf);
                NewPdf.Close();
                this.Status.Content = "保存成功";
            }
        }
        private object ScanPDF()
        {
            this.Status.Content = "正在扫描文件";
            PDFDocument = new PdfDocument(new PdfReader(CurrentFile));
            int Pages = PDFDocument.GetNumberOfPages();
            this.Status.Content = "共计" + Pages.ToString() + "页";
            return null;
        }

        private void AntiAcrobat_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.Forms.MessageBox.Show("你确定这个PDF需要字体去重吗？若查看PDF字体列表所有字体名称前都有6或6的整数倍个随机字符，才需要执行该操作。对不需要该操作的PDF执行可能会导致PDF显示异常。", "PDFonty", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (!(result == System.Windows.Forms.DialogResult.Yes))
            {
                return;
            }
            if (string.IsNullOrEmpty(CurrentFile))
            {
                this.Status.Content = "未选择文件";
                return;
            }
            this.Status.Content = "正在读取字体列表";
            var FontStatistic = new Dictionary<string, int>();
            var OriginFontStatistic = new Dictionary<string, int>();
            int ObjectNum = PDFDocument.GetNumberOfPdfObjects();
            int CurrentObjectNum = 1;
            while (CurrentObjectNum <= ObjectNum)
            {
                var CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum);
                if (CurrentObject is not null && CurrentObject.GetObjectType() == 3)
                {
                    PdfDictionary Dict = (PdfDictionary)CurrentObject;
                    if (ReferenceEquals(Dict.GetAsName(PdfName.Type), PdfName.Font))
                    {
                        // 得到原始的字体名称
                        string FontName = Dict.GetAsName(PdfName.BaseFont).ToString();
                        // 格式化为标准字体名称
                        FontName = FontName.Substring(FontName.IndexOf("+") + 1);
                        // 统计去除前字符后的字体名
                        // 一定有傻逼往这个输入框里填不是整数的东西，所以做一个检测，去酒吧点炒饭的有一个算一个都是傻逼
                        if (!new Regex("^[0-9]*$").IsMatch(this.AcroFontLength.Text))
                        {
                            this.Status.Content = "你在只允许数字的输入框里输了什么寄吧玩意";
                            return;
                        }
                        int SubLength = int.Parse(this.AcroFontLength.Text);
                        if (FontName.Length > SubLength)
                        {
                            string OriginFontName = FontName.Substring(SubLength);
                            if (!FontStatistic.ContainsKey(FontName))
                            {
                                FontStatistic[FontName] = 1;
                                if (!OriginFontStatistic.ContainsKey(OriginFontName))
                                {
                                    OriginFontStatistic[OriginFontName] = 1;
                                }
                                else
                                {
                                    OriginFontStatistic[OriginFontName] += 1;
                                }
                            }
                            else
                            {
                                FontStatistic[FontName] += 1;
                            }
                        }
                    }
                }
                CurrentObjectNum += 1;
            }
            this.Status.Content = "统计完成，开始施工";
            // 只出现一次的就没必要施工了
            string ToReplaceFont = "";
            foreach (var Font in OriginFontStatistic)
            {
                if (Font.Value == 1)
                {
                    OriginFontStatistic.Remove(Font.Key);
                }
                else
                {
                    ToReplaceFont += Font.Key + ",";
                }
            }
            if (string.IsNullOrEmpty(ToReplaceFont))
            {
                this.Status.Content = "没找到需要去重的字体，可能是参数不正确";
                return;
            }
            if (OriginFontStatistic.Count > 100)
            {
                result = System.Windows.Forms.MessageBox.Show("去重后的字体似乎仍然有点多，还要继续吗？" + NewLine + "这种情况很可能是参数不正确，试试对照说明调节参数", "PDFonty", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (!(result == System.Windows.Forms.DialogResult.Yes))
                {
                    this.Status.Content = "去重后的字体仍然很多，用户终止处理";
                    return;
                }
            }
            result = System.Windows.Forms.MessageBox.Show("需要去重的字体包括：" + ToReplaceFont + NewLine + "确定继续？", "PDFonty", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (!(result == System.Windows.Forms.DialogResult.Yes))
            {
                return;
            }
            CurrentObjectNum = 1;
            while (CurrentObjectNum <= ObjectNum)
            {
                var CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum);
                if (CurrentObject is not null && CurrentObject.GetObjectType() == 3)
                {
                    PdfDictionary Dict = (PdfDictionary)CurrentObject;
                    if (ReferenceEquals(Dict.GetAsName(PdfName.Type), PdfName.Font))
                    {
                        // 得到原始的字体名称
                        string FontName = Dict.GetAsName(PdfName.BaseFont).ToString();
                        // 格式化为标准字体名称
                        FontName = FontName.Substring(FontName.IndexOf("+") + 1);
                        int SubLength = int.Parse(this.AcroFontLength.Text);
                        if (FontName.Length > SubLength)
                        {
                            string OriginFontName = FontName.Substring(SubLength);
                            if (OriginFontStatistic.ContainsKey(OriginFontName))
                            {
                                Dict.Remove(PdfName.BaseFont);
                                Dict.Remove(PdfName.FontDescriptor);
                                Dict.Put(PdfName.BaseFont, new PdfString(OriginFontName));
                            }
                        }
                    }
                }
                CurrentObjectNum += 1;
            }
            this.Status.Content = "处理完成";
        }

        private void FontReplace_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentFile))
            {
                this.Status.Content = "未选择文件";
                return;
            }
            this.Status.Content = "正在替换字体";
            int ObjectNum = PDFDocument.GetNumberOfPdfObjects();
            int CurrentObjectNum = 1;
            int ReplaceCounter = 0;
            while (CurrentObjectNum <= ObjectNum)
            {
                var CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum);
                if (CurrentObject is not null && CurrentObject.GetObjectType() == 3)
                {
                    PdfDictionary Dict = (PdfDictionary)CurrentObject;
                    if (ReferenceEquals(Dict.GetAsName(PdfName.Type), PdfName.Font))
                    {
                        // 得到原始的字体名称
                        string FontName = Dict.GetAsName(PdfName.BaseFont).ToString();
                        // 格式化为标准字体名称
                        FontName = FontName.Substring(FontName.IndexOf("+") + 1);
                        if ((FontName ?? "") == (this.OriginFont.Text ?? ""))
                        {
                            Dict.Remove(PdfName.BaseFont);
                            Dict.Remove(PdfName.FontDescriptor);
                            Dict.Remove(PdfName.W);
                            Dict.Put(PdfName.BaseFont, new PdfName(this.NewFont.Text));
                            ReplaceCounter += 1;
                        }
                    }
                }
                CurrentObjectNum += 1;
            }
            if (ReplaceCounter > 0)
            {
                this.Status.Content = "替换" + ReplaceCounter.ToString() + "处";
            }
            else
            {
                this.Status.Content = "没有找到需要替换的字体";
            }
        }

        private void FixEncoding_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentFile))
            {
                this.Status.Content = "未选择文件";
                return;
            }
            this.Status.Content = "正在修复编码";
            int ObjectNum = PDFDocument.GetNumberOfPdfObjects();
            int CurrentObjectNum = 1;
            int ReplaceCounter = 0;
            while (CurrentObjectNum <= ObjectNum)
            {
                var CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum);
                if (CurrentObject is not null && CurrentObject.GetObjectType() == 3)
                {
                    PdfDictionary Dict = (PdfDictionary)CurrentObject;
                    if (ReferenceEquals(Dict.GetAsName(PdfName.Type), PdfName.Font))
                    {
                        var enc = Dict.GetAsName(PdfName.Encoding);
                        Dict.Put(PdfName.Encoding, new PdfName(this.NewEncoding.Text));
                        ReplaceCounter += 1;
                    }
                }
                CurrentObjectNum += 1;
            }
            if (ReplaceCounter > 0)
            {
                this.Status.Content = "修复" + ReplaceCounter.ToString() + "处";
            }
            else
            {
                this.Status.Content = "没有找到需要修复编码的字体";
            }
        }

        private void RebuildFont_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentFile))
            {
                this.Status.Content = "未选择文件";
                return;
            }
            this.Status.Content = "正在处理字体";
            int ObjectNum = PDFDocument.GetNumberOfPdfObjects();
            int CurrentObjectNum = 1;
            int ReplaceCounter = 0;
            while (CurrentObjectNum <= ObjectNum)
            {
                var CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum);
                if (CurrentObject is not null && CurrentObject.GetObjectType() == 3)
                {
                    PdfDictionary Dict = (PdfDictionary)CurrentObject;
                    if (ReferenceEquals(Dict.GetAsName(PdfName.Type), PdfName.Font) && Dict.GetAsName(PdfName.BaseFont).ToString().IndexOf("+") > 0)
                    {
                        try
                        {
                            // 得到原始的字体名称
                            string FontName = Dict.GetAsName(PdfName.BaseFont).ToString();
                            // 格式化为标准字体名称
                            FontName = FontName.Substring(FontName.IndexOf("+") + 1);
                            var Font = PDFDocument.GetFont(Dict);
                            Dict.Remove(PdfName.BaseFont);
                            Dict.Put(PdfName.BaseFont, Font.GetPdfObject());
                            ReplaceCounter += 1;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                CurrentObjectNum += 1;
            }
            if (ReplaceCounter > 0)
            {
                this.Status.Content = "修复" + ReplaceCounter.ToString() + "处";
            }
            else
            {
                this.Status.Content = "没有找到需要处理的字体";
            }
        }

        private void UnembedAll_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentFile))
            {
                this.Status.Content = "未选择文件";
                return;
            }
            this.Status.Content = "正在处理字体";
            int ObjectNum = PDFDocument.GetNumberOfPdfObjects();
            int CurrentObjectNum = 1;
            int ReplaceCounter = 0;
            while (CurrentObjectNum <= ObjectNum)
            {
                var CurrentObject = PDFDocument.GetPdfObject(CurrentObjectNum);
                if (CurrentObject is not null && CurrentObject.GetObjectType() == 3)
                {
                    PdfDictionary Dict = (PdfDictionary)CurrentObject;
                    if (ReferenceEquals(Dict.GetAsName(PdfName.Type), PdfName.Font) && Dict.GetAsName(PdfName.BaseFont).ToString().IndexOf("+") > 0)
                    {
                        PdfName BaseFont;
                        if (this.AlsoReplaceFont.IsChecked == true)
                        {
                            BaseFont = new PdfName(this.NewFont.Text);
                        }
                        else
                        {
                            BaseFont = new PdfName(Dict.GetAsName(PdfName.BaseFont).ToString().Substring(8));
                        }
                        Dict.Put(PdfName.BaseFont, BaseFont);
                        Dict.Remove(PdfName.CIDToGIDMap);
                        var fontDescriptor = Dict.GetAsDictionary(PdfName.FontDescriptor);
                        if (fontDescriptor is not null)
                        {
                            fontDescriptor.Put(PdfName.FontName, BaseFont);
                            fontDescriptor.Remove(PdfName.FontFile2);
                        }
                        ReplaceCounter += 1;
                    }
                }
                CurrentObjectNum += 1;
            }
            if (ReplaceCounter > 0)
            {
                this.Status.Content = "取消" + ReplaceCounter.ToString() + "个字体内嵌";
            }
            else
            {
                this.Status.Content = "没有找到内嵌的字体";
            }
        }

        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            if (WV2Engine is null)
            {
                var Options = new PDFWV2Options()
                {
                    DefaultEngine = Engines.Adobe,
                    DebugTool = true
                };
                var Instance = await PDFWV2Instance.CreateInstance(Options);
                WV2Engine = await Instance.CreateEngine();
            }
            if (string.IsNullOrEmpty(CurrentFile))
            {
                this.Status.Content = "未选择文件";
                return;
            }
            var FileStream = new MemoryStream();
            var Writer = new PdfWriter(FileStream);
            Writer.SetCloseStream(false);
            var NewPdf = new PdfDocument(Writer);
            PDFDocument.CopyPagesTo(1, PDFDocument.GetNumberOfPages(), NewPdf);
            NewPdf.Close();
            WV2Engine.ViewStream(FileStream);
        }
    }
}