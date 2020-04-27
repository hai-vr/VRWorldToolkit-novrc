﻿using System;
using System.Linq;
using UnityEditor;
using VRC.SDKBase;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Reflection;
using UnityEngine.Assertions;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
using System.IO;
#endif

namespace VRCWorldToolkit
{
    public class Helper
    {
        public static string ReturnPlural(int counter)
        {
            return counter > 1 ? "s" : "";
        }

        public static bool CheckNameSpace(string namespace_name)
        {
            bool namespaceFound = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                   from type in assembly.GetTypes()
                                   where type.Namespace == namespace_name
                                   select type).Any();
            return namespaceFound;
        }

        public static float GetBrightness(Color color)
        {
            float num = ((float)color.r);
            float num2 = ((float)color.g);
            float num3 = ((float)color.b);
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
                num4 = num2;
            if (num3 > num4)
                num4 = num3;
            if (num2 < num5)
                num5 = num2;
            if (num3 < num5)
                num5 = num3;
            return (num4 + num5) / 2;
        }

        public static string Truncate(string text, int length)
        {
            if (text.Length > length)
            {
                text = text.Substring(0, length);
                text += "...";
            }
            return text;
        }
    }

    public class WTPostProcessing
    {

#if UNITY_POST_PROCESSING_STACK_V2
        public static void Setup(VRC_SceneDescriptor descriptor)
        {
            if (EditorUtility.DisplayDialog("Setup Post Processing?", "This will setup your scenes Reference Camera and make a new global volume using the included example Post Processing Profile", "OK", "Cancel"))
            {
                if (Camera.main == null)
                {
                    GameObject camera = new GameObject("Main Camera");
                    camera.AddComponent<Camera>();
                    camera.AddComponent<AudioListener>();
                    camera.tag = "MainCamera";
                }
                descriptor.ReferenceCamera = Camera.main.gameObject;
                if (!Camera.main.gameObject.GetComponent<PostProcessLayer>())
                    Camera.main.gameObject.AddComponent(typeof(PostProcessLayer));
                PostProcessLayer postprocess_layer = Camera.main.gameObject.GetComponent(typeof(PostProcessLayer)) as PostProcessLayer;
                postprocess_layer.volumeLayer = LayerMask.GetMask("Water");
                PostProcessVolume volume = GameObject.Instantiate(PostProcessManager.instance.QuickVolume(16, 100f));
                if (!Directory.Exists("Assets/Post Processing"))
                    AssetDatabase.CreateFolder("Assets", "Post Processing");
                if (AssetDatabase.LoadAssetAtPath("Assets/Post Processing/SilentProfile.asset", typeof(PostProcessProfile)) == null)
                {
                    AssetDatabase.CopyAsset("Assets/VRWorldToolkit/Resources/PostProcessing/SilentProfile.asset", "Assets/Post Processing/SilentProfile.asset");
                }
                volume.sharedProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath("Assets/Post Processing/SilentProfile.asset", typeof(PostProcessProfile));
                volume.gameObject.name = "Post Processing Volume";
                volume.gameObject.layer = LayerMask.NameToLayer("Water");
            }
        }
#endif

        static AddRequest Request;

        public static void Install()
        {
            Request = Client.Add("com.unity.postprocessing");
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }
        }
    }
}