﻿using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SturfeeVPS.SDK.Editor
{
    public class SturfeeObjectsCreator : MonoBehaviour
    {
        [MenuItem("GameObject/SturfeeXR/SturfeeXRSession")]
        public static void CreateSturfeeXrSession()
        {
            var prefab = Resources.Load<SturfeeXrSession>($"Prefabs/{SturfeeObjects.SturfeeXrSession}");
            
            if(prefab == null)
            {
                SturfeeDebug.LogError($"Cannot instantiate {SturfeeObjects.SturfeeXrSession}. Prefab not found");
                return;
            }

            var go = Instantiate(prefab);
            go.name = SturfeeObjects.SturfeeXrSession;
        }

        [MenuItem("GameObject/SturfeeXR/SturfeeXRSession-AR")]
        public static void CreateSturfeeXrSessionAR()
        {
            var prefab = Resources.Load<SturfeeXrSession>($"Prefabs/{SturfeeObjects.SturfeeXrSessionAR}");
            if (prefab == null)
            {
                SturfeeDebug.LogError($"Cannot instantiate {SturfeeObjects.SturfeeXrSessionAR}. Prefab not found");
                return;
            }


            var go = Instantiate(prefab);
            go.name = SturfeeObjects.SturfeeXrSessionAR;
        }

        [MenuItem("GameObject/SturfeeXR/XrCamera")]
        public static void CreateXrCamera()
        {
            var prefab = Resources.Load<XrCamera>($"Prefabs/{SturfeeObjects.XrCamera}");
            if (prefab == null)
            {
                SturfeeDebug.LogError($"Cannot instantiate {SturfeeObjects.XrCamera}. Prefab not found");
                return;
            }

            var go = Instantiate(prefab);
            go.name = SturfeeObjects.XrCamera;

        }

        [MenuItem("GameObject/SturfeeXR/SturfeeUI")]
        public static void CreateSturfeeUI()
        {
            var prefab = Resources.Load<SturfeeUIManager>($"Prefabs/{SturfeeObjects.SturfeeUI}");
            if (prefab == null)
            {
                SturfeeDebug.LogError($"Cannot instantiate {SturfeeObjects.SturfeeUI}. Prefab not found");
                return;
            }

            var go = Instantiate(prefab);
            go.name = SturfeeObjects.SturfeeUI;
        }

        [MenuItem("GameObject/SturfeeXR/XrLight")]
        public static void CreateXrLight()
        {
            var prefab = Resources.Load<Light>($"Prefabs/{SturfeeObjects.XrLight}");
            if (prefab == null)
            {
                SturfeeDebug.LogError($"Cannot instantiate {SturfeeObjects.XrLight}. Prefab not found");
                return;
            }

            var go = Instantiate(prefab);
            go.name = SturfeeObjects.XrCamera;

        }

    }
}