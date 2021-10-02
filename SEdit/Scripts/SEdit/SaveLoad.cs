using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owlcat.Runtime.Core.Logging;
using RuntimeGizmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace OwlcatModification.Modifications.SEdit
{
    public class SaveLoad : MonoBehaviour
    {
        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.SaveLoad");

        public static SaveLoad instance { get; set; }
        public static string savePath { get; set; } = ""; SEditUserData userData = new SEditUserData();

        Dictionary<string, List<SEditData>> sEditDataDictionary { get; set; } = new Dictionary<string, List<SEditData>>();

        Dictionary<string, SEditSceneData> sEditSceneDataDictionary { get; set; } = new Dictionary<string, SEditSceneData>();

        private Dictionary<string, bool> loadedScene { get; set; } = new Dictionary<string, bool>();


        [System.Serializable]
        public class Vector3S
        {
            public float x;
            public float y;
            public float z;

            public Vector3S(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
        [System.Serializable]
        public class Vector4S
        {
            public float x;
            public float y;
            public float z;
            public float w;
            public Vector4S(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }
        }

        [Serializable]
        public class SEditData
        {
            public Vector3S position;
            public Vector4S rotation;
            public Vector3S scale;

            [NonSerialized]
            public int instaceId;

            public string assetPath;
            public string assetName;
            public string gameObjectName;


        }

        [Serializable]
        public class SEditSceneData
        {
            public string sceneName;
            public string scenePath;
            public SEditData[] dataArray;
        }

        /// <summary>
        /// Main object to store gameobjects and associated data
        /// </summary>

        [Serializable]
        public class SEditUserData
        {
            public string GUID;
            public string usedSEditVersion;
            public SEditSceneData[] sceneDataArray;
        }




        public void UpdateSaveElement(GameObject obj)
        {
            if (sEditDataDictionary.ContainsKey(SceneEditor.currentEditableScene))
            {
                int instanceID = obj.GetInstanceID();
                foreach (SEditData data in sEditDataDictionary[SceneEditor.currentEditableScene])
                {
                    if (instanceID == data.instaceId)
                    {
                        data.position.x = obj.transform.position.x;
                        data.position.y = obj.transform.position.y;
                        data.position.z = obj.transform.position.z;
                        data.scale.x = obj.transform.localScale.x;
                        data.scale.y = obj.transform.localScale.y;
                        data.scale.z = obj.transform.localScale.z;
                        data.rotation.x = obj.transform.rotation.x;
                        data.rotation.y = obj.transform.rotation.y;
                        data.rotation.z = obj.transform.rotation.z;
                        return;
                    }
                }
            }
        }

        public void RemoveSaveElement(GameObject obj)
        {
            if (obj != null && sEditDataDictionary.ContainsKey(SceneEditor.currentEditableScene))
            {
                int instanceID = obj.GetInstanceID();
                foreach (SEditData data in sEditDataDictionary[SceneEditor.currentEditableScene])
                {

                    if (instanceID == data.instaceId)
                    {
                        sEditDataDictionary[SceneEditor.currentEditableScene].Remove(data);
                        return;
                    }
                }
            }

        }

        //todo fix shit code
        public void AddSaveElement(GameObject obj, string assetPath, string assetName, Scene scene)
        {

            if (!sEditDataDictionary.ContainsKey(scene.name))
            {
                sEditDataDictionary.Add(scene.name, new List<SEditData>());
            }

            if (sEditSceneDataDictionary.ContainsKey(scene.name))

            {
                SEditSceneData sEditSceneData = sEditSceneDataDictionary[scene.name];

                SEditData sEditData = new SEditData();
                sEditData.assetPath = assetPath;
                sEditData.position = new Vector3S(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
                sEditData.scale = new Vector3S(obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z);
                sEditData.rotation = new Vector4S(obj.transform.rotation.x, obj.transform.rotation.y, obj.transform.rotation.z, obj.transform.rotation.w);
                sEditData.assetName = assetName;
                sEditData.gameObjectName = obj.name;
                sEditData.instaceId = obj.GetInstanceID();
                sEditDataDictionary[scene.name].Add(sEditData);


                sEditSceneData.dataArray = sEditDataDictionary[scene.name].ToArray();


                userData.GUID = ModificationRoot.userData.GUID;
                userData.usedSEditVersion = ModificationRoot.SEDITVERSION;
                userData.sceneDataArray = sEditSceneDataDictionary.Values.ToArray();
                Channel.Log("Scene allready in save");

            }
            else
            {
                SEditSceneData sEditSceneData = new SEditSceneData();

                sEditSceneData.sceneName = scene.name;
                sEditSceneData.scenePath = scene.path;

                SEditData sEditData = new SEditData();
                sEditData.position = new Vector3S(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
                sEditData.scale = new Vector3S(obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z);
                sEditData.rotation = new Vector4S(obj.transform.rotation.x, obj.transform.rotation.y, obj.transform.rotation.z, obj.transform.rotation.w);
                sEditData.assetName = assetName;
                sEditData.assetPath = assetPath;
                sEditData.gameObjectName = obj.name;
                sEditData.instaceId = obj.GetInstanceID();
                sEditDataDictionary[scene.name].Add(sEditData);
                sEditSceneData.dataArray = sEditDataDictionary[scene.name].ToArray();
                sEditSceneDataDictionary.Add(scene.name, sEditSceneData);
                userData.GUID = ModificationRoot.userData.GUID;
                userData.usedSEditVersion = "0.01";
                userData.sceneDataArray = sEditSceneDataDictionary.Values.ToArray();
                Channel.Log($"Scene not in save{scene.name}");

            }
        }



        public void Save()
        {

            JsonSerializerSettings oldSettings = JsonConvert.DefaultSettings();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            string output = JsonConvert.SerializeObject(userData);
            Channel.Log($"Got output {output}");
            JsonConvert.DefaultSettings = () => oldSettings;


            try
            {


                System.IO.Directory.CreateDirectory((ModificationRoot.modPath + "\\SEditData\\"));
                File.WriteAllText(ModificationRoot.modPath + "\\SEditData\\" + ModificationRoot.userData.GUID + ".json", output);


            }
            catch (Exception e)
            {
                Utils.LogError("SaveLoad", $"Got error while saving SaveLoad {e.Message}");
                JsonConvert.DefaultSettings = () => oldSettings;
            }

        }



        private void LoadSaveDataIntoScene(string seditSceneData, int sceneID)
        {
            SceneEditor.instance.MakeSceneEditable(sceneID);
            loadedScene[seditSceneData] = true;

            foreach (SEditData data in sEditSceneDataDictionary[seditSceneData].dataArray)
            {
                data.assetPath += " (UnityEngine.AssetBundle)";
                if (BundleLoader.IsBundleLoaded(data.assetPath, true) && SceneEditor.instance != null)
                {
                    Channel.Log($"Bundle is loaded into scene assetPath {data.assetPath}");

                    Channel.Log($"BundleLoader.bundles.ContainsKey({data.assetPath}) == {BundleLoader.bundles.ContainsKey(data.assetPath)}  with {BundleLoader.bundles[data.assetPath].objects.Count} Objects");

                    Channel.Log($"BundleLoader.bundles[data.assetPath].objects.ContainsKey({data.assetName}) == {BundleLoader.bundles[data.assetPath].objects.ContainsKey(data.assetName)}");


                    GameObject gm = BundleLoader.bundles[data.assetPath].objects[data.assetName];
                    gm.transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
                    gm.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                    gm.transform.rotation.Set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w);
                    SceneEditor.instance.AddObjectToScene(gm, data.assetName, BundleLoader.bundles[data.assetPath].bundle.name, null, false, seditSceneData);

                }
                else
                {
                    Channel.Log($"{data.assetPath} == {BundleLoader.bundles.ContainsKey(data.assetPath)};");
                }

            }


        }
        public void Load()
        {
            JsonSerializerSettings oldSettings = JsonConvert.DefaultSettings();
            try
            {
                string tmp = File.ReadAllText(ModificationRoot.modPath + "\\SEditData\\" + ModificationRoot.userData.GUID + ".json");
                ;
                if (tmp != null && tmp != "")
                {


                    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    userData = JsonConvert.DeserializeObject<SEditUserData>(tmp);
                    foreach (SEditSceneData sData in userData.sceneDataArray)
                    {
                        sEditSceneDataDictionary.Add(sData.sceneName, sData);
                        int sceneCheck = SceneSearcher.IsSceneActive(sData.sceneName);

                        if (sceneCheck >= 0)
                        {
                            //Scene is found
                            Channel.Log("Scenecheck >= 0");
                            LoadSaveDataIntoScene(sData.sceneName, sceneCheck);
                        }
                        else
                        {
                            Channel.Log("Scenecheck <= 0");
                        }
                    }
                    SceneEditor.instance.currentObj = null;
                    JsonConvert.DefaultSettings = () => oldSettings;
                    Channel.Log($"Loaded {tmp} and userdata {userData.GUID}");
                }
                else
                {
                    Utils.LogError("SaveLoad", $"Failed to load tmp is null or \"\" :{tmp}");
                }

            }
            catch (Exception e)
            {
                Utils.LogError("SaveLoad", $"Got error while loading Save {e.Message}");
                JsonConvert.DefaultSettings = () => oldSettings;
            }




        }


        void Start()
        {
            SaveLoad.instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

        }


        void OnSceneUnloaded(Scene scene)
        {

            Channel.Log("Unloaded currentEditableScene Scene!");
            if (loadedScene.ContainsKey(scene.name))
            {
                loadedScene[scene.name] = false;

            }
            if (sEditDataDictionary.ContainsKey(scene.name))
            {
                sEditDataDictionary[scene.name].Clear();
            }

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string name = scene.name;
            Channel.Log($"Scene loaded with name{name}");
            if (!loadedScene.ContainsKey(scene.name))
            {
                loadedScene.Add(scene.name, false);
            }
            foreach (string sData in sEditSceneDataDictionary.Keys)
            {

                if (loadedScene.ContainsKey(sData) && !loadedScene[sData])
                {
                    int sceneCheck = SceneSearcher.IsSceneActive(sData);

                    if (sceneCheck >= 0 && SceneEditor.instance != null)
                    {
                        //Scene is found
                        Channel.Log("Scenecheck >= 0.Trying to add TransformGizmo");
                        if (Camera.main.gameObject.GetComponent<TransformGizmo>() == null)
                        {
                            Camera.main.gameObject.AddComponent<TransformGizmo>();
                        }

                        LoadSaveDataIntoScene(sData, sceneCheck);
                        SceneEditor.instance.currentObj = null; // dont show "editor"
                    }
                    else
                    {
                        Utils.LogError("SaveLoad", $"Scenecheck <= 0 || sceneEditor = {SceneEditor.instance == null} ");
                    }
                }
                else
                {

                }
            }


        }
    }
}