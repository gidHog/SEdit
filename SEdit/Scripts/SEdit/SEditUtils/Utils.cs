using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting;
using UnityEngine;
namespace OwlcatModification.Modifications.SEdit
{
    public class Utils
    {

        private static readonly LogChannel Channel = LogChannelFactory.GetOrCreate("SEdit.Error");


        public static float currentScreenWidth = Screen.width;
        public static float currentScreenHeight = Screen.height;

        public static float scaleFactorX = Screen.width / 1920.0f;
        public static float scaleFactorY = Screen.width / 1080.0f;
        public static float buttonWidth = 50 * Utils.scaleFactorX;
        public static void HighlightColor(Transform target, bool mode = true)
        {

            if (target != null)
            {
                Highlighter tmp = target.gameObject.GetComponent<Highlighter>();
                if (tmp != null)
                {
                    GameObject.Destroy(tmp);
                }
                target.gameObject.AddComponent<Highlighter>();
                Highlighter highlighter = target.gameObject.GetComponent<Highlighter>();
                highlighter.FlashingOff();
                if (mode) highlighter.ConstantOn(Color.blue);
                else highlighter.ConstantOff();
            }

        }
        

        public static void LogError(string from, string errorMessage)
        {
            Channel.Log($"Got Error: {errorMessage} from : {from}");
        }
    }
}