using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TVH.ClaimCheck
{
    public class BlobStorageClaimCheckProvider : IClaimCheckProvider
    {
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        public BlobStorageClaimCheckProvider(string connectionString, string containerName)
        {
            _blobClient = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(containerName);
        }

        public async Task<string> StoreMessage(byte[] messageContent)
        {
            await _blobContainer.CreateIfNotExistsAsync();

            string messageReference = Guid.NewGuid().ToString();

            var blob = _blobContainer.GetBlockBlobReference(messageReference);
            await blob.UploadFromStreamAsync(new MemoryStream(messageContent));

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(10),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            return blob.Uri + sasBlobToken;
        }

        public async Task<byte[]> RetrieveMessage(string messageReference)
        {
            var blob = new CloudBlockBlob(new Uri(messageReference));
            blob.FetchAttributes();

            var blobContent = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(blobContent, 0);

            return blobContent;
        }
    }
}
