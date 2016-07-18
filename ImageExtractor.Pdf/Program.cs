using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;

namespace ImageExtractor.Pdf
{
    public class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                var pdfFileName =
                    Console.ReadLine();
                if (string.IsNullOrEmpty(pdfFileName))
                {
                    Console.WriteLine("Please Provide a filename near this project!\n" +
                                      "For example <pdfFilename>");
                    Console.ReadLine();
                }
                else
                {
                    ExtractImageFromPdfFile(
                        Path.Combine(System.IO.Directory.GetCurrentDirectory(),pdfFileName));
                }
            }
        }

        private static void ExtractImageFromPdfFile(string pdfFile)
        {
            string imgPath;
            PdfReader pdfReader = new PdfReader(pdfFile);
            for (var i = 1; i <= pdfReader.NumberOfPages; i++)
            {
                PdfDictionary dictionary = pdfReader.GetPageN(i);
                PdfDictionary res = (PdfDictionary) PdfReader.GetPdfObject(dictionary.
                    Get(PdfName.RESOURCES));
                PdfDictionary xobj = (PdfDictionary) PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    if (obj.IsIndirect())
                    {
                        PdfDictionary tg = (PdfDictionary) PdfReader.GetPdfObject(obj);
                        PdfName type = (PdfName) PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));
                        if (!type.Equals(PdfName.IMAGE))
                        {
                            continue;
                        }
                        int XrefIndex =
                            Convert.ToInt32(
                                ((PRIndirectReference) obj).Number.ToString(
                                    System.Globalization.CultureInfo.InvariantCulture));
                        PdfObject pdfObj = pdfReader.GetPdfObject(XrefIndex);
                        PdfStream pdfStrem = (PdfStream) pdfObj;
                        byte[] bytes = PdfReader.GetStreamBytesRaw((PRStream) pdfStrem);
                        if (bytes == null)
                        {
                            continue;
                        }
                        using (System.IO.MemoryStream memStream = new System.IO.MemoryStream(bytes))
                        {
                            memStream.Position = 0;
                            System.Drawing.Image img = System.Drawing.Image.FromStream(memStream);
                            if (!Directory.Exists("ImageFolder"))
                                Directory.CreateDirectory("ImageFolder");

                            string path = Path.Combine("ImageFolder", String.Format(@"Photo{0}.jpg", i));
                            System.Drawing.Imaging.EncoderParameters parms =
                                new System.Drawing.Imaging.EncoderParameters(1);
                            parms.Param[0] =
                                new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression,
                                    0);
                            var jpegEncoder =
                                ImageCodecInfo.GetImageEncoders()
                                    .ToList()
                                    .Find(x => x.FormatID == ImageFormat.Jpeg.Guid);
                            img.Save(path, jpegEncoder, parms);

                        }
                    }
                }
            }
        }
    }
}
