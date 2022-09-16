using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SturfeeVPS.SDK
{
    [Serializable]
    public class HDSiteFilter
    {
        public string AppId;
        public SortOptions SortOptions = SortOptions.Location;
        public GeoLocation Location;
    }

    public enum SortOptions
    {
        Location,
        Name,
        CreatedDate,
        MostRecent
    }
}
