using Owlcat.Runtime.Core.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OwlcatModification.Modifications.SEdit
{

    public class BundleLoader : MonoBehaviour
    {
        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.BundleLoader");


        public static BundleLoader instance;

        public static Dictionary<string, GameBundle> bundles = new Dictionary<string, GameBundle>();
        public static Dictionary<string, string> discBundles = new Dictionary<string, string>();


        //For UI 
        public static Dictionary<string, bool> expandedloadedBundles = new Dictionary<string, bool>();
        public static Dictionary<string, bool> expandedloadedDiscBundles = new Dictionary<string, bool>();

        /// <summary>
        /// Stores all relevant data from a loaded assetbundle, for later access
        /// </summary>
        public class GameBundle
        {
            public AssetBundle bundle { get; set; }
            public Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
            public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            public Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
            public Dictionary<string, Texture2D> textures2D = new Dictionary<string, Texture2D>();

            private bool hasLoadedBundle = false;
            /// <summary>
            /// Fills the dictionary with the usable objects
            /// </summary>
            public void LoadBundle()
            {
                if (hasLoadedBundle) return;
                try
                {
                    int count = 0;
                    foreach (string path in bundle.GetAllAssetNames())
                    {
                        UnityEngine.Object foundObject = bundle.LoadAsset<UnityEngine.Object>(path);
                        count++;
                        if (foundObject is GameObject)
                        {
                            if (!objects.ContainsKey(foundObject.name))
                            {
                                objects.Add(foundObject.name, foundObject as GameObject);
                            }
                        }
                        else if (foundObject is Sprite)
                        {
                            if (!sprites.ContainsKey(foundObject.name))
                            {
                                Channel.Log("Found Sprite");
                                sprites.Add(foundObject.name, foundObject as Sprite);
                            }
                        }
                        else if (foundObject is Mesh)
                        {
                            if (!meshes.ContainsKey(foundObject.name))
                            {
                                Channel.Log("Found Mesh");
                                meshes.Add(foundObject.name, foundObject as Mesh);
                            }
                        }
                        else if (foundObject is Texture2D)
                        {
                            if (!textures2D.ContainsKey(foundObject.name))
                            {
                                Channel.Log("Found Texture");
                                textures2D.Add(foundObject.name, foundObject as Texture2D);
                            }
                        }

                        Channel.Log($"GameObject name: {foundObject.name} path: {path}");
                    }
                    Channel.Log($"Got{count} Objects");
                }
                catch (Exception e)
                {
                    Channel.Log(e.Message);
                }
                hasLoadedBundle = true;
            }

        }




        private void Start()
        {
            GetLoadedBundles();
            GetBundlesFromDisc();
            BundleLoader.instance = this;

        }

        /// <summary>
        /// Checks if a bundle with the name exists
        /// </summary>
        /// <param name="bundlename"> The name of the bundle</param>
        /// <param name="tryAutoload">If the bundle exists, but isnt loaded the bundle is loaded into the game, if the argument is "true"</param>
        /// <returns></returns>
        public static bool IsBundleLoaded(string bundlename, bool tryAutoload = false)
        {
            if (BundleLoader.bundles.ContainsKey(bundlename))
            {
                BundleLoader.bundles[bundlename].LoadBundle();
                return true;
            }
            else
            {

                if (tryAutoload && BundleLoader.discBundles.ContainsKey(bundlename.Replace(" (UnityEngine.AssetBundle)", "")))
                {
                    LoadBundleFromDisc(BundleLoader.discBundles[bundlename.Replace(" (UnityEngine.AssetBundle)", "")]);
                    BundleLoader.bundles[bundlename].LoadBundle();
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Tries to load the bundle from the disc, with a given path
        /// </summary>
        /// <param name="path">Path to the bundle on the disc</param>
        public static void LoadBundleFromDisc(string path)
        {
            AssetBundle bundle = null;
            try
            {
                bundle = AssetBundle.LoadFromFile(path);
                if (bundle)
                {

                    if (bundle.isStreamedSceneAssetBundle)
                    {
                        Channel.Log("Found streamed scene bundle");
                    }
                    else
                    {
                        Channel.Log($"Loaded bundlename={bundle} from disk");
                        GameBundle newBundle = new GameBundle();
                        newBundle.bundle = bundle;

                        if (!bundles.ContainsKey(($"{bundle}")))
                        {
                            bundles.Add($"{bundle}", newBundle);
                            expandedloadedBundles.Add($"{bundle}", false);
                            if (expandedloadedDiscBundles.ContainsKey($"{bundle}"))
                            {
                                expandedloadedDiscBundles.Remove($"{bundle}");
                                discBundles.Remove($"{bundle}");
                            }

                        }
                    }
                }
                else
                {
                    Channel.Log("Loading failed! Bundle == null");
                }
            }
            catch (Exception e)
            {
                Channel.Log(e.Message);
            }
        }

        /// <summary>
        /// Creates a dictionary, filled with all found bundles under .../Wrath_Data/Bundles/
        /// </summary>
        private void GetBundlesFromDisc()
        {
            string path = Application.dataPath.Replace("Wrath_Data", "") + "Bundles/";
            Channel.Log("Trying to use bundlepath: " + path);

            try
            {
                string[] fileEntries = Directory.GetFiles(path);

                foreach (string pathEntries in fileEntries)
                {
                    string tmp = pathEntries.Replace(path, "");
                    discBundles.Add(tmp, pathEntries);

                    //Channel.Log(($"Found DiskBundle {pathEntries}"));
                    expandedloadedDiscBundles.Add(tmp, false);


                }
                Channel.Log($" Found  {fileEntries.Length} bundles on disk");
            }
            catch (Exception e)
            {
                Channel.Log(e.Message);
            }
        }
        /// <summary>
        /// Creates a dictionary, filled with all currently loaded bundles 
        /// </summary>
        private void GetLoadedBundles()
        {
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();

            int count = 0;
            foreach (var bundle in loadedBundles)
            {
                Channel.Log($"Loaded bundlename={bundle}");
                GameBundle tmp = new GameBundle();
                tmp.bundle = bundle;
                expandedloadedBundles.Add($"{bundle}", false);
                bundles.Add($"{bundle}", tmp);
                count++;
            }
            Channel.Log("Datapath:" + Application.dataPath + " Got " + count + " Bundles");
        }


    }
}