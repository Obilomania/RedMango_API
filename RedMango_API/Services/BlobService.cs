using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RedMango_API.Services.IServices;

namespace RedMango_API.Services
{
    public class BlobService : IBlobservice
    {
        private readonly BlobServiceClient _blobClient;

        public BlobService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task<bool> DeleteBlob(string blobName, string containerName)
        {
            //Get The particular Azure Container Client
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            //Serch for a blob in thatt container
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            return await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> GetBlob(string blobName, string containerName)
        {
            //Get The particular Azure Container Client
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            //Serch for a blob in thatt container
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);


            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<string> UploadBlob(string blobName, string containerName, IFormFile file)
        {
            //Get The particular Azure Container Client
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            //Serch for a blob in thatt container
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            //Type of Data to upload
            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType
            };
            //upload
            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
            //validation
            if (result != null)
            {
                return await GetBlob(blobName, containerName);
            }
            return "";

        }
    }
}
