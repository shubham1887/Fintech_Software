using QRCoder;
using System;
using System.Drawing;
using System.IO;

namespace Project.BAL
{
    public class QRCodeBAL
    {
        public string getQRImage(string input)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            byte[] b = BitmapToBytesCode(qrCodeImage);
            string Result = "data:image/png;base64," + Convert.ToBase64String(b);
            return Result;
        }

        private Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}