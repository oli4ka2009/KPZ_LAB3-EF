using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LAB3.Helpers
{
    public class BlobService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobService(string connectionString, string containerName)
        {
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task UploadFileAsync(string blobName, Stream stream)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public string GetFileSasUri(string blobName, DateTimeOffset expiryTime)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerClient.Name,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = expiryTime
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }
            else
            {
                throw new InvalidOperationException("Не вдалося згенерувати SAS URL для цього блоба.");
            }
        }
    }
}
