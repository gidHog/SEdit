using Owlcat.Runtime.Core.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace OwlcatModification.Modifications.SEdit
{
    public class SceneSearcher : MonoBehaviour
    {

        public string sceneName { get; private set; }


        public Scene activeScene { get; private set; }

        public static Dictionary<string, SceneWrapper> scenes = new Dictionary<string, SceneWrapper>();
        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.SceneSearcher");


        public class GameObjectWrapper
        {
            public GameObject gameobject;



            public GameObjectWrapper()
            {

            }
            public GameObjectWrapper(GameObject obj)
            {
                this.gameobject = obj;
            }
        }

        public class Node
        {
            public GameObjectWrapper obj { get; set; } = null;
            public Dictionary<int, Node> rootGameobjects = new Dictionary<int, Node>();
            public bool isExpanded = false;
            public void fill()
            {
                List<GameObject> childs = new List<GameObject>();
                if (obj != null)
                {
                    foreach (Transform tmp in obj.gameobject.transform)
                    {
                        if (obj.gameobject.GetInstanceID() != tmp.gameObject.GetInstanceID())
                        {
                            childs.Add(tmp.gameObject);

                        }
                    }
                    if (childs.Count > 0)
                    {
                        foreach (GameObject gameObj in childs)

                        {
                            Node tmp = new Node();
                            GameObjectWrapper wrapper = new GameObjectWrapper();
                            wrapper.gameobject = gameObj;
                            tmp.obj = wrapper;
                            //Channel.Log("# # # #Found:" + gameObj.name + " with instanceID" + gameObj.GetInstanceID());
                            if (!rootGameobjects.ContainsKey(obj.gameobject.GetInstanceID()))
                            {
                                rootGameobjects.Add(gameObj.GetInstanceID(), tmp);
                                tmp.fill();
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    foreach (Node tmp in rootGameobjects.Values)
                    {
                        tmp.fill();
                    }
                }
            }
        }

        public class SceneWrapper
        {
            public Scene scene { get; set; }

            public GameObjectWrapper[] rootGameObjects;

            public Transform sceneRoot;

            public List<Node> nodes = new List<Node>();
            public bool hasUpdated { get; set; } = false;

            public bool isEditable = false;

            public void AddRootGameObjects(GameObject[] obj)
            {
                List<GameObjectWrapper> tmp = new List<GameObjectWrapper>();
                foreach (GameObject i in obj)
                {
                    tmp.Add(new GameObjectWrapper(i));
                }
                rootGameObjects = tmp.ToArray();
                if (rootGameObjects.Length > 0)
                {
                    this.sceneRoot = rootGameObjects[0].gameobject.GetComponentInParent<Transform>();
                }
            }

            public void HasChanged()
            {
                hasUpdated = false;
            }
            public void fill()
            {
                hasUpdated = true;


            }




        }

        public void ReloadScene(string key)
        {
            Channel.Log($"Reload got key:{key}");
            scenes[key].nodes = new List<Node>();
            scenes[key].AddRootGameObjects(scenes[key].scene.GetRootGameObjects());
            StartCoroutine(LoadGameObjects(scenes[key]));
        }



        void Start()
        {
            LoadSceneElements();
        }



        public static int IsSceneActive(string name)
        {
            //Channel.Log($"Checking for active Scene{name} contains{scenes.ContainsKey(name)}");
            scenes.Clear();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {

                Scene tmp = SceneManager.GetSceneAt(i);
                SceneWrapper scenewrapper = new SceneWrapper();
                scenewrapper.scene = tmp;
                scenewrapper.AddRootGameObjects(tmp.GetRootGameObjects());
                scenes.Add(tmp.name, scenewrapper);
                Channel.Log($"Found Scene with name: {tmp.name}");
            }
            if (scenes.ContainsKey(name))
            {
                int count = scenes.Values.Count;
                SceneWrapper[] scenesWrapped = scenes.Values.ToArray();
                int i = 0;
                for (; i < count; i++)
                {
                    Channel.Log($"Checking for active Scene {scenesWrapped[i].scene.name} == {name}");
                    if (scenesWrapped[i].scene.name == name)
                    {
                        return i;
                    }
                }

            }
            return -1;
        }

        private void LoadSceneElements()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {

                Scene tmp = SceneManager.GetSceneAt(i);
                SceneWrapper scenewrapper = new SceneWrapper();
                scenewrapper.scene = tmp;
                scenewrapper.AddRootGameObjects(tmp.GetRootGameObjects());
                scenes.Add(tmp.name, scenewrapper);
                Channel.Log($"Found Scene with name: {tmp.name}");
            }
            activeScene = SceneManager.GetActiveScene();
            sceneName = SceneManager.GetActiveScene().name;
            if (activeScene != null)
            {
                Debug.Log("Got active scene, with name:" + sceneName);
                init();
            }
            else
            {
                Debug.Log("Error getting scene");
            }
            foreach (SceneWrapper subScene in scenes.Values)
            {
                StartCoroutine(LoadGameObjects(subScene));
            }
        }


        IEnumerator LoadGameObjects(SceneWrapper sceneWrapper)
        {
            foreach (GameObjectWrapper wrappedgameobject in sceneWrapper.rootGameObjects)
            {
                GameObject gameobject = wrappedgameobject.gameobject;
                Channel.Log("Found:" + gameobject.name + " with instanceID" + gameobject.GetInstanceID());

                Node tmp = new Node();
                tmp.obj = wrappedgameobject;
                tmp.fill();
                sceneWrapper.nodes.Add(tmp);

            }
            yield return null;
        }


        private void init()
        {


        }

    }
}