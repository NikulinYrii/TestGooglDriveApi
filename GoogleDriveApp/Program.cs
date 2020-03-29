using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace GoogleDriveApp
{
    class Program
    {
        private static string[] scope = { DriveService.Scope.Drive };
        private static string aplicationName = "Quickstart";
        private static string folderId = "1jrWZPEewWJ3rtvWHGLf0H6dKMpUHLg3C";

        static void Main(string[] args)
        {
            UserCredential credential = GetUserCredential();
            DriveService service = GetDriveService(credential);

            var files = GetFiles(service);
            foreach (var file in files)
            {
                string saveFile = file.Name + "." + file.MimeType.Split('/')[1];
                Console.WriteLine("File title: {0}, id {1}", file.Name, file.Id);
                Console.WriteLine(saveFile);
                Console.WriteLine("----------------------------------------------------");
                if (file.MimeType.Split('/')[1] == "pdf")
                {
                    DownloadFileFromDrive(service, file.Id, @"D:\source\"+saveFile);
                }
            }

            //UploadFileToDrive(service, "test", @"C:\Users\y.nikulin\Desktop\standart_stoika v24012020.pdf", "application/pdf");

            //DownloadFileFromDrive(service, "1xrdjU36ZXF0irvq4Byr4TPi0ugZT9Gmk", @"D:\source\");

            Console.ReadLine();
        }

        private static UserCredential GetUserCredential()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string creadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                creadPath = Path.Combine(creadPath, "driveApiCredentials", "drive-credential.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scope,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(creadPath, true)).Result;
            }
        }

        private static DriveService GetDriveService(UserCredential credential)
        {
            return new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = aplicationName
                });
        }

        private static IList<File> GetFiles(DriveService service)
        {
            return service.Files.List().Execute().Files;
        }

        private static string UploadFileToDrive(DriveService service, string fileName, string filePath, string contentType)
        {
            var fileMetadata = new File();
            fileMetadata.Name = fileName;
            fileMetadata.Parents = new List<string> { folderId };

            FilesResource.CreateMediaUpload request;

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, contentType);
                request.Upload();
            }

            var file = request.ResponseBody;

            return file.Id;
        }

        private static void DownloadFileFromDrive(DriveService service, string fileId, string filePath)
        {
            var request = service.Files.Get(fileId);

            using(var memoryStream = new MemoryStream())
            {
                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        case DownloadStatus.Completed:
                            Console.WriteLine("Download Complete");
                            break;
                        case DownloadStatus.Failed:
                            Console.WriteLine("Download Failed");
                            break;
                    }
                };

                request.Download(memoryStream);

                using(var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                }
            }
        }
    }
}
