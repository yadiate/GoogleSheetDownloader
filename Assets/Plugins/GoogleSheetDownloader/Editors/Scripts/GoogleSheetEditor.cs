using Cysharp.Threading.Tasks;
using RIPinc.GoogleSheet.ClassGenerator;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RIPinc.GoogleSheet
{
    [CustomEditor(typeof(GoogleSheet)),CanEditMultipleObjects]
    public class GoogleSheetEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();
            if(GUILayout.Button("다운로드"))
                Editor_DownLoad();
            if(GUILayout.Button("시트 열기"))
                Editor_OpenSheetURL();
            
            var googleSheet = target as GoogleSheet;
            var sheetCreateName =  !googleSheet.ClassGeneratorSetting.IsStructMode ? "클래스 생성" : "구조체 생성";
            if(GUILayout.Button(sheetCreateName))
                GenerateClassSheetKey();
            
            DrawDefaultInspector();
            GUILayout.EndVertical();
        }


        public void Editor_DownLoad()
        {
            DownLoad();
        }
        
        public virtual async UniTask DownLoad()
        {
            if(target is not GoogleSheet googleSheet) return;
            
            var sheet = await GoogleSheetDownloader.DownLoad(GetURL(googleSheet.Url), name, googleSheet);
            googleSheet.SetData(sheet);
            
            EditorUtility.SetDirty(target);
            
            OnDownLoadComplete(googleSheet);
        }
        
        protected void OnDownLoadComplete(GoogleSheet sheet)
        {
            
        }
        public static string GetURL(string code) => $"https://docs.google.com/spreadsheets/d/{code}";
        public void Editor_OpenSheetURL()
        {   
            if(target is GoogleSheet sheet)
                Application.OpenURL(GetURL(sheet.Url));
        }
        
        // private string GenerateClassButtonLabelName() => !IsStructMode ? "클래스 생성" : "구조체 생성";
        //
        //
        // [Button("$GenerateClassButtonLabelName",ButtonSizes.Large)]
        public void GenerateClassSheetKey()
        {
            PlayableSheetGenerator.Generate(target as GoogleSheet);
        }
        

    }
}