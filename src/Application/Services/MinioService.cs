using Microsoft.AspNetCore.Http;
using Minio.DataModel.Args;
using Minio.DataModel;
using Microsoft.Extensions.Options;
using Domain.AppSettings;
using Minio;

namespace Application.Services
{
    public class MinioService
    {

        private readonly MinioSettings _settings;
        private readonly IMinioClient _minioClient;
        private readonly PhotoResizerService _resizer;

        public MinioService(IOptions<MinioSettings> opts,PhotoResizerService resizerService)
        {
            _resizer = resizerService;
            _settings = opts.Value;
            _minioClient= new MinioClient().WithEndpoint(_settings.MainEndpoint)
                .WithCredentials(_settings.AccessKey,_settings.SecretKey).WithSSL(false).Build();
        }

        public async Task<(string, Guid)> SaveFileToMinio(IFormFile file)
        {
            async Task<bool> CheckBucket()
            {
                var found = await _minioClient.BucketExistsAsync(
                    new Minio.DataModel.Args.BucketExistsArgs().WithBucket(_settings.Bucket));
                if (!found)
                {

                    return false;
                }
                return true;
            }
            async Task<bool> CheckFolder()
            {
                string folderkey = _settings.MainFolder.TrimEnd('/') + "/.folder";
                try
                {
                    await _minioClient.StatObjectAsync(
                        new StatObjectArgs()
                            .WithBucket(_settings.Bucket)
                            .WithObject(folderkey));
                    return true;
                }
                catch (Minio.Exceptions.ObjectNotFoundException)
                {
                    return false;
                }

            }
            async Task CreateFolder()
            {
                string folderKey = _settings.MainFolder.TrimEnd('/') + "/.folder";
                using var dummyStream = new MemoryStream(new byte[0]);
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_settings.Bucket)
                    .WithObject(folderKey)
                    .WithStreamData(dummyStream)
                    .WithObjectSize(0)
                    .WithContentType("application/x-empty"));
            }

            bool bucketExixts = await CheckBucket();
            if (!bucketExixts)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.Bucket));
            }
            bool folderExists = await CheckFolder();
            if (!folderExists)
            {
                await CreateFolder();
            }

            Guid photoId = Guid.NewGuid();

            string objectname = $"{_settings.MainFolder}/{photoId.ToString()}.jpg";

            using var originalStream = new MemoryStream();

            await file.CopyToAsync(originalStream);
            originalStream.Position = 0;

            using var resized = await _resizer.ResizeImage(originalStream);
            resized.Position = 0;

            await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithObject(objectname)
            .WithStreamData(resized)
            .WithObjectSize(resized.Length)
            .WithContentType("image/jpeg")
            );

            string fullpath = $"{_settings.MainEndpoint}/{_settings.Bucket}/{objectname}";

            return (fullpath, photoId);
        }

        public async Task<string> GenerateUrl(string id)
        {
            string presignedUrl = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_settings.Bucket)
                    .WithObject($"{_settings.MainFolder}/{id}.jpg")
                    .WithExpiry(60 * 60)
                );

            return presignedUrl;
        }

        public async Task<bool> DeleteFilesFromMinio(List<Guid> idsToDelete)
        {

            try
            {
                if (idsToDelete == null || idsToDelete.Count == 0)
                    return true;

                foreach (var id in idsToDelete)
                {
                    string fileName = $"{id}.jpg";
                    await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                        .WithBucket(_settings.Bucket)
                        .WithObject(fileName));
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
