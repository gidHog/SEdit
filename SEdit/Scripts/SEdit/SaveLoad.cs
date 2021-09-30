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

        public static string savePath { get; set; } = "";


        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.SaveLoad");



        SEditUserData userData = new SEditUserData();


        List<SEditData> sEditDataArrayList = new List<SEditData>();



        Dictionary<string, SEditSceneData> sEditSceneDataDictionary = new Dictionary<string, SEditSceneData>();

        public SceneEditor sceneEditor = null;

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
            int instanceID = obj.GetInstanceID();
            foreach (SEditData data in sEditDataArrayList)
            {

                if (instanceID == data.instaceId)
                {
                    Channel.Log($"obj instanceId: {instanceID} data {data.instaceId}");
                    data.position.x = obj.transform.position.x;
                    data.position.y = obj.transform.position.y;
                    data.position.z = obj.transform.position.z;
                    data.scale.x = obj.transform.localScale.x;
                    data.scale.y = obj.transform.localScale.y;
                    data.scale.z = obj.transform.localScale.z;
                    data.rotation.x = obj.transform.rotation.x;
                    data.rotation.y = obj.transform.rotation.y;
                    data.rotation.z = obj.transform.rotation.z;
                }
            }
        }

        public void RemoveSaveElement(GameObject obj)
        {
            if (obj != null)
            {
                int instanceID = obj.GetInstanceID();
                foreach (SEditData data in sEditDataArrayList)
                {

                    if (instanceID == data.instaceId)
                    {
                        sEditDataArrayList.Remove(data);
                        return;
                    }
                }
            }

        }

        //todo fix shit code
        public void AddSaveElement(GameObject obj, string assetPath, string assetName, Scene scene)
        {

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
                sEditDataArrayList.Add(sEditData);


                sEditSceneData.dataArray = sEditDataArrayList.ToArray();


                userData.GUID = ModificationRoot.userData.GUID;
                userData.usedSEditVersion = ModificationRoot.SEDITVERSION;
                userData.sceneDataArray = sEditSceneDataDictionary.Values.ToArray();
                Channel.Log("Scene allready in save");
                Save();
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
                sEditDataArrayList.Add(sEditData);
                sEditSceneData.dataArray = sEditDataArrayList.ToArray();
                sEditSceneDataDictionary.Add(scene.name, sEditSceneData);
                userData.GUID = ModificationRoot.userData.GUID;
                userData.usedSEditVersion = "0.01";
                userData.sceneDataArray = sEditSceneDataDictionary.Values.ToArray();
                Channel.Log($"Scene not in save{scene.name}");
                Save();
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
                Channel.Log($"Got error while saving SaveLoad {e.Message}");
            }

        }



        private void LoadSaveDataIntoScene(string seditSceneData, int sceneID)
        {
            sceneEditor.MakeSceneEditable(sceneID);
            foreach (SEditData data in sEditSceneDataDictionary[seditSceneData].dataArray)
            {
                data.assetPath += " (UnityEngine.AssetBundle)";
                if (BundleLoader.IsBundleLoaded(data.assetPath, true) && sceneEditor != null)
                {
                    Channel.Log($"Bundle is loaded into scene assetPath {data.assetPath}");

                    Channel.Log($"BundleLoader.bundles.ContainsKey({data.assetPath}) == {BundleLoader.bundles.ContainsKey(data.assetPath)}  with {BundleLoader.bundles[data.assetPath].objects.Count} Objects");

                    Channel.Log($"BundleLoader.bundles[data.assetPath].objects.ContainsKey({data.assetName}) == {BundleLoader.bundles[data.assetPath].objects.ContainsKey(data.assetName)}");


                    GameObject gm = BundleLoader.bundles[data.assetPath].objects[data.assetName];
                    gm.transform.position = new Vector3(data.position.x, data.position.y, data.position.z);
                    gm.transform.localScale = new Vector3(data.scale.x, data.scale.y, data.scale.z);
                    gm.transform.rotation.Set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w);
                    sceneEditor.AddObjectToScene(gm, data.assetName, BundleLoader.bundles[data.assetPath].bundle.name, null, false, seditSceneData);

                }
                else
                {
                    Channel.Log($"{data.assetPath} == {BundleLoader.bundles.ContainsKey(data.assetPath)};");
                }

            }


        }
        public void Load()
        {
            try
            {
                string tmp = File.ReadAllText(ModificationRoot.modPath + "\\SEditData\\" + ModificationRoot.userData.GUID + ".json");

                if (tmp != null && tmp != "")
                {
                    JsonSerializerSettings settings = JsonConvert.DefaultSettings();

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
                    sceneEditor.currentObj = null;
                    JsonConvert.DefaultSettings = () => settings;
                    Channel.Log($"Loaded {tmp} and userdata {userData.GUID}");
                }
                else
                {
                    Channel.Log($"Failed to load tmp is null or \"\" :{tmp}");
                }

            }
            catch (Exception e)
            {
                Channel.Log($"Got error while loading Save {e.Message}");
            }




        }


        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

        }


        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string name = scene.name;
            Channel.Log($"Scene loaded with name{name}");
            foreach (string sData in sEditSceneDataDictionary.Keys)
            {

                int sceneCheck = SceneSearcher.IsSceneActive(sData);

                if (sceneCheck >= 0 && sceneEditor != null)
                {
                    //Scene is found
                    Channel.Log("Scenecheck >= 0.Trying to add TransformGizmo");
                    if (Camera.main.gameObject.GetComponent<TransformGizmo>() == null)
                    {
                        Camera.main.gameObject.AddComponent<TransformGizmo>();
                    }
                    sceneEditor.gizmo = Camera.main.gameObject.GetComponent<TransformGizmo>();
                    LoadSaveDataIntoScene(sData, sceneCheck);
                }
                else
                {
                    Channel.Log($"Scenecheck <= 0 || sceneEditor = {sceneEditor == null} ");
                }

            }


        }
    }
}