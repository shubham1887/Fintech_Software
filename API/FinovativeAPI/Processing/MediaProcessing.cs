using Project.Processing;
using Project.StaticModel;
using System;
using System.IO;
using System.Net.Http;
using static NSoup.Parse.Token;

namespace Processing.Processing
{
    public class MediaProcessing
    {
        private bool isWhiteListedFileType(string fName)
        {
            if (fName.ToLower().EndsWith(".png"))
            {
                return true;
            }
            if (fName.ToLower().EndsWith(".jpg"))
            {
                return true;
            }
            if (fName.ToLower().EndsWith(".jpeg"))
            {
                return true;
            }
            if (fName.ToLower().EndsWith(".pdf"))
            {
                return true;
            }
            return false;
        }

        private bool isWhiteListedEkoFileType(string fName)
        {
            if (fName.ToLower().EndsWith(".jpg"))
            {
                return true;
            }
            if (fName.ToLower().EndsWith(".jpeg"))
            {
                return true;
            }
            if (fName.ToLower().EndsWith(".pdf"))
            {
                return true;
            }
            return false;
        }

        public string SaveImage(CustomFile file, string Type, out string failureMessage)
        {
            string path = string.Empty;

            failureMessage = new Validation().ServerDownTimeForFileUpload();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return path;
            }

            if (!isWhiteListedFileType(file.Name.Trim()))
            {
                return path;
            }

            string content = file.Data;

            if (content == null) return path;

            if (content.Trim().Length <= 0) return path;

            if (!new Validation().IsValidFileSize(content))
            {
                return path;
            }

            if (Type.Trim().Length <= 0) return path;

            var imageDataByteArray = Convert.FromBase64String(content.Replace("data:image/jpeg;base64,", ""));

            var imageDataStream = new System.IO.MemoryStream(imageDataByteArray)
            {
                Position = 0
            };
            string imageName = string.Empty;
            string DocType = "OtherDocuments";

            string fileExtensionWithSuffix = "_" + Guid.NewGuid().ToString("N") + ".jpg";

            if (Type.ToLower().Contains(MediaType.KYC_AADHAAR.ToLower()))
            {
                Type = MediaType.KYC_AADHAAR;
            }

            switch (Type)
            {
                case MediaType.IdfyOnBoarding:
                    imageName = MediaType.IdfyOnBoarding;
                    DocType = MediaType.IdfyOnBoarding;
                    break;

                case MediaType.KYC_PAN:
                    imageName = MediaType.KYC_PAN;
                    DocType = MediaType.KYC_PAN;
                    break;

                case MediaType.KYC_GST:
                    imageName = MediaType.KYC_GST;
                    DocType = MediaType.KYC_GST;
                    break;

                case MediaType.KYC_AADHAAR:
                    imageName = MediaType.KYC_AADHAAR;
                    DocType = MediaType.KYC_AADHAAR;
                    break;

                case MediaType.QRCode:
                    imageName = MediaType.QRCode;
                    DocType = MediaType.QRCode;
                    break;

                case MediaType.Slider:
                    imageName = MediaType.Slider;
                    DocType = MediaType.Slider;
                    break;

                case MediaType.OperatorImage:
                    imageName = MediaType.OperatorImage;
                    DocType = MediaType.OperatorImage;
                    break;

                case MediaType.ProfileImage:
                    imageName = MediaType.ProfileImage;
                    DocType = MediaType.ProfileImage;
                    break;

                case MediaType.Invoice_Logo:
                    imageName = MediaType.Invoice_Logo;
                    DocType = MediaType.Invoice_Logo;
                    break;

                case MediaType.Invoice_Signature:
                    imageName = MediaType.Invoice_Signature;
                    DocType = MediaType.Invoice_Signature;
                    break;

                case MediaType.Ticket:
                    imageName = MediaType.Ticket;
                    DocType = MediaType.Ticket;
                    break;

                case MediaType.ReceiptImage:
                    imageName = MediaType.ReceiptImage;
                    DocType = MediaType.ReceiptImage;
                    break;

                case MediaType.News:
                    imageName = MediaType.News;
                    DocType = MediaType.News;
                    break;

                case MediaType.ServiceWiseSlider:
                    imageName = MediaType.ServiceWiseSlider;
                    DocType = MediaType.ServiceWiseSlider;
                    break;

                case MediaType.ChannelImage:
                    imageName = MediaType.ChannelImage;
                    DocType = MediaType.ChannelImage;
                    break;
                case MediaType.PayoutDocument:
                    imageName = MediaType.PayoutDocument;
                    DocType = MediaType.PayoutDocument;
                    break;
                case MediaType.SubscriptionIMG:
                    imageName = MediaType.SubscriptionIMG;
                    DocType = MediaType.SubscriptionIMG;
                    break;

                default:
                    imageName = MediaType.Other;
                    DocType = MediaType.Other;
                    break;
            }

            imageName = imageName + fileExtensionWithSuffix;

            string savepath = Path.Combine(Directory.GetCurrentDirectory(), @"Media/[FOLDER_NAME]");

            savepath = savepath.Replace("[FOLDER_NAME]", GetFolderName(DocType));

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(savepath);

                using FileStream fstream = new FileStream(savepath + "\\" + imageName, FileMode.Create);

                imageDataStream.WriteTo(fstream);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string OriginalPath = "app-docs/[FOLDER_NAME]/" + imageName;
            OriginalPath = OriginalPath.Replace("[FOLDER_NAME]", GetFolderName(DocType));
            path = OriginalPath;

            failureMessage = StatusType.Ok;
            return path;
        }

        public string SaveImageEko(CustomFile file)
        {
            string path = string.Empty;

            string Type = MediaType.Eko_AEPSServiceAgent;

            if (!isWhiteListedEkoFileType(file.Name.Trim()))
            {
                return path;
            }

            string content = file.Data;

            if (content == null) return path;

            if (content.Trim().Length <= 0) return path;

            if (!new Validation().IsValidFileOrImageSizeForEko(content))
            {
                return path;
            }

            if (Type.Trim().Length <= 0) return path;

            var imageDataByteArray = Convert.FromBase64String(content.Replace("data:image/jpeg;base64,", ""));

            var imageDataStream = new System.IO.MemoryStream(imageDataByteArray)
            {
                Position = 0
            };
            string imageName = string.Empty;
            string DocType = "OtherDocuments";

            string fileExtensionWithSuffix = "_" + Guid.NewGuid().ToString("N") + file.Name;

            switch (Type)
            {
                case MediaType.Eko_AEPSServiceAgent:
                    imageName = MediaType.Eko_AEPSServiceAgent;
                    DocType = MediaType.Eko_AEPSServiceAgent;
                    break;

                default:
                    imageName = MediaType.Other;
                    DocType = MediaType.Other;
                    break;
            }

            imageName = imageName + fileExtensionWithSuffix;

            string savepath = Path.Combine(Directory.GetCurrentDirectory(), @"Media/[FOLDER_NAME]");

            savepath = savepath.Replace("[FOLDER_NAME]", GetFolderName(DocType));

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(savepath);

                using FileStream fstream = new FileStream(savepath + "\\" + imageName, FileMode.Create);

                imageDataStream.WriteTo(fstream);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            path = savepath + "\\" + imageName;

            return path;
        }

        public string SaveImagePaysrpint(CustomFile file)
        {
            string path = string.Empty;

            string Type = MediaType.PaysrpintPayout;

            if (!isWhiteListedFileType(file.Name.Trim()))
            {
                return path;
            }

            string content = file.Data;

            if (content == null) return path;

            if (content.Trim().Length <= 0) return path;

            if (!new Validation().IsValidFileSize(content))
            {
                return path;
            }

            if (Type.Trim().Length <= 0) return path;

            var imageDataByteArray = Convert.FromBase64String(content.Replace("data:image/jpeg;base64,", ""));

            var imageDataStream = new System.IO.MemoryStream(imageDataByteArray)
            {
                Position = 0
            };
            string imageName = string.Empty;
            string DocType = "OtherDocuments";

            string fileExtensionWithSuffix = "_" + Guid.NewGuid().ToString("N") + file.Name;

            switch (Type)
            {
                case MediaType.PaysrpintPayout:
                    imageName = MediaType.PaysrpintPayout;
                    DocType = MediaType.PaysrpintPayout;
                    break;

                default:
                    imageName = MediaType.Other;
                    DocType = MediaType.Other;
                    break;
            }

            imageName = imageName + fileExtensionWithSuffix;

            string savepath = Path.Combine(Directory.GetCurrentDirectory(), @"Media/[FOLDER_NAME]");

            savepath = savepath.Replace("[FOLDER_NAME]", GetFolderName(DocType));

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(savepath);

                using FileStream fstream = new FileStream(savepath + "\\" + imageName, FileMode.Create);

                imageDataStream.WriteTo(fstream);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            path = savepath + "\\" + imageName;

            return path;
        }

        public string SaveImageIdfy(CustomFile file,string UserPath)
        {
            string path = string.Empty;

            string Type = MediaType.PaysrpintPayout;

            if (!isWhiteListedFileType(file.Name.Trim()))
            {
                return path;
            }

            string content = file.Data;

            if (content == null) return path;

            if (content.Trim().Length <= 0) return path;

            if (!new Validation().IsValidFileSize(content))
            {
                return path;
            }

            if (Type.Trim().Length <= 0) return path;

            var imageDataByteArray = Convert.FromBase64String(content.Replace("data:image/jpeg;base64,", ""));

            var imageDataStream = new System.IO.MemoryStream(imageDataByteArray)
            {
                Position = 0
            };
            string imageName = string.Empty;
           
            string fileExtensionWithSuffix = "_" + Guid.NewGuid().ToString("N") + file.Name;

            

            imageName = imageName + fileExtensionWithSuffix;

            string savepath = Path.Combine(Directory.GetCurrentDirectory(), UserPath);

            
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(savepath);

                using FileStream fstream = new FileStream(savepath + "\\" + imageName, FileMode.Create);

                imageDataStream.WriteTo(fstream);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            path = savepath + "\\" + imageName;

            return path;
        }

        private string GetFolderName(string docType)
        {
            docType = docType.ToLower();
            string FolderName = "";
            if (docType.Contains("KYC".ToLower()))
                FolderName = "KYC";
            if (docType.Contains("IdfyOnBoarding".ToLower()))
                FolderName = "IdfyOnBoarding";
            if (docType.Contains("Eko_AEPSServiceAgent".ToLower()))
                FolderName = "Eko_AEPSServiceAgent";

            if (docType.Contains("QRCode".ToLower()))
                FolderName = "QRCode";

            if (docType.Contains("Slider".ToLower()))
                FolderName = "Slider";

            if (docType.Contains("OperatorImage".ToLower()))
                FolderName = "OperatorImage";

            if (docType.Contains("ProfileImage".ToLower()))
                FolderName = "ProfileImage";

            if (docType.Contains("Invoice-Logo".ToLower()))
                FolderName = "InvoiceLogo";

            if (docType.Contains("Invoice-Signature".ToLower()))
                FolderName = "InvoiceSignature";

            if (docType.Contains("Ticket".ToLower()))
                FolderName = "Ticket";

            if (docType.Contains("OtherDocuments".ToLower()))
                FolderName = "OtherDocuments";

            if (docType.Contains("ReceiptImage".ToLower()))
                FolderName = "ReceiptImage";

            if (docType.Contains("News".ToLower()))
                FolderName = "News";

            if (docType.Contains("ServiceWiseSlider".ToLower()))
                FolderName = "ServiceWiseSlider";

            if (docType.Contains("ChannelImage".ToLower()))
                FolderName = "ChannelImage";

            return FolderName;
        }

        public string SaveImageIdfyOnboarding(string url, string fileName, string UserName)
        {
            string path = string.Empty;
            string Type = MediaType.IdfyOnBoarding;

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("Invalid URL.");
                return path;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to download image. Status code: {response.StatusCode}");
                        return path;
                    }

                     byte[] imageData = response.Content.ReadAsByteArrayAsync().Result;

                    if (imageData == null || imageData.Length == 0)
                    {
                        Console.WriteLine("No image data received.");
                        return path;
                    }
                    string DocType = "OtherDocuments";
                    string docName = string.Empty;
                    switch (Type)
                    {
                        case MediaType.IdfyOnBoarding:
                            docName = MediaType.IdfyOnBoarding;
                            DocType = MediaType.IdfyOnBoarding;
                            break;
                        default:
                            docName = MediaType.Other;
                            DocType = MediaType.Other;
                            break;
                    }

                    string baseFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media");
                    string documentFolderPath = Path.Combine(baseFolderPath, DocType);
                    string userFolderPath = Path.Combine(documentFolderPath, UserName);

                    Directory.CreateDirectory(userFolderPath);

                    string fileExtension = Path.GetExtension(new Uri(url).LocalPath);
                    fileName = fileName + (fileExtension != "" ? fileExtension : ".png");
                    path = Path.Combine(userFolderPath, fileName);

                
                    File.WriteAllBytes(path, imageData);

                    string OriginalPath = "app-docs/[FOLDER_NAME]/" + UserName + "/" + fileName;
                    OriginalPath = OriginalPath.Replace("[FOLDER_NAME]", GetFolderName(DocType));
                    path = OriginalPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return path;
        }
    


        public string SavePdfIdfyOnboarding(string URL, string FileName, string UserName)
        {
            string path = string.Empty;
            string Type = MediaType.IdfyOnBoarding;

            if (string.IsNullOrWhiteSpace(URL)) return path;

            try
            {
                using HttpClient client = new HttpClient();
                var response = client.GetAsync(URL).Result;

                if (!response.IsSuccessStatusCode) return path;

                var contentType = response.Content.Headers.ContentType.MediaType;

                // Ensure that the content is a PDF
                if (contentType != "application/pdf") return path;

                var pdfDataByteArray = response.Content.ReadAsByteArrayAsync().Result;

                if (pdfDataByteArray == null || pdfDataByteArray.Length == 0) return path;

                string DocType = "OtherDocuments";
                string docName = string.Empty;
                switch (Type)
                {
                    case MediaType.IdfyOnBoarding:
                        docName = MediaType.IdfyOnBoarding;
                        DocType = MediaType.IdfyOnBoarding;
                        break;
                    default:
                        docName = MediaType.Other;
                        DocType = MediaType.Other;
                        break;
                }

                string baseFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media");
                string documentFolderPath = Path.Combine(baseFolderPath, DocType);
                string userFolderPath = Path.Combine(documentFolderPath, UserName);

                Directory.CreateDirectory(userFolderPath);
                string fileExtension = ".pdf"; // Since we're specifically handling PDFs
                FileName = FileName + fileExtension;
                string fullPath = Path.Combine(userFolderPath, FileName);

                File.WriteAllBytes(fullPath, pdfDataByteArray);

                string OriginalPath = "app-docs/[FOLDER_NAME]/" + UserName + "/" + FileName;
                OriginalPath = OriginalPath.Replace("[FOLDER_NAME]", GetFolderName(DocType));
                path = OriginalPath;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return path;
        }

        public string SaveImagesIdfy(CustomFile file, string Type, string FileName, string UserName)
        {
            string path = string.Empty;

            string failureMessage = new Validation().ServerDownTimeForFileUpload();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return path;
            }

            if (!isWhiteListedFileType(file.Name.Trim()))
            {
                return path;
            }

            string content = file.Data;

            if (content == null) return path;

            if (content.Trim().Length <= 0) return path;

            if (!new Validation().IsValidFileSize(content))
            {
                return path;
            }

            if (Type.Trim().Length <= 0) return path;

            var imageDataByteArray = Convert.FromBase64String(content.Replace("data:image/jpeg;base64,", ""));

            var imageDataStream = new System.IO.MemoryStream(imageDataByteArray)
            {
                Position = 0
            };
            string imageName = string.Empty;
            string DocType = "OtherDocuments";

         
       

            switch (Type)
            {
                case MediaType.IdfyOnBoarding:
                    imageName = MediaType.IdfyOnBoarding;
                    DocType = MediaType.IdfyOnBoarding;
                    break;

                default:
                    imageName = MediaType.Other;
                    DocType = MediaType.Other;
                    break;
            }
            string baseFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media");
            string documentFolderPath = Path.Combine(baseFolderPath, DocType);
            string userFolderPath = Path.Combine(documentFolderPath, UserName);

            Directory.CreateDirectory(userFolderPath);
            string fileExtension = ".jpg";
            if ("Images/jpg" == file.Type)
            {
                fileExtension = ".jpg";
            }
            if ("Images/png" == file.Type)
            {
                fileExtension = ".png";
            }
            if ("application/pdf" == file.Type)
            {
                fileExtension = ".pdf";
            }
            
            FileName = FileName + fileExtension;
            string fullPath = Path.Combine(userFolderPath, FileName);

            File.WriteAllBytes(fullPath, imageDataByteArray);

            string OriginalPath = "app-docs/[FOLDER_NAME]/" + UserName + "/" + FileName;
            OriginalPath = OriginalPath.Replace("[FOLDER_NAME]", GetFolderName(DocType));
            path = OriginalPath;
            failureMessage = StatusType.Ok;
            return path;
        }






    }
}