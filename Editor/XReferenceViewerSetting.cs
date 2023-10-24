using UnityEditor;
using UnityEngine;

namespace XReferenceViewer.Editor
{
    public class XReferenceViewerSetting : ScriptableObject
    {
        private static XReferenceViewerSetting _Inst;
        
        public static XReferenceViewerSetting Inst
        {
            get
            {
                if (_Inst == null)
                {
                    _Inst = new XReferenceViewerSetting();
                }

                return _Inst;
            }
        }
        
        public float VerticalPadding = 20;
        public float HorizontalPadding = 100;
    }
}