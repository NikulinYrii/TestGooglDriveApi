using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v2.Data.File;

namespace GoogleDriveApp
{
    class Program
    {
        private static string[] scope = { DriveService.Scope.Drive };
        private static string aplicationName = "Quickstart";
        static void Main(string[] args)
        {
            UserCredential credential = GetUserCredential();
            DriveService service = GetDriveService(credential);

            IList<File> files = service.Files.List().Execute().Items;
            foreach(var file in files)
            {
                Console.WriteLine("File title: {0}, id {1}", file.Title, file.Id);
            }
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
    }
}
