using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GappFilePerms
{
    public class MplGoogleDriveHelper
    {
        public static int m_downloadCounter = 0;
        public static int m_uploadCounter = 0;
        List<string> m_getAllId = new List<string>();
        List<string> m_getAllId_down = new List<string>();
        public List<string> m_diffList = new List<string>();
        public static IList<File> m_Files = new List<File>();
        List<string> m_getAllId_init = new List<string>();

        /// <summary>
        /// Create a new Directory.
        public File createDirectory(DriveService service,string m_c_directoryName,string m_p_directoryName)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File();
            fileMetadata.Name = m_c_directoryName;
            fileMetadata.Parents = new List<string> { GetGDriveFolderId(m_p_directoryName, service) };
            fileMetadata.MimeType = "application/vnd.google-apps.folder";
            var request = service.Files.Create(fileMetadata);
            request.Fields = "id,parents";
            var folder = request.Execute();
            Console.WriteLine("Folder ID: " + folder.Id);
            return null;
        }


        //List all FIles in Google Drive
        public static IList<File> GetFiles(DriveService service, string search)
        {
            try
            {
                //List all of the files and directories for the current user.  
                FilesResource.ListRequest list = service.Files.List();
                list.Fields = "files(id,parents,name,mimeType)";
                if (search != null)
                {
                    list.Q = search;
                }
                FileList filesFeed = list.Execute();

                while (filesFeed.Files != null)
                {
                    foreach (File item in filesFeed.Files)
                    {
                        m_Files.Add(item);
                    }
                    if (filesFeed.NextPageToken == null)
                    {
                        break;
                    }
                    list.PageToken = filesFeed.NextPageToken;
                    filesFeed = list.Execute();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return m_Files;
        }

        //Get MimeType 
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        //Get Shareable Link of Files
        public string GetShareableLink(DriveService service, string fileId)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            // listRequest.Fields = "files(id,webContentLink,parents)";
            listRequest.Fields = "files(webContentLink,id)";
            String returnFile = String.Empty;
            var files = listRequest.Execute().Files;
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Id == fileId)
                    // if (file.Parents != null && file.Parents[0] == fileId)
                    {
                        returnFile = file.WebContentLink;
                        break;
                    }
                    //break;
                }
            }
            return returnFile;
        }
        //uploading all files from PC in Google Drive folder
        public File uploadFileInFolder(DriveService service, string c_directory)
        {
            Form1 form1 = new Form1();
            var fileMetadata = new Google.Apis.Drive.v3.Data.File();
            fileMetadata.Parents = new List<string> { GetGDriveFolderId(c_directory, service) };
            FilesResource.CreateMediaUpload request;
            //Google.Apis.Upload.IUploadProgress x;
            for (int indexOfFile = 5; indexOfFile <= 8; indexOfFile++)
            {
                fileMetadata.Name = "Rebook.jpg";
                using (var stream = new System.IO.FileStream("C:/Users/Kishore/Downloads/image" + indexOfFile + ".jpg",
                    System.IO.FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream, "image/jpeg");
                    request.Fields = "id";
                    request.Upload();
                    var file = request.ResponseBody;
                }
            }
            return null;
        }

        ///Get Folder Id 
        public string GetGDriveFolderId(string Name, DriveService service)
        {
            String returnId = String.Empty;
            String returnFileName = String.Empty;
            string Q = "mimeType = 'application/vnd.google-apps.folder'";
            IList<Google.Apis.Drive.v3.Data.File> _Files = MplGoogleDriveHelper.GetFiles(service, Q);
            foreach (var file in _Files)
            {
                if (file.Name == Name)
                {
                    returnId = file.Id;
                }
            }
            return returnId;
        }


        //List files in child folder and store File Id in a LIst
        public IList<File> listFilesInitial(DriveService service)

        {
          
            m_getAllId_init.Clear();
            string queryFolder = "name='MplSample1' and mimeType = 'application/vnd.google-apps.folder' and trashed=false";
            IList<Google.Apis.Drive.v3.Data.File> gFolder = MplGoogleDriveHelper.GetFiles(service, queryFolder);

            string queryFiles = "mimeType = 'image/jpeg'";
            IList<Google.Apis.Drive.v3.Data.File> gFiles = MplGoogleDriveHelper.GetFiles(service, queryFiles);

            if (gFiles != null && gFolder != null)
            {
                foreach (Google.Apis.Drive.v3.Data.File item in gFiles)
                {
                    if (null != item.Parents && gFolder[0].Id == item.Parents[0])
                    {
                        if (!m_getAllId_init.Contains(item.Id))
                        {
                            m_getAllId_init.Add(item.Id);
                        }
                    }

                    m_getAllId = m_getAllId_init;
                }
            }
            return null;
        }


        //Downloads the Difference list
        public void downloadList(DriveService service)
        {
            bool flag = true;
            do
            {
                listFilesInitial(service);
                m_diffList = compareList();
                if (m_diffList.Count != 0)
                {
                    foreach (var value in m_diffList)
                    {
                        DownloadFile(service, value);
                        m_getAllId_down.Add(value);
                    }
                }
                else
                {
                    flag = false;
                }

            } while (flag);
        }


        //Comparing Initial List with Downloaded List
        public List<string> compareList()
        {
            m_diffList.Clear();
            foreach (var id in m_getAllId)
            {
                if (!m_getAllId_down.Contains(id))
                {
                    m_diffList.Add(id);
                }

            }
            return m_diffList;
        }

        public static int incrementMethod()
        {
            m_downloadCounter++;
            return 0;
        }


        //Downloading The Files in a location
        public string DownloadFile(DriveService service, string fileId)
        {
            string setFileName = "Rebook_Dow";
            string link = GetShareableLink(service, fileId);
            var stream = service.HttpClient.GetStreamAsync(link);
            var result = stream.Result;
            using (var fileStream = System.IO.File.Create(@"C:\Users\Kishore\Downloads\" + setFileName + fileId + ".jpg"))
            {
                incrementMethod();
                result.CopyTo(fileStream);
            }
            return null;
        }

    }
}
