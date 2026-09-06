using Project.BAL;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class FileDeleteProcessing
    {
        public void DeleteFiles()
        {
            bool isDevelopment = new Util().isDevelopment();
            if (isDevelopment == true)
            {
                return;
            }

            Parallel.Invoke(
            () =>
            {
                DeleteUnUsedServiceWiseSliderFiles();
            }
               ,
               () =>
               {
                   DeleteTransactionFiles();
               }
               ,
               () =>
               {
                   DeleteBackupFiles();
               }
               ,
               () =>
               {
                   DeleteUnUsedOperatorImages();
               }
               ,
               () =>
               {
                   DeleteUnUsedKYCFiles();
               }
               ,
               () =>
               {
                   DeleteUnUsedNewsFiles();
               });
        }

        private void DeleteBackupFiles()
        {
            try
            {
                string Path = Directory.GetCurrentDirectory();

                string DirectoryPath = String.Concat(Path, @"\Media\Backup");

                if (!Directory.Exists(DirectoryPath))
                    return;

                string[] files = Directory.GetFiles(DirectoryPath);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < DateTime.Now.AddDays(-7))
                    { File.SetAttributes(file, FileAttributes.Normal); fi.Delete(); }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void DeleteTransactionFiles()
        {
            try
            {
                string Path = Directory.GetCurrentDirectory();

                string DirectoryPath = String.Concat(Path, @"\Media\TransactionExcels");

                if (!Directory.Exists(DirectoryPath))
                    return;

                string[] files = Directory.GetFiles(DirectoryPath);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < DateTime.Now.AddDays(-3))
                    { File.SetAttributes(file, FileAttributes.Normal); fi.Delete(); }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        private void DeleteUnUsedKYCFiles()
        {
            try
            {
                return;

                string Path = Directory.GetCurrentDirectory();

                string DirectoryPath = String.Concat(Path, @"\Media\KYC");

                if (!Directory.Exists(DirectoryPath))
                    return;

                // string DirectoryPath = String.Concat("D:/Finnovative/API/finovativeapi/FinovativeAPI/Media/KYC/");
                string[] files = Directory.GetFiles(DirectoryPath);

                foreach (string file in files)
                {
                    string[] arr = file.Split(@"\KYC\");
                    if (arr.Length <= 1)
                    {
                        continue;
                    }
                    string Filename = arr[1];

                    string path = "app-docs/KYC/" + Filename;
                    if (!new UserDocumentMasterBal().CheckFileExists(path))
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddMonths(-1))
                        { File.SetAttributes(file, FileAttributes.Normal); fi.Delete(); }
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex); return;
            }
        }

        private void DeleteUnUsedNewsFiles()
        {
            try
            {
                string Path = Directory.GetCurrentDirectory();

                string DirectoryPath = String.Concat(Path, @"\Media\News");

                if (!Directory.Exists(DirectoryPath))
                    return;

                string[] files = Directory.GetFiles(DirectoryPath);

                foreach (string file in files)
                {
                    string[] arr = file.Split(@"\News\");
                    if (arr.Length <= 1)
                    {
                        continue;
                    }
                    string Filename = arr[1];

                    string path = "app-docs/News/" + Filename;
                    if (!new NewsBAL().CheckFileExists(path))
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddMonths(-1))
                        { File.SetAttributes(file, FileAttributes.Normal); fi.Delete(); }
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex); return;
            }
        }

        private void DeleteUnUsedServiceWiseSliderFiles()
        {
            try
            {
                string Path = Directory.GetCurrentDirectory();

                string DirectoryPath = String.Concat(Path, @"\Media\ServiceWiseSlider");

                if (!Directory.Exists(DirectoryPath))
                    return;

                string[] files = Directory.GetFiles(DirectoryPath);

                foreach (string file in files)
                {
                    string[] arr = file.Split(@"\ServiceWiseSlider\");
                    if (arr.Length <= 1)
                    {
                        continue;
                    }
                    string Filename = arr[1];
                    string path = "app-docs/ServiceWiseSlider/" + Filename;

                    if (!new ServiceWiseSliderSettingBal().CheckFileExists(path))
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.CreationTime < DateTime.Now.AddDays(-15))
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            fi.Delete();
                            //new Logger().Info("FileDeleteProcessing", "FileDeleted", fi.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex); return;
            }
        }

        private void DeleteUnUsedOperatorImages()
        {
            try
            {
                string Path = Directory.GetCurrentDirectory();
                string DirectoryPath = String.Concat(Path, @"\Media\OperatorImage");

                if (!Directory.Exists(DirectoryPath))
                    return;

                string[] files = Directory.GetFiles(DirectoryPath);

                foreach (string file in files)
                {
                    string[] arr = file.Split(@"\OperatorImage\");
                    if (arr.Length <= 1)
                    {
                        continue;
                    }
                    string Filename = arr[1];

                    string path = "app-docs/OperatorImage/" + Filename;
                    if (!new OperatorMasterBAL().CheckFileExists(path))
                    {
                        FileInfo fi = new FileInfo(file);
                        File.SetAttributes(file, FileAttributes.Normal);
                        fi.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}