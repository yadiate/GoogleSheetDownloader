using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RIPinc.GoogleSheet
{
    public static class GoogleSheetUrl
    {
        public static async UniTask<List<(string gid, string title)>> GetSheetNames(string url)
        {

            var replace0 = "굶굸";
            var replace1 = "엷엶";

            var htmlData = await GetWebTextDataAsync(url);
            var data = htmlData.Replace("[", "ㅎ").Replace("\\", "ㄹ").Replace("\"", "ㅛ")
                               .Replace("<div class=ㅛgoog-inline-block docs-sheet-tab-captionㅛ>", replace0).Replace("</div>", replace1);


            //Regex 정규식을 이용하여 데이터를 추출합니다.
            var find       = true;
            var index      = 0;
            var gridNames  = new List<string>();
            var titleNames = new List<string>();
            var timeCheck  = DateTime.Now;
            
            while (find && timeCheck.AddSeconds(5) > DateTime.Now)
            {
                var pattern = $"ㅛㅎ{index},0," + @"ㄹㅛ(\d+)ㄹㅛ";
                var match   = Regex.Match(data, pattern);

                if (match.Success)
                {
                    gridNames.Add(match.Groups[1].Value);
                }
                else
                    find = false;

                if(timeCheck.AddSeconds(5) < DateTime.Now)
                {
                    Debug.LogError("시트 목록을 가져오는데 시간이 너무 오래 걸립니다.");
                    break;
                }
                index++;
            }

            //Regex 정규식을 이용하여 데이터를 추출합니다.
            find = true;
            string titlePattern = replace0 + "(\\w+)" + replace1;
            while (find)
            {
                Match titleMatch = Regex.Match(data, titlePattern);
                
                
                if (titleMatch.Success)
                {
                    // if (data.Contains(":"))
                    // {
                    //     data = data.Replace(":", "_");
                    //     Debug.Assert(false," : 가 포함된 시트 이름은 지원하지 않습니다.");
                    // }
                    titleNames.Add(titleMatch.Groups[1].Value);
                    data = data.Remove(0, titleMatch.Index + titleMatch.Length);
                }
                else
                    find = false;

            }


            var returnData = new List<(string, string)>();
            for (var i = 0; i < gridNames.Count; i++)
            {
                returnData.Add((gridNames[i], titleNames[i]));
            }

            return returnData;
        }
        
        public static async UniTask<string> GetWebTextDataAsync(string url)
        {
            try
            {
                var www = UnityWebRequest.Get(url);
                await www.SendWebRequest();
                return www.downloadHandler.text;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {url}");
                Debug.LogError("웹 리퀘스트 실패, URL이 틀렸다거나 권한이 없다거나");
                Debug.Break();
                return string.Empty;
            }
        }
        
        public static string GetUrl(string googleSheetsUrl1, string sheetGrid)
        {
            return $"{googleSheetsUrl1}/export?format=csv&gid={sheetGrid}";
        }

    }
}