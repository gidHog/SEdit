using System.Linq;
using UnityEngine;
using Owlcat.Runtime.Core.Logging;
using RuntimeGizmos;
using static OwlcatModification.Modifications.SEdit.SceneSearcher;

namespace OwlcatModification.Modifications.SEdit
{
    public class SceneEditor : MonoBehaviour
    {


        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.SceneEditor");


        public static SceneEditor instance { get; set; }
        private static GameObject sceneEditorObj { get; set; }
        private static GameObject userSEditObj { get; set; }


        public bool hasObjectToAdd { get; set; } = false;
        public GameObject currentObj { get; set; } = null;
        public Transform parentTransform { get; set; } = null;

        private string assetName { get; set; } = "";
        private string assetPath { get; set; } = "";

        public void Start()
        {
            SceneEditor.instance = this;
        }

        public void Init()
        {

            if (SaveLoad.instance != null)
            {
                SaveLoad.instance.Load();
            }
            if (SceneSearcher.instance != null)
            {
                SceneSearcher.instance.ReloadScene(SceneSearcher.scenes.Keys.ToArray()[ModificationRoot.selectedScene]);
            }

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
            if (SceneSearcher.instance != null)
            {
                Channel.Log($"scenesearcher{SceneSearcher.instance.name}");
            }

        }

        public void SelectObject(GameObject obj)
        {
            if (TransformGizmo.instance != null)
            {
                Channel.Log($"Selected Obj with name {obj.name}");
                SaveLoad.instance.UpdateSaveElement(currentObj);
                if (currentObj != null)
                {
                    TransformGizmo.instance.RemoveTarget(currentObj.transform);
                    Utils.HighlightColor(currentObj.transform, false);
                }


                currentObj = obj;
                Utils.HighlightColor(currentObj.transform);
                TransformGizmo.instance.AddTarget(currentObj.transform);
            }
            else
            {
                Channel.Log("TransformGizmo.instance ==null");
            }

        }

        public void DeselectCurrentObj()
        {
            if (currentObj != null && TransformGizmo.instance != null)
            {
                SaveLoad.instance.UpdateSaveElement(currentObj);
                TransformGizmo.instance.RemoveTarget(currentObj.transform);
                Utils.HighlightColor(currentObj.transform, false);
                currentObj = null;
            }
        }

        public void AddObjectToScene(GameObject obj, string assetName = "please enter a name", string assetPath = "please enter a path", Transform at = null, bool waitForMouseClick = true, string sceneKey = "")
        {


            this.parentTransform = at;

            if (currentObj != null && TransformGizmo.instance != null)
            {
                SaveLoad.instance.UpdateSaveElement(currentObj);
                TransformGizmo.instance.RemoveTarget(currentObj.transform);
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
                    if (SaveLoad.instance != null)
                    {
                        SaveLoad.instance.AddSaveElement(currentObj, assetPath, assetName, SceneSearcher.scenes[sceneKey].scene);
                        SaveLoad.instance.Save();
                    }
                    obj = null;
                }
                else
                {
                    this.hasObjectToAdd = true;
                    Channel.Log($"Trying to add {obj.name} with path {assetPath}");
                }
            }

            SceneSearcher.instance.ReloadScene(currentEditableScene);

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
                        Channel.Log($"Trying to reload with key {SceneSearcher.scenes[currentEditableScene]}");
                        SceneSearcher.instance.ReloadScene(currentEditableScene);
                    }
                    else
                    {
                        Channel.Log($"Trying to add without ray: {currentObj.name}");
                        GameObject objNew = GameObject.Instantiate(currentObj, userSEditObj.transform);
                    }
                    if (SaveLoad.instance != null)
                    {
                        SaveLoad.instance.AddSaveElement(currentObj, assetPath, assetName, SceneSearcher.scenes[currentEditableScene].scene);
                        SaveLoad.instance.Save();
                    }
                    else
                    {
                        Channel.Log("SaveLoad.instance element is null!");
                    }
                    if (TransformGizmo.instance != null)
                    {
                        TransformGizmo.instance.AddTarget(currentObj.transform);
                    }
                    else
                    {
                        Channel.Log("Gizmo = null");
                    }


                }
                else if (Input.GetKey(KeyCode.Delete))
                {
                    RemoveSelectedGameObject();

                }

            }
        }


        public void RemoveSelectedGameObject()
        {
            SaveLoad.instance.RemoveSaveElement(currentObj);
            Object.Destroy(currentObj);
            SceneSearcher.instance.ReloadScene(currentEditableScene);
        }



        float newX, newY, newZ;

        void OnGUI()
        {

            if (TransformGizmo.instance != null)
            {
                if (currentObj != null)
                {
                    GUILayout.BeginArea(new Rect(10 * Utils.scaleFactorX, 10 * Utils.scaleFactorY, 300 * Utils.scaleFactorX, 400 * Utils.scaleFactorY));
                    GUILayout.BeginHorizontal();


                    switch (TransformGizmo.instance.transformType)
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
                    TransformGizmo.instance.speed = float.Parse(GUILayout.TextField($"{TransformGizmo.instance.speed}"));
                    GUILayout.EndHorizontal();
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
                if (TransformGizmo.instance.hasChanged)
                {
                    TransformGizmo.instance.hasChanged = false;
                    SaveLoad.instance.UpdateSaveElement(currentObj);
                }
            }

        }

    }
}