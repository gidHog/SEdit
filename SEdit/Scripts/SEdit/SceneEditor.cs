using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting;
using RuntimeGizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static OwlcatModification.Modifications.SEdit.SceneSearcher;

namespace OwlcatModification.Modifications.SEdit
{
    public class SceneEditor : MonoBehaviour
    {


        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.SceneEditor");



        private static GameObject sceneEditorObj;
        private static GameObject userSEditObj;
        public SceneSearcher sceneSearcher = null;
        public SaveLoad saveLoad = null;
        public bool hasObjectToAdd = false;
        public GameObject currentObj = null;
        public Transform parentTransform = null;
        public TransformGizmo gizmo { get; set; } = null;

        private string assetName = "";
        private string assetPath = "";



        public void Init()
        {
            saveLoad.sceneEditor = this;
            saveLoad.Load();
            sceneSearcher.ReloadScene(SceneSearcher.scenes.Keys.ToArray()[ModificationRoot.selectedScene]);
        }

        public static string currentEditableScene;

        public void MakeSceneEditable(int sceneId = 0)
        {
            Channel.Log("Trying to make scene editable");
            sceneEditorObj = new GameObject("SceneEditor");
            SceneWrapper scene = SceneSearcher.scenes.Values.ToArray()[sceneId];
            scene.isEditable = true;
            currentEditableScene = scene.scene.name;
            sceneEditorObj.transform.SetParent(SceneSearcher.scenes.Values.ToArray()[sceneId].sceneRoot);
            Channel.Log(currentEditableScene);
            sceneEditorObj.transform.parent = null;
            userSEditObj = new GameObject(ModificationRoot.userData.GUID);
            userSEditObj.transform.transform.SetParent(sceneEditorObj.transform);
            if (sceneSearcher != null)
            {
                Channel.Log($"scenesearcher{sceneSearcher.name}");
            }

        }

        public void SelectObject(GameObject obj)
        {
            saveLoad.UpdateSaveElement(currentObj);
            gizmo.RemoveTarget(currentObj.transform);
            Utils.HighlightColor(currentObj.transform, false);
            this.currentObj = obj;
            Utils.HighlightColor(currentObj.transform);
            gizmo.AddTarget(currentObj.transform);
        }

        public void AddObjectToScene(GameObject obj, string assetName = "please enter a name", string assetPath = "please enter a path", Transform at = null, bool waitForMouseClick = true, string sceneKey = "")
        {


            this.parentTransform = at;

            if (currentObj != null && gizmo != null)
            {
                saveLoad.UpdateSaveElement(currentObj);
                gizmo.RemoveTarget(currentObj.transform);
                Utils.HighlightColor(currentObj.transform, false);
            }
            if (obj != null)
            {

                this.currentObj = obj;
                this.assetName = assetName;
                this.assetPath = assetPath;
                if (!waitForMouseClick)
                {
                    Channel.Log($"Trying saved object with name: {obj.name}");
                    currentObj = GameObject.Instantiate(obj, userSEditObj.transform);

                    //Utils.HighlightColor(objToAdd.transform);
                    //gizmo.AddTarget(objToAdd.transform);
                    if (saveLoad != null)
                    {
                        saveLoad.AddSaveElement(currentObj, assetPath, assetName, SceneSearcher.scenes[currentEditableScene].scene);
                        saveLoad.Save();
                    }
                    obj = null;
                }
                else
                {
                    this.hasObjectToAdd = true;
                    Channel.Log($"Trying to add {obj.name} with path {assetPath}");
                }
            }

            sceneSearcher.ReloadScene(currentEditableScene);

        }


        public void Update()
        {
            if (hasObjectToAdd)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    hasObjectToAdd = !hasObjectToAdd;
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ModificationRoot.m_MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, Physics.DefaultRaycastLayers))
                    {
                        Transform target = hitInfo.transform;
                        Channel.Log($"Trying to add with ray: {currentObj.name}");
                        currentObj = GameObject.Instantiate(currentObj, userSEditObj.transform);
                        currentObj.transform.position = target.position;
                        Utils.HighlightColor(currentObj.transform);
                        Channel.Log($"Trying to reload with key {SceneSearcher.scenes.Keys.ToArray()[ModificationRoot.selectedScene]}");
                        sceneSearcher.ReloadScene(SceneSearcher.scenes.Keys.ToArray()[ModificationRoot.selectedScene]);
                    }
                    else
                    {
                        Channel.Log($"Trying to add without ray: {currentObj.name}");
                        GameObject objNew = GameObject.Instantiate(currentObj, userSEditObj.transform);
                    }
                    if (saveLoad != null)
                    {
                        saveLoad.AddSaveElement(currentObj, assetPath, assetName, SceneSearcher.scenes.Values.ToArray()[ModificationRoot.selectedScene].scene);
                        saveLoad.Save();
                    }
                    else
                    {
                        Channel.Log("SaveLoad element is null!");
                    }
                    gizmo.AddTarget(currentObj.transform);

                }
                else if (Input.GetKey(KeyCode.Delete))
                {
                    RemoveSelectedGameObject();

                }

            }
        }


        public void RemoveSelectedGameObject()
        {
            saveLoad.RemoveSaveElement(currentObj);
            GameObject.Destroy(currentObj);
            sceneSearcher.ReloadScene(SceneSearcher.scenes.Keys.ToArray()[ModificationRoot.selectedScene]);
        }





        void OnGUI()
        {
            float newX, newY, newZ;
            if (currentObj != null)
            {
                GUILayout.BeginArea(new Rect(10 * Utils.scaleFactorX, 10 * Utils.scaleFactorY, 300 * Utils.scaleFactorX, 400 * Utils.scaleFactorY));
                GUILayout.BeginHorizontal();


                switch (gizmo.transformType)
                {
                    case TransformType.Move:
                        GUILayout.Label("Move");
                        break;
                    case TransformType.Rotate:
                        GUILayout.Label("Rotate");
                        break;
                    case TransformType.Scale:
                        GUILayout.Label("Scale");
                        break;
                    case TransformType.All:
                        GUILayout.Label("All");
                        break;
                    default:
                        GUILayout.Label("Unknown");
                        break;
                }
                GUILayout.Label("Speed");
                gizmo.speed = float.Parse(GUILayout.TextField($"{gizmo.speed}"));
                GUILayout.EndHorizontal();
                //  foreach (Component comp in objToAdd.GetComponents(typeof(Component)))
                //  {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Position");

                GUILayout.BeginVertical();
                GUILayout.Label("X");
                newX = float.Parse(GUILayout.TextField($"{currentObj.transform.position.x}"));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Y");
                newY = float.Parse(GUILayout.TextField($"{currentObj.transform.position.y}"));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Z");
                newZ = float.Parse(GUILayout.TextField($"{currentObj.transform.position.z}"));
                currentObj.transform.position = new Vector3(newX, newY, newZ);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                //Rotation
                GUILayout.BeginHorizontal();
                GUILayout.Label("Rotation");
                GUILayout.BeginVertical();

                GUILayout.Label("X");
                newX = float.Parse(GUILayout.TextField($"{currentObj.transform.rotation.x}"));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Y");
                newY = float.Parse(GUILayout.TextField($"{currentObj.transform.rotation.y}"));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Z");
                newZ = float.Parse(GUILayout.TextField($"{currentObj.transform.rotation.z}"));

                currentObj.transform.rotation = new Quaternion(newX, newY, newZ, currentObj.transform.rotation.w);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                //Scale
                GUILayout.BeginHorizontal();
                GUILayout.Label("Scale");
                GUILayout.BeginVertical();

                GUILayout.Label("X");
                newX = float.Parse(GUILayout.TextField($"{currentObj.transform.localScale.x}"));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Y");
                newY = float.Parse(GUILayout.TextField($"{currentObj.transform.localScale.y}"));
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Z");
                newZ = float.Parse(GUILayout.TextField($"{currentObj.transform.localScale.z}"));

                currentObj.transform.localScale = new Vector3(newX, newY, newZ);
                GUILayout.EndVertical();


                GUILayout.EndHorizontal();
                //  }
                GUILayout.EndArea();
            }
            // Make a group on the center of the screen
            if (gizmo.hasChanged)
            {
                gizmo.hasChanged = false;
                saveLoad.UpdateSaveElement(currentObj);
            }
        }

    }
}