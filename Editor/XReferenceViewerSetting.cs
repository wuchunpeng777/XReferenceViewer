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
                    
                }

                return _Inst;
            }
        }
    }
}