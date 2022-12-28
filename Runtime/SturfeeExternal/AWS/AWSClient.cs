using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sturfee.External.AWS
{    
    public class AWSClient : MonoBehaviour
    {
        public string AccessKey;
        public string Secret;
        public string IdentityPoolId;
        public string Region;

        private AWSCredentials _credentials;

        public AWSCredentials Credentials
        {
            get
            {
                if (_credentials == null)
                {
                    if (string.IsNullOrEmpty(AccessKey))
                    {

                        if (string.IsNullOrEmpty(IdentityPoolId))
                        {
                            throw new Exception("No IdentityPoolId or AccessKey available");
                        }
                        _credentials = new CognitoAWSCredentials(IdentityPoolId, RegionEndpoint.GetBySystemName(Region));
                    }
                    else
                    {
                        _credentials = new BasicAWSCredentials(AccessKey, Secret);
                    }
                }
                    
                return _credentials;
            }
        }
    }
}
