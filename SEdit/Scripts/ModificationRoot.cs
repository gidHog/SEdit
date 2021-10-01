using System.Reflection;
using System.Linq;
using System;
using HarmonyLib;
using Kingmaker.Modding;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using RuntimeGizmos;



namespace OwlcatModification.Modifications.SEdit
{
    // ReSharper disable once UnusedType.Global
    public static class ModificationRoot
    {


        private static Kingmaker.Modding.OwlcatModification Modification { get; set; }

        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit");
        public static Camera m_MainCamera { get; set; }
        [Serializable]
        public class ModificationData
        {
            public string GUID;
            public string usedSEditVersion;
            public bool AutoLoad;
        }

        public static ModificationData userData;

        public static string SEDITVERSION { get; set; } = "0.01";// current SEdit-version
        private static bool inEdit { get; set; } = false; // if the editor is enabled
        private static bool showSceneElements { get; set; } = false; // if scene elements are shown
        private static bool loadBundles { get; set; } = false;
        private static bool loadBundlesFromDisc { get; set; } = false;
        public static int selectedScene { get; private set; } = 0;// currently selected subscene

        private static Vector3 partialShowAmount; // amount of shown bundles per page and their index x->y z = amount

        private static bool hasLoaded { get; set; } = false;

        public static GameObject m_MainObject { get; set; }

        public static string modPath { get; set; }
        // ReSharper disable once UnusedMember.Global
        [OwlcatModificationEnterPoint]
        public static void Initialize(Kingmaker.Modding.OwlcatModification modification)
        {
            Modification = modification;
            modPath = Modification.Path;
            Channel.Log($"Modpath: {modPath}");
            var harmony = new Harmony(modification.Manifest.UniqueName);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modification.OnGUI += OnGUI;
            GetUserData();
            Autoload();
        }


        private static void Autoload()
        {
            Init();
            //SceneEditor.instance.Init();
        }


        /// <summary>
        /// Loads the saved userdata
        /// </summary>

        private static void GetUserData()
        {
            userData = Modification.LoadData<ModificationData>();

            if (userData.GUID == "" || userData.GUID == null)
            {
                userData.GUID = System.Guid.NewGuid().ToString();
            }
            userData.usedSEditVersion = SEDITVERSION;

            Modification.SaveData(userData);
        }


        /// <summary>
        /// Displays a given scene
        /// </summary>
        /// <param name="wrpScene"> scene to display</param>
        private static void ShowSceneData(SceneSearcher.SceneWrapper wrpScene)
        {
            GUILayout.Label(wrpScene.scene.name, GUILayout.ExpandWidth(false));
            if (wrpScene.scene != null)
            {

                foreach (SceneSearcher.Node node in wrpScene.nodes)
                {
                    ShowData(node, 0);
                }
            }
            else
            {
                GUILayout.Label("Scene is null!", GUILayout.ExpandWidth(false));
            }
        }

        /// <summary>
        /// Displays a given node
        /// </summary>
        /// <param name="currentNode">node to display</param>
        /// <param name="depth">current draw offset / recursion depth</param>
        private static void ShowData(SceneSearcher.Node currentNode = null, int depth = 0)
        {

            if (currentNode != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space((Utils.buttonWidth * depth));
                if (currentNode.rootGameobjects.Count > 0)
                {
                    if (currentNode.isExpanded)
                    {
                        if (GUILayout.Button("-", GUILayout.Width(Utils.buttonWidth)))
                        {
                            currentNode.isExpanded = !currentNode.isExpanded;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(Utils.buttonWidth)))
                        {
                            currentNode.isExpanded = !currentNode.isExpanded;
                        }
                    }



                }
                else
                {
                    GUILayout.Button("=", GUILayout.Width(Utils.buttonWidth));

                }
                if (GUILayout.Button(currentNode.obj.gameobject.name, GUILayout.ExpandWidth(false)))
                {
                    SelectedGameObject(currentNode.obj.gameobject);
                }
                GUILayout.EndHorizontal();

                if (currentNode.isExpanded)
                {
                    depth++;
                    foreach (SceneSearcher.Node node in currentNode.rootGameobjects.Values)
                    {

                        GUILayout.BeginHorizontal();
                        GUILayout.Space((Utils.buttonWidth * depth));
                        if (node.rootGameobjects.Count > 0)
                        {
                            if (node.isExpanded)
                            {
                                if (GUILayout.Button("-", GUILayout.Width(Utils.buttonWidth)))
                                {
                                    node.isExpanded = !node.isExpanded;
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("+", GUILayout.Width(Utils.buttonWidth)))
                                {
                                    node.isExpanded = !node.isExpanded;
                                }
                            }

                        }
                        else
                        {
                            GUILayout.Button("=", GUILayout.Width(Utils.buttonWidth));

                        }

                        if (GUILayout.Button(node.obj.gameobject.name, GUILayout.ExpandWidth(false)))
                        {
                            SelectedGameObject(node.obj.gameobject);
                        }
                        if (node.rootGameobjects.Count > 0 && node.isExpanded)
                        {

                            GUILayout.EndHorizontal();

                            foreach (OwlcatModification.Modifications.SEdit.SceneSearcher.Node obj in node.rootGameobjects.Values)
                            {

                                ShowData(obj, depth + 1);

                            }
                        }
                        else
                        {
                            GUILayout.EndHorizontal();
                        }

                    }

                }

            }


        }





        /// <summary>
        /// Displays the content of a given assetbundle
        /// </summary>
        /// <param name="key">Name of the bundle / key for the dictionary</param>

        private static void ShowBundle(string key)
        {
            GUILayout.BeginHorizontal();
            if (!BundleLoader.expandedloadedBundles[key])
            {
                if (GUILayout.Button("+", GUILayout.Width(Utils.buttonWidth)))
                {
                    BundleLoader.expandedloadedBundles[key] = !BundleLoader.expandedloadedBundles[key];
                    GUILayout.Button(key, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                if (GUILayout.Button("-", GUILayout.Width(Utils.buttonWidth)))
                {
                    BundleLoader.expandedloadedBundles[key] = !BundleLoader.expandedloadedBundles[key];
                    GUILayout.Button(key, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                }

            }

            if (BundleLoader.expandedloadedBundles[key])
            {
                //show assets

                GUILayout.Button(key);
                GUILayout.EndHorizontal();
                BundleLoader.bundles[key].LoadBundle();

                foreach (string assetName in BundleLoader.bundles[key].objects.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(Utils.buttonWidth * 2);
                    if (GUILayout.Button(assetName + (" (GameObject)"), GUILayout.ExpandWidth(false)))
                    {

                        SceneEditor.instance.AddObjectToScene(BundleLoader.bundles[key].objects[assetName], assetName, BundleLoader.bundles[key].bundle.name, null, true, SceneEditor.currentEditableScene);


                    }
                    GUILayout.EndHorizontal();
                }
                foreach (string assetName in BundleLoader.bundles[key].sprites.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(Utils.buttonWidth * 2);
                    if (GUILayout.Button(assetName + (" (Sprite)"), GUILayout.ExpandWidth(false)))
                    {
                        //todo maybe drag and drop?
                    }
                    GUILayout.EndHorizontal();
                }
                foreach (string assetName in BundleLoader.bundles[key].meshes.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(Utils.buttonWidth * 2);
                    if (GUILayout.Button(assetName + (" (Mesh)"), GUILayout.ExpandWidth(false)))
                    {
                        //todo spawn create gameobject with mat etc.
                    }
                    GUILayout.EndHorizontal();
                }

                foreach (string assetName in BundleLoader.bundles[key].textures2D.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(Utils.buttonWidth * 2);
                    if (GUILayout.Button(assetName + (" (Texture2D)"), GUILayout.ExpandWidth(false)))
                    {
                        //todo
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Button(key);
                GUILayout.EndHorizontal();
            }
        }


        /// <summary>
        /// Iterates over all loaded bundles
        /// </summary>
        private static void ShowBundles()
        {


            foreach (string key in BundleLoader.expandedloadedBundles.Keys)
            {

                ShowBundle(key);
            }


        }



        /// <summary>
        /// Displays a given unloaded bundle
        /// </summary>
        /// <param name="key">Name of the bundle / key for the dictionary</param>
        private static void ShowDiscBundle(string key)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(50)))
            {
                BundleLoader.expandedloadedDiscBundles[key] = !BundleLoader.expandedloadedDiscBundles[key];
                BundleLoader.LoadBundleFromDisc(BundleLoader.discBundles[key]);
                GUILayout.Button(key);
                GUILayout.EndHorizontal();
                return;
            }

            GUILayout.Button(key);
            GUILayout.EndHorizontal();

        }



        /// <summary>
        /// Displays all unloaded bundles
        /// </summary>

        private static void ShowDiscBundles()
        {



            foreach (string key in BundleLoader.expandedloadedDiscBundles.Keys.Skip((int)partialShowAmount.x).Take((int)partialShowAmount.y))
            {

                ShowDiscBundle(key);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<="))
            {
                partialShowAmount.x -= partialShowAmount.z;
                if (partialShowAmount.x < 0) partialShowAmount.x = 0;
            }
            if (GUILayout.Button("=>"))
            {
                partialShowAmount.x += partialShowAmount.z;
                if (partialShowAmount.x > BundleLoader.expandedloadedDiscBundles.Count) partialShowAmount.x = BundleLoader.expandedloadedDiscBundles.Count;
            }
            GUILayout.EndHorizontal();
        }


        /// <summary>
        /// Selects a GameObject in the editor
        /// </summary>
        /// <param name="gmObject">The selected GameObject</param>

        private static void SelectedGameObject(GameObject gmObject)
        {
            if (m_MainCamera != null && m_MainObject != null)
            {

                //m_MainCamera.transform.position = new Vector3(gmObject.transform.position.x, m_MainCamera.transform.position.y, gmObject.transform.position.z);
                SceneEditor.instance.SelectObject(gmObject);

            }


            //highlight it unfreeze it
        }

        /// <summary>
        /// Initializes all required variables
        /// </summary>
        private static void Init()
        {
            m_MainCamera = Camera.main;
            //Channel.Log(m_MainCamera.name);
            if (m_MainCamera != null)
            {
                //Channel.Log("Got Camera");
                inEdit = true;
                //savedTransform = m_MainCamera.transform;  // Currently disabled , will be enabled on the next update

                m_MainObject = new GameObject();
                m_MainObject.name = "SEditMainObject";
                m_MainObject.transform.position = m_MainCamera.transform.position;
                m_MainObject.transform.SetParent(null);
                GameObject.DontDestroyOnLoad(m_MainObject);

                m_MainCamera.gameObject.AddComponent<TransformGizmo>(); // needs to be on camera

                m_MainObject.AddComponent<SceneSearcher>();
                m_MainObject.AddComponent<BundleLoader>();
                m_MainObject.AddComponent<SceneEditor>();
                m_MainObject.AddComponent<SaveLoad>();

                partialShowAmount.x = 0;
                partialShowAmount.y = 100;
                partialShowAmount.z = 100;
            }
            else
            {
                //Channel.Log("Failed to get Camera");
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        private static void EndEditing()
        {
            UnityEngine.Object.Destroy(m_MainCamera.gameObject.GetComponent<TransformGizmo>());
            UnityEngine.Object.Destroy(m_MainObject.GetComponent<SceneSearcher>());
            UnityEngine.Object.Destroy(m_MainObject.GetComponent<BundleLoader>());
            UnityEngine.Object.Destroy(m_MainObject.GetComponent<SceneEditor>());
            UnityEngine.Object.Destroy(m_MainObject.GetComponent<SaveLoad>());
            UnityEngine.Object.Destroy(m_MainObject);
            inEdit = false;
        }


        private static void OnGUI()
        {

            if (!inEdit)
            {
                if (GUILayout.Button("Load SEdit into scene"))
                {
                    Init();
                };
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Unload SEdit", GUILayout.ExpandWidth(false)))
                {
                    EndEditing();

                }
                if (!hasLoaded)
                {
                    if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
                    {
                        hasLoaded = true;
                        SceneEditor.instance.Init();
                    }
                }

                if (GUILayout.Button("Reload", GUILayout.ExpandWidth(false)))
                {
                    SceneSearcher.instance.LoadSceneElements();
                }


                if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
                {
                    SaveLoad.instance.Save();
                    Modification.SaveData(userData);

                }
                userData.AutoLoad = GUILayout.Toggle(userData.AutoLoad, "Use autoload");
                if (userData.AutoLoad)
                {
                    //todo
                }
                GUILayout.EndHorizontal();

                showSceneElements = GUILayout.Toggle(showSceneElements, "Show scenes");
                if (showSceneElements)
                {
                    selectedScene = GUILayout.SelectionGrid(selectedScene, SceneSearcher.scenes.Keys.ToArray(), 3, GUILayout.ExpandWidth(false));
                    if (!SceneSearcher.scenes[SceneSearcher.scenes.Keys.ToArray()[selectedScene]].isEditable)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50 * Utils.scaleFactorX);
                        if (GUILayout.Button("Make current selected scene editable", GUILayout.ExpandWidth(false)))
                        {
                            SceneSearcher.scenes[SceneSearcher.scenes.Keys.ToArray()[selectedScene]].isEditable = true;




                            SceneEditor.instance.MakeSceneEditable(selectedScene);
                            Channel.Log("Made scene editable");
                            if (SceneSearcher.instance != null)
                            {
                                SceneSearcher.instance.ReloadScene(SceneSearcher.scenes.Keys.ToArray()[selectedScene]);

                            }
                            else
                            {
                                Channel.Log("SceneSearcher is null");
                            }
                        }
                        GUILayout.EndHorizontal();

                    }
                    else
                    {
                        GUILayout.Label("Current selected scene is editable", GUILayout.ExpandWidth(false));

                    }

                    ShowSceneData(SceneSearcher.scenes[SceneSearcher.scenes.Keys.ToArray()[selectedScene]]);
                }
                loadBundles = GUILayout.Toggle(loadBundles, "Show active bundles");
                if (loadBundles)
                {
                    GUILayout.Label("================================================================");
                    ShowBundles();
                }
                loadBundlesFromDisc = GUILayout.Toggle(loadBundlesFromDisc, "Get bundles from Disk (RAM!)");
                if (loadBundlesFromDisc)
                {
                    GUILayout.Label("================================================================");
                    ShowDiscBundles();


                }

            }


        }

    }
}