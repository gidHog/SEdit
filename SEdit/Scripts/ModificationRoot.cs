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
        public static Camera m_MainCamera;
        [Serializable]
        public class ModificationData
        {
            public string GUID;
            public string usedSEditVersion;
            public bool AutoLoad;
        }

        public static ModificationData userData;

        public static string SEDITVERSION = "0.01";// current SEdit-version
        private static bool inEdit = false; // if the editor is enabled
        private static bool showSceneElements = false; // if scene elements are shown
        private static bool loadBundles = false;
        private static bool loadBundlesFromDisc = false;
        public static int selectedScene = 0;// currently selected subscene

        private static Vector3 partialShowAmount;// amount of shown bundles per page and their index
                                                 //private static Transform savedTransform = null;

        public static SceneEditor sceneEditor = null;
        private static SceneSearcher sceneSearcher = null;
        private static SaveLoad saveLoad = null;

        //public static GameObject m_MainObject; // currently disabled

        public static string modPath;
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
                GUILayout.Space((50 * depth * Utils.scaleFactorX));
                if (currentNode.rootGameobjects.Count > 0)
                {

                    if (GUILayout.Button("+", GUILayout.Width(50 * Utils.scaleFactorX)))
                    {
                        currentNode.isExpanded = !currentNode.isExpanded;
                    }


                }
                else
                {
                    GUILayout.Button("=", GUILayout.Width(50 * Utils.scaleFactorX));

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
                        GUILayout.Space((50 * depth * Utils.scaleFactorX));
                        if (node.rootGameobjects.Count > 0)
                        {
                            if (GUILayout.Button("+", GUILayout.Width(50 * Utils.scaleFactorX)))
                            {
                                node.isExpanded = !currentNode.isExpanded;
                            }

                        }
                        else
                        {
                            GUILayout.Button("=", GUILayout.Width(50 * Utils.scaleFactorX));

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
            if (GUILayout.Button("+", GUILayout.Width(50)))
            {
                BundleLoader.expandedloadedBundles[key] = !BundleLoader.expandedloadedBundles[key];
                GUILayout.Button(key, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }
            else if (BundleLoader.expandedloadedBundles[key])
            {
                //show assets

                GUILayout.Button(key);
                GUILayout.EndHorizontal();
                BundleLoader.bundles[key].LoadBundle();


                foreach (string assetName in BundleLoader.bundles[key].objects.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(100);
                    if (GUILayout.Button(assetName + (" (GameObject)"), GUILayout.ExpandWidth(false)))
                    {

                        sceneEditor.AddObjectToScene(BundleLoader.bundles[key].objects[assetName], assetName, BundleLoader.bundles[key].bundle.name);


                    }
                    GUILayout.EndHorizontal();
                }
                foreach (string assetName in BundleLoader.bundles[key].sprites.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(100);
                    if (GUILayout.Button(assetName + (" (Sprite)"), GUILayout.ExpandWidth(false)))
                    {
                        //todo maybe drag and drop?
                    }
                    GUILayout.EndHorizontal();
                }
                foreach (string assetName in BundleLoader.bundles[key].meshes.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(100);
                    if (GUILayout.Button(assetName + (" (Mesh)"), GUILayout.ExpandWidth(false)))
                    {
                        //todo spawn create gameobject with mat etc.
                    }
                    GUILayout.EndHorizontal();
                }

                foreach (string assetName in BundleLoader.bundles[key].textures2D.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(100);
                    if (GUILayout.Button(assetName + (" (Texture2D)"), GUILayout.ExpandWidth(false)))
                    {
                        //todo spawn create gameobject with mat etc.
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
            if (m_MainCamera != null)
            {

                //m_MainCamera.transform.position = new Vector3(gmObject.transform.position.x, m_MainCamera.transform.position.y, gmObject.transform.position.z);
                sceneEditor.SelectObject(gmObject);

            }


            //highlight it unfreeze it
        }

        /// <summary>
        /// Initializes all required variables
        /// </summary>
        private static void Init()
        {
            m_MainCamera = Camera.main;
            Channel.Log(m_MainCamera.name);
            if (m_MainCamera != null)
            {
                Channel.Log("Got Camera");
                inEdit = true;
                /*savedTransform = m_MainCamera.transform;  // Currently disabled , will be enabled on the next update
		
				m_MainObject = new GameObject();
				m_MainObject.name = "SEditMainObject";
				GameObject.DontDestroyOnLoad(m_MainObject);
				m_MainObject.transform.position = m_MainCamera.transform.position;
				m_MainObject.transform.SetParent(null);
				*/

                m_MainCamera.gameObject.AddComponent<TransformGizmo>();
                m_MainCamera.gameObject.AddComponent<SceneSearcher>();
                m_MainCamera.gameObject.AddComponent<BundleLoader>();
                m_MainCamera.gameObject.AddComponent<SceneEditor>();
                m_MainCamera.gameObject.AddComponent<SaveLoad>();

                sceneEditor = m_MainCamera.gameObject.GetComponent<SceneEditor>();
                sceneSearcher = m_MainCamera.gameObject.GetComponent<SceneSearcher>();
                saveLoad = m_MainCamera.gameObject.GetComponent<SaveLoad>();
                sceneEditor.saveLoad = saveLoad;
                //Requires camera!
                sceneEditor.gizmo = m_MainCamera.gameObject.GetComponent<TransformGizmo>();
                sceneEditor.sceneSearcher = sceneSearcher;

                partialShowAmount.x = 0;
                partialShowAmount.y = 100;
                partialShowAmount.z = 100;
            }
            else
            {
                Channel.Log("Failed to get Camera");
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        private static void EndEditing()
        {
            UnityEngine.Object.Destroy(m_MainCamera.gameObject.GetComponent<TransformGizmo>());
            UnityEngine.Object.Destroy(m_MainCamera.gameObject.GetComponent<SceneSearcher>());
            UnityEngine.Object.Destroy(m_MainCamera.gameObject.GetComponent<BundleLoader>());
            UnityEngine.Object.Destroy(m_MainCamera.gameObject.GetComponent<SceneEditor>());
            UnityEngine.Object.Destroy(m_MainCamera.gameObject.GetComponent<SaveLoad>());
            UnityEngine.Object.Destroy(m_MainCamera);
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
                if (GUILayout.Button("Stop editing", GUILayout.ExpandWidth(false)))
                {
                    EndEditing();

                }
                if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
                {
                    //m_MainCamera.transform.position = savedTransform.position;
                    sceneEditor.Init();
                }
                if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
                {
                    saveLoad.Save();
                    Modification.SaveData(userData);

                }
                userData.AutoLoad = GUILayout.Toggle(userData.AutoLoad, "Use autoload");
                if (userData.AutoLoad)
                {
                    //
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

                            sceneEditor.gizmo = m_MainCamera.GetComponent<TransformGizmo>();
                            sceneEditor.sceneSearcher = m_MainCamera.GetComponent<SceneSearcher>();

                            sceneEditor.MakeSceneEditable(selectedScene);
                            Channel.Log("Made scene editable");
                            if (sceneSearcher != null)
                            {
                                sceneSearcher.ReloadScene(SceneSearcher.scenes.Keys.ToArray()[selectedScene]);

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