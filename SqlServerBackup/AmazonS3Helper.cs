using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace SqlServerBackup
{
    public class AmazonS3Helper
    {
        /// <summary>
        /// Occurs when a synchronization request is made, before any data is synchronized.
        /// </summary>
        public event EventHandler UploadComplete;
        protected void OnUploadComplete(IAsyncResult ar)
        {
            EventHandler handler = UploadComplete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public String BucketName
        {
            get { return _bucket; }
        }

        private IAmazonS3 _client;

        private String _keyPublic;
        private String _keySecret;
        private String _bucket;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonS3Helper" /> class using the specified credentials.
        /// </summary>
        /// <param name="keyPublic">The public Amazon S3 key.</param>
        /// <param name="keySecret">The secret Amazon S3 key.</param>
        public AmazonS3Helper(String keyPublic, String keySecret, String bucket)
        {
            _keyPublic = keyPublic;
            _keySecret = keySecret;
            _bucket = bucket;
            ValidateConfiguration();

            var s3Config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.USEast1 };
            _client = AWSClientFactory.CreateAmazonS3Client(keyPublic, _keySecret, s3Config);
        }

        private void ValidateConfiguration()
        {
            // Are either of the Amazon S3 keys missing?
            Boolean keyMissing = (String.IsNullOrWhiteSpace(_keyPublic) || String.IsNullOrWhiteSpace(_keySecret));
            if (keyMissing)
            {
                throw new Exception("Amazon S3 credentials not found!");
            }
        }

        public IEnumerable<S3Object> GetFiles(String prefix = null)
        {
            var objects = new List<S3Object>();
            String lastKey = "";
            String preLastKey = "";
            do
            {
                preLastKey = lastKey;
                ListObjectsRequest request;

                if (String.IsNullOrWhiteSpace(prefix))
                {
                    request = new ListObjectsRequest();
                    request.BucketName = _bucket;
                }
                else
                {
                    request = new ListObjectsRequest();
                    request.BucketName = _bucket;
                    request.Prefix = prefix;
                }

                request.Marker = lastKey;
                ListObjectsResponse response = _client.ListObjects(request);
                var objectListing = response.S3Objects;
                foreach (S3Object obj in objectListing)
                {
                    // Exclude directories.
                    Boolean isDirectory = obj.Key.EndsWith("/");
                    if (!isDirectory)
                    {
                        objects.Add(obj);
                        lastKey = obj.Key;
                    }
                }
            } while (lastKey != preLastKey);
            return objects;
        }

        public static String GetPublicUrl(String bucketName, String filename)
        {
            const String template = "https://s3.amazonaws.com/{0}/{1}";
            return String.Format(template, bucketName, filename);
        }

        public String UploadFile(String amazonPath, String localPath)
        {
            var transferUtility = new TransferUtility(_keyPublic, _keySecret);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                FilePath = localPath,
                BucketName = _bucket,
                Key = amazonPath,
                CannedACL = new S3CannedACL("public-read"),
            };

            var upload = transferUtility.UploadAsync(uploadRequest);
            upload.ContinueWith(OnUploadComplete);
            upload.Wait();
            return GetPublicUrl(_bucket, amazonPath);
        }

        public void DeleteFile(String filename)
        {
            String key = filename;
            var amazonClient = new AmazonS3Client(_keyPublic, _keySecret);
            var deleteObjectRequest = new DeleteObjectRequest { BucketName = _bucket, Key = key };
            var response = amazonClient.DeleteObject(deleteObjectRequest);
        }

    }
}
