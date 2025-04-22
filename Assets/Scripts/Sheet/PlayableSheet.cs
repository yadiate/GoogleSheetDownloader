using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RIPinc.GoogleSheet
{
    public abstract class PlayableSheet<T,TF,E> : ITableAddress where T : PlayableSheet<T,TF,E>, new() where TF : IPlayableSheetData, new() where E : Enum
    {
        private bool _hasLoad;
        private int _domainHash; 
        public static T Instance
        {
            get
            {
                if(!Application.isPlaying)
                    Debug.LogError("에디터에선 접근 하지 마세요 'Editor_Instance' 사용");
                
                if (_instance == null || _instance._domainHash != GoogleSheetManager.DomainReloadHash)
                {
                    _instance = Activator.CreateInstance<T>();
                    _instance._domainHash = GoogleSheetManager.DomainReloadHash;
                    _instance.InGameSetup(GoogleSheetManager.Instance);
                    _instance.LateSetup();
                }
                
                return _instance;
            }
        }


        public static T Editor_Instance
        {
            get
            {
                var instance = Activator.CreateInstance<T>();
                instance.InGameSetup(GoogleSheetManager.Editor_Instance);
                instance.LateSetup();
                return instance;
            }
        }
        
        /// <summary>
        /// 왠만하면 절대 쓰지 마세요
        /// </summary>
        public static T EditorOrInGameAutoInstance => Application.isPlaying ? Instance : Editor_Instance;

        private static T _instance;
        
        
        private TableData _tableData;

        protected TF[] Data { get; private set; }
        public int Count => Data.Length;
        public abstract string TableName();
        public abstract string SheetName();


        public void InGameSetup(GoogleSheetManager sheetManager)
        {
            var googleSheet = sheetManager.Sheets.Find(x => x.Data.SheetName == SheetName());
            _tableData = Array.Find(googleSheet.Data.Tables,x => x.TableName == TableName());
            Data       = new TF[_tableData.YKeys.Length];
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = Activator.CreateInstance<TF>();
                Data[i].SetKeyName(_tableData.YKeys[i]);
                Data[i].InGameSetup(_tableData,i);
            }
        }

        /// <summary>
        /// partial class 커스텀 쪽에서 사용하려고 만듬
        /// </summary>
        public virtual void LateSetup()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i].LateSetup();
            }
        }



        public TF GetLevel(int y)
        {
            return Data[y];
        }

        public virtual TF GetElement(E mode) => Data[Convert.ToInt32(mode)];

        public TF[] GetDatas() => Data.ToArray();
        
        public TF GetRandom(int min = 0, int max =-1)
        {
            if (max == -1) max = Data.Length;
            return Data[UnityEngine.Random.Range(min, max)];
        }
        
        public TF GetProbability()
        {
            var total = 0f;
            foreach (var data in Data)
            {
                total += data.GetProbability();
            }

            var random = UnityEngine.Random.Range(0, total);
            var sum = 0f;
            for (var i = 0; i < Data.Length; i++)
            {
                sum += Data[i].GetProbability();
                if (random < sum)
                    return Data[i];
            }

            return Data[^1];
        }
        
        public void OnDestroy()
        {
            _instance = null;
        }
    }
}