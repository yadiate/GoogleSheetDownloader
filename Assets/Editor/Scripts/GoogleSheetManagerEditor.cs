using System.Linq;
using Cysharp.Threading.Tasks;
using RIPinc.GoogleSheet.ClassGenerator;
using UnityEditor;

namespace RIPinc.GoogleSheet
{
    [CustomEditor(typeof(GoogleSheetManager))]
    public class GoogleSheetManagerEditor : Editor
    {
        // [Button]
        // public async UniTask Editor_DownloadAll()
        // {
        //     if (target is not GoogleSheetManager manager) return;
        //
        //     var tasks = Enumerable.Select(manager.Sheets, sheet => sheet.DownLoad()).ToArray();
        //     await UniTask.WhenAll(tasks);
        // }
        //
        // [Button]
        // public async UniTask Editor_CreateClassAll()
        // {
        //     if (target is not GoogleSheetManager manager) return;
        //     
        //     var tasks = Enumerable.Select(manager.Sheets, sheet => PlayableSheetGenerator.Generate(sheet)).ToArray();
        //     await UniTask.WhenAll(tasks);
        // }
    }
}