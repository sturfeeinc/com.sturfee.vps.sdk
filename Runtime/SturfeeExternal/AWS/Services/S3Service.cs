using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Sturfee.External.AWS
{
    public class S3Service
    {
        private IAmazonS3 _client;

        public S3Service(AWSCredentials awsCredentials, RegionEndpoint regionEndpoint)
        {
            _client = new AmazonS3Client(awsCredentials, regionEndpoint);
        }

        /// <summary>
        /// Gets object from S3 by providing a bucket and Key
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<byte[]> GetObjectAsync(string bucket, string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = key,
            };

            try
            {
                using var response = await _client.GetObjectAsync(request);
                using MemoryStream stream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(stream);
                return stream.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError("Exception during GetObject : " + e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Gets object from S3 by providing a Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<byte[]> GetObjectAsync(string url)
        {
            AmazonS3Uri s3URI = new AmazonS3Uri(new Uri(url));
            return await GetObjectAsync(s3URI.Bucket, s3URI.Key);
        }

        /// <summary>
        /// Posts an object into the provided S3 bucket
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="filepath">local filepath to the file that needs to be uploaded to S3</param>
        /// <returns></returns>
        public async Task PostObjectAsync(string bucket, string key, string filepath)
        {
            var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var request = new PutObjectRequest()
            {
                BucketName = bucket,
                Key = key,
                InputStream = stream,
                CannedACL = S3CannedACL.BucketOwnerFullControl
            };

            try
            {
                var response = await _client.PutObjectAsync(request);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception during PostObject : " + e.Message);
                throw e;
            }
        }

#if UNITY_ANDROID
    public void UsedOnlyForAOTCodeGeneration()
    {
        //Bug reported on github https://github.com/aws/aws-sdk-net/issues/477
        //IL2CPP restrictions: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
        //Inspired workaround: https://docs.unity3d.com/ScriptReference/AndroidJavaObject.Get.html

        AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
        int valueString = jo.Get<int>("what");
    }
#endif

    }
}