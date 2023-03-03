using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SturfeeVPS.Core;
using Newtonsoft.Json;

namespace SturfeeVPS.SDK
{
    public class DTHDConstants_SDK
    {
        public static readonly string DTHD_API = "https://digitaltwin.sturfee.com/hd/layout";
        public static readonly string TestID = "3745b04f-7465-4533-b84f-406690685845";
    }

    [Serializable]
    public class DtHdAssetItem_SDK
    {
        public string DtHdAssetId;
        public string DtHdAssetItemId;
        public string Name;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;
        public GeoLocation Location;
        public float LocalX;
        public float LocalY;
        public float LocalZ;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;
        public float Scale;
    }

    [Serializable]
    public enum AssetType_SDK
    {
        Prop,
        ProductGroup,
        EditableSurface
    }

    [Serializable]
    public class DtHdAsset_SDK
    {
        public string DtHdAssetId;
        public string Name;
        public string Description;
        public List<DtHdAssetItem_SDK> Items;
        public string FileUrl;
        public int FileSizeBytes;
        public string Format;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;
        public AssetType_SDK AssetType;
        public string ExternalRefId;
        public string EditMode;
        public string EditRole;
        public string PhysicsMode;
    }

    [Serializable]
    public class DtHdLayout_SDK
    {
        public string DtHdId;
        public string UserId;
        public string Name;
        public GeoLocation Location;
        public double RefX;
        public double RefY;
        public double RefZ;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;
        public int FileSizeBytes;
        public float SpawnPositionX;
        public float SpawnPositionY;
        public float SpawnPositionZ;
        public float SpawnHeading;
        public bool IsIndoor;
        public bool IsPublic;
        public List<ScanMesh_SDK> ScanMeshes;
        public string EnhancedMesh;
        public List<DtHdAsset_SDK> Assets;
        public string DtEnvironmentUrl;
    }

    [Serializable]
    public class ScanMesh_SDK
    {
        public string DtHdScanId;
        public string Status;
        public string SiteName;
        public string Thumbnail;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;
        public GeoLocation ScanLocation;
        public double RefX;
        public double RefY;
        public double RefZ;
        public int Floor;
        public string ScanMeshUrl;
        public VpsHdSite_SDK VpsHdSite;

    }

    // [Serializable]
    // public class VpsHdSite
    // {
    //     [JsonProperty("site_id")]
    //     public string SiteId;
    //     public string Name;
    //     [JsonProperty("dthd_id")]
    //     public string DtHdId;
    //     [JsonProperty("dtscan_id")]
    //     public string DtScanId;

    // }

    [Serializable]
    public class VpsHdSite_SDK
    {
        public string thumbnailUrl;
        public SiteInfo_SDK siteInfo;
        public string anchorMesh;
    }

    [Serializable]
    public class SiteInfo_SDK
    {
        public string site_id;
        public string name;
        public string dthd_id;
        public string dtscan_id;
        public string thumbnail_id;
        public DateTime createdDate;
        public DateTime updatedDate;
        public int floor;
        public bool isIndoor;
        public double refX;
        public double refY;
        public double refZ;
        public string source;
        public string platform;
        public string s3_key;
        public double longitude;
        public double latitude;
        public int utm_lon_zone;
        public string utm_lat_zone;
        public float radius;
        public bool active;
        public float terrainAdjustment;
        public float projectionErrorThreshold;
    }

    // data for DtHd Environment.json
    [Serializable]
    public class DtEnvironment_SDK
    {
        public UnityEnvironment_SDK Unity;
    }

    [Serializable]
    public class UnityEnvironment_SDK
    {
        public UnityReflectionProbe_SDK[] ReflectionProbes;
        public UnityLight_SDK[] Lights;
    }

    public enum ReflectionProbeType_SDK
    {
        Baked,
        Custom,
        Realtime
    }

    [Serializable]
    public class UnityReflectionProbe_SDK
    {
        public Guid ReflectionProbeId;
        public Guid? DtHdId;
        public Guid UserId;

        public string Name;
        public int Importance;
        public float Intensity;
        public bool BoxProjection;
        public float BoxSize;
        public ReflectionProbeType_SDK Type;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;

        public double Lat;
        public double Lon;
        public double Alt;

        public float LocalX;
        public float LocalY;
        public float LocalZ;
    }

    public enum LightType_SDK
    {
        Point,
        Spot,
        Directional
    }

    public enum ShadowType_SDK
    {
        NoShadows,
        HardSadows,
        SoftShadows
    }

    public enum LightMode_SDK
    {
        RealTime,
        Mixed,
        Baked
    }

    [Serializable]
    public class UnityLight_SDK
    {
        public Guid LightId;
        public Guid? DtHdId;
        public Guid UserId;

        public string Name;
        public LightType_SDK LightType;

        public bool IsMainLight;

        public float Range;
        public float SpotAngle;

        public float ColorR;
        public float ColorG;
        public float ColorB;

        public float Intensity;
        public float ShadowStrength;
        public ShadowType_SDK ShadowType;
        public LightMode_SDK LightMode;

        public DateTime CreatedDate;
        public DateTime UpdatedDate;

        public double Lat;
        public double Lon;
        public double Alt;

        public float LocalX;
        public float LocalY;
        public float LocalZ;

        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;
    }
}
