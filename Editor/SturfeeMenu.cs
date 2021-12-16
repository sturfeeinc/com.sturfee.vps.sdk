using System.IO;
using UnityEngine;
using UnityEditor;

namespace SturfeeVPS.SDK
{

    public class SturfeeMenu : MonoBehaviour
    {
        [MenuItem("Sturfee/Clear TileCache")]
        public static void ClearTileCache()
        {
            string dir = Path.Combine(Application.persistentDataPath, "TileCache");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);

                Debug.Log(" Tile Cache cleared");
            }
        }
    }
}
