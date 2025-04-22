using System;
using System.Collections.Generic;
using UnityEngine;

namespace RIPinc.GoogleSheet
{
    [CreateAssetMenu(fileName = "GoogleSheetManager", menuName = "RIPinc/GoogleSheet/GoogleSheetManager")]
    public class GoogleSheetManager : ScriptableObject
    {
        private static GoogleSheetManager _instance = null;
        private const string _path = "GoogleSheetManager";
        public static GoogleSheetManager Instance
        {
            get
            {
                if (!Application.isPlaying)
                {
                    Debug.Assert(true, "에디터에선 접근 하지 마세요 'Editor_Instance' 사용");
                }

#if UNITY_EDITOR
                if (!_instance)
                    _instance = Resources.Load<GoogleSheetManager>(_path);

#endif
                return _instance;
            }
        }
        
        public static GoogleSheetManager Editor_Instance
        {
            get
            {
                var instance = Resources.Load<GoogleSheetManager>(_path);
                return instance;
            }
        }
        
        
        // [AssetList(AutoPopulate = true)]
        public List<GoogleSheet> Sheets;

         public static int DomainReloadHash { get; private set; }
        
        public void Awake()
        {
            _instance = Resources.Load<GoogleSheetManager>(_path);
            DomainReloadHash = DateTime.Now.Second;
        }

        public void OnDestroy()
        {
            _instance = null;
            DomainReloadHash = DateTime.Now.Second;
        }

        public void Start()
        {
            
        }
        
    }
}