using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.FileStorage
{
    public class FileStorageClient
    {
        private string _storageAccountConnectionString;
        public FileStorageClient(string connectionString)
        {
            _storageAccountConnectionString = connectionString;
        }

        public async Task<byte[]> GetFileContent(string shareName, string fileName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageAccountConnectionString);
            var fileClient = storageAccount.CreateCloudFileClient();
            var share = fileClient.GetShareReference(shareName.ToLower());

            if (share.Exists())
            {
                var rootDir = share.GetRootDirectoryReference();
                var file = rootDir.GetFileReference(fileName);

                if (file.Exists())
                {
                    var content = new byte[file.Properties.Length];
                    await file.DownloadToByteArrayAsync(content, 0);
                    return content;
                }
            }

            throw new Exception(String.Format("Unable to find file '{0}' on share '{1}'", fileName, shareName));
        }
    }
}
