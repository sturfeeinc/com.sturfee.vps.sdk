using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.SDK
{

    [Serializable]
    public class SiteResponse
    {
        public HDSite[] Items;
        public string Version;
    }
    /// <summary>
    /// Data structure for an HD site meta-data.  
    /// </summary>/
    [Serializable]
    public class HDSite
    {
        public string siteName;
        public string siteId;
        public double latitude;
        public double longitude;
        public string ImageUrl;
        public SitePointCloud mesh;
        public SitePointCloud scanTerrainMesh;
        public SitePointCloud pointCloud;
        public SitePointCloud arFoundationPointCloud;
        public SturfeeCMSAnchor[] anchors;
        public SiteMetadata site_meta_data;

        public HDSite()
        {
            anchors = new SturfeeCMSAnchor[0];
        }
    }


    [Serializable]
    public class SturfeeCMSAnchor
    {
        public int anchorId;
        public string anchorName;
        public int assetId;
        public double latitude;
        public double longitude;
        public double altitude;
        public XrQuaternion rotation;
        public bool useAltitude;
        public bool useRotation;
        public float heightAboveTerrain;
        public float heading;
        public float scale;
        public string dateOfAddition;
        public string videoURL;

        public CmsAnchorMetadata metadata;
    }

    [Serializable]
    public class SitePointCloud
    {
        /// <summary>
        /// point cloud (ply) file currently stores URL to anchormesh (glb) instead. To be revised soon.
        /// </summary>
        public string ply;
        public string csv;
        public CenterRef centerRef;
        public float heightOffset;
    }

    [Serializable]
    public class CenterRef
    {
        public double x;
        public double y;
    }


    // metadata for anchors
    public enum AnchorType
    {
        None,
        XrAsset,
        StickyNote
    }

    [Serializable]
    public class CmsAnchorMetadata
    {
        public AnchorType Type;
        public string Data; // json storage of data
    }

    [Serializable]
    public class XrQuaternion
    {
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class SiteMetadata
    {
        public string UserId;
        public string RefId;
        public string ThumbId;
        public string CreatedDate;
        public string SpaceType; // Indoor/Outdoor

        [JsonIgnore]
        public DateTime CreatedDateTime
        {
            get
            {
                if (!string.IsNullOrEmpty(CreatedDate))
                {
                    return DateTime.Parse(CreatedDate);
                }
                else
                {
                    return DateTime.Now.AddDays(-100);
                }
            }
        }
    }

}
