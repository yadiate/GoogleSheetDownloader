using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using GoogleSheet.Playable;

namespace RIPinc.GoogleSheet
{
    public class GoogleSheetDownloader
    {
        
        public static async UniTask<SheetData> DownLoad(string url,string sheetName, GoogleSheet googleSheet)
        {
            var proressBarName = "시트 다운로드";
            EditorUtility.DisplayProgressBar(proressBarName, "시트 다운로드중", 0);

            var sheet = new SheetData();
            List<(string gid, string title)> tableNames = new();
            try
            {

                UniTask.SwitchToThreadPool();
                EditorUtility.DisplayProgressBar(proressBarName, "시트 목록 다운로드중", 0);
                tableNames = await GoogleSheetUrl.GetSheetNames(url);
                if(tableNames == null || tableNames.Count == 0)
                {
                    Debug.LogError("시트 목록을 가져오는데 실패했습니다.");
                    return sheet;
                }
                Debug.Log($"시트 목록 다운로드 완료 {tableNames.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.StackTrace}");
                Debug.LogError("시트 목록을 가져오는데 실패했습니다.");
                return sheet;
            }

            try
            {
                sheet.SheetName  = sheetName;
                sheet.TableNames = new string[tableNames.Count];
                sheet.Tables     = new TableData[tableNames.Count];
              
                var loadCount = tableNames.Count;
                var loadIndex = 0;
                
                var hasLabelExportSetting = googleSheet.LabelToLabelExportSetting;
                if (hasLabelExportSetting)
                {
                    googleSheet.LabelToLabelExportSetting.LabelToLabels = new();
                }
                
                var downloadQueue = Enumerable.Select(tableNames, tableName => DownLoadSheet(googleSheet,url, tableName.Item1,tableName.Item2, loadIndex++, loadCount)).ToList();
                foreach (var valueTuple in tableNames)
                {
                    Debug.Log($"<color=green> 시트 다운로드 시작 </color> - {valueTuple.title}");
                }
                
                var awaiter = await UniTask.WhenAll(downloadQueue);
                var index   = 0;
      
             
                foreach (var sheetTableData in awaiter)
                {
                    sheet.TableNames[index] = tableNames[index].title;
                    sheet.Tables[index] = sheetTableData;
                    sheet.Tables[index].TableName = tableNames[index].title;
                    index++;
                }
                
                EditorUtility.DisplayProgressBar(proressBarName, "다운로드 완료", 1);
                Debug.Log($"{sheetName}시트 다운로드 완료");
                UniTask.SwitchToMainThread();
                
                
   
             
                if(hasLabelExportSetting)
                {
                    Dictionary<string, string> labelToLabel = new Dictionary<string, string>();
                    foreach (var sheetTable in sheet.Tables)
                    {
                        var yIndex = 0;
                        foreach (var yKey in sheetTable.YKeys)
                        {
                            var value = sheetTable.GetData(0, yIndex++).Replace(" ", "_");
                            if (!labelToLabel.TryAdd(yKey, value))
                            {
                                Debug.LogError($"중복된 키가 있습니다. {yKey}");
                            }
                        }
                    }
                    
                    googleSheet.LabelToLabelExportSetting.SetLabelToLabel(labelToLabel);
                    EditorUtility.SetDirty(googleSheet.LabelToLabelExportSetting);
                }
         
                EditorUtility.SetDirty(googleSheet);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.StackTrace}");
            }

            await UniTask.WaitForSeconds(0.5f);
            EditorUtility.ClearProgressBar();
            sheet.Editor_EmptyOrIgnoreRemove();
            return sheet;
        }
        

        private static async UniTask<TableData> DownLoadSheet(GoogleSheet googleSheet,string url, string sheetGid, string debugTitleName,int index,int count)
        {
            await UniTask.WaitForSeconds(index * 0.01532f); // 0.01초 간격으로 다운로드 (너무 빠르면 구글에서 차단함
            url = GoogleSheetUrl.GetUrl(url, sheetGid);
            string data = await GoogleSheetUrl.GetWebTextDataAsync(url);
            if (data.ToLower().Contains("#ignore")) return new();
            if(data.ToLower().Contains("ignore")) return new();
            var csvData = new GenerateTableData();
            
            
            try
            {
                if(googleSheet.LabelToLabelSetting )//&& !googleSheet.IgnoreSheetNames.Contains(debugTitleName))
                {
                    data = googleSheet.LabelToLabelSetting.ReplaceLabel(data);
                }
                csvData.CsvData = data;
                
                var downLoadData = GenerateTable.GenerateTableData(csvData,googleSheet);    
                EditorUtility.DisplayProgressBar("시트 다운로드", $"시트 다운로드중 {index}/{count}", (float)index / count);
                return downLoadData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error SheetName: {debugTitleName} \n {e.StackTrace}");
                EditorUtility.ClearProgressBar();
                return new();
            }
       
            return new();
        }
        
        [MenuItem("RipTool/이벤트/시트 이벤트 다운로드")]
        private static async UniTask Download()
        {
            var googleSheet =
                AssetDatabase.LoadAssetAtPath<GoogleSheet>(
                    "Assets/RIPinc/EventSystem/SheetData/EventTableData.asset");

            var sheet = await DownLoad(GoogleSheetEditor.GetURL(googleSheet.Url),
                googleSheet.name, googleSheet);
            googleSheet.SetData(sheet);

            EditorUtility.SetDirty(googleSheet);
        }
    }
}
