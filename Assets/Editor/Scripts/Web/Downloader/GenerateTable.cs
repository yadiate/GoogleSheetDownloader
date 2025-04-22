using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RIPinc.GoogleSheet
{
    public struct GenerateTableData
    {
        public string CsvData;
        public bool IsNotReplaceSlash;
    }

    public static class GenerateTable
    {
   
        // CSV 데이터를 파싱하여 TableData 객체로 변환하는 메소드
        public static TableData GenerateTableData(GenerateTableData csvSetting,GoogleSheet googleSheet)
        {
            var sheetData = new TableData();
                
            var csvData = csvSetting.CsvData;
            var yList = csvData.Split('\n'); // CSV 데이터를 줄바꿈 기준으로 분리
            
            var yListCheck = yList.ToList();
            for (var i = yListCheck.Count - 1; i >= 1; i--)
            {
                if(yListCheck[i][0] != '#' && !yListCheck[i - 1].Contains('\r'))
                {
                    yListCheck[i - 1] += yListCheck[i];
                    yListCheck.RemoveAt(i);
                }
            }
            
            yList = yListCheck.ToArray();
            
            var yHeaderIndex = 0; // 헤더의 인덱스, 기본적으로 0부터 시작

                
            // 첫 번째 행이 네임스페이스, 클래스 이름, 리스트 타입을 포함하는 경우 처리
            // if(yList[0].Contains("_NameSpace") || yList[0].Contains("_ClassName"))
            // {
            //     yHeaderIndex = 1; // 헤더의 인덱스를 1로 조정
            //     ySkilCount++; // 스킵할 행의 수 증가
            //     ParseMetaData(yList.First(),ref sheetData); // 메타데이터 파싱 및 저장
            // }

            // for (int i = 0; i < Mathf.Min(5,yList.Length); i++)
            // {
            //     if(!yList[i].Contains("#설명")) continue;
            //     ySkilCount++;                               // 스킵할 행의 수 증가
            // }
            
       
            var typeIndex = yList.ToList().FindIndex(i => i.Split(',').First().ToLower().Contains("type"));
            var nameIndex = yList.ToList().FindIndex(i => i.Split(',').First().ToLower().Contains("name"));
            
            var ySkipList = new List<int>();
            ySkipList.Add(typeIndex);
            ySkipList.Add(nameIndex);
            
            
            var headerColumns = SplitX(yList[nameIndex]).ToList(); // 헤더 열 분리 및 저장
            
            ySkipList.AddRange(yList.Select((i, index) =>ExceptionLineFilter(i) ? index : -1).Where(i => i != -1).ToList());
            ySkipList = ySkipList.Distinct().ToList();
            
            var typeColumns   = SplitX(yList[typeIndex]).ToList(); // 타입 열 분리 및 저장
            
            yList = yList.ToArray(); // 데이터 시작 부분으로 배열 조정
            
            int yCount = yList.Length;        
            int xCount = headerColumns.Count; 

            // 데이터 및 키 배열 초기화
            sheetData.SetDataSize(xCount, yCount);
            sheetData.YKeys  = new string[yCount];
            sheetData.XKeys  = headerColumns.Select(i => ParseNameLabel(i,googleSheet.IsNotReplaceSlash)).ToArray();
            
            var xSkipList = new List<int>();
            xSkipList.Add(headerColumns.FindIndex(x => x.ToLower().Contains("name")));
            xSkipList.AddRange(headerColumns.Select((i, index) => ExceptionLineFilter(i) ? index : -1).Where(i => i != -1).ToList());
            xSkipList = xSkipList.Distinct().ToList();
            
            
            List<(ValueType Type, string TypeName)> typeDatas = typeColumns.Select((t) =>
            {
                if(!string.IsNullOrEmpty(t) && t.Length > 1)
                {
                    t = t.ToLower();
                    t = t.ToUpper()[0] + t.Substring(1);
                }
                else
                {
                    return (ValueType.String,"String");
                }
              

                if (Enum.TryParse<ValueType>(t,true, out var result))
                    return (result, t);
                if (t == "Rangemin")
                {
                   
                    return (ValueType.RangeMin, "RangeMin");
                }

                if (t == "Rangemax")
                {
                    return (ValueType.RangeMax, "RangeMax");
                }
                
                {
                    if (t.Contains(":"))
                    {
                        var split = t.Split(':');
                        if (split.Length == 2)
                        {
                            if (Enum.TryParse<ValueType>(split[0],true, out var result2))
                                return (result2, split[1]);
                        }
                    }

                    if (t.Contains("<"))
                    {//Generic
                        var split = t.Split('<');
                        if (split.Length == 2)
                        {
                            if(split[0] == "Enum")
                                return (ValueType.EnumGenenric, split[1].Substring(0, split[1].Length - 1));
                            else 
                                return (ValueType.String, "String");
                        }
                    }

                    return (ValueType.String, "String");
                }

            }).ToList();



            // 각 행과 열에 대해 데이터 추출 및 저장
            for (int y = 0; y < yList.Length; y++)
            {
                string currentRow = yList[y];
                if (string.IsNullOrWhiteSpace(currentRow)) continue;

                var xData = SplitX(currentRow).ToList();

                sheetData.YKeys[y] = ParseNameLabel(xData[0],googleSheet.IsNotReplaceSlash);          
                xData              = xData.ToList();

                for (int x = 0; x < xCount; x++)
                {
                    string columnValue = x < xData.Count ? xData[x] : string.Empty;
                    sheetData.SetData(x, y, columnValue);
                }
            }
            
            var skipArray = new List<List<string>>();
            for (int x = 0; x < xCount; x++)
            {
                
                if (xSkipList.Contains(x)) continue;
                var skipData = new List<string>();
                for (int y = 0; y < yCount; y++)
                {
                    if (ySkipList.Contains(y)) continue;
                    skipData.Add(sheetData.GetData(x, y));
                }

                skipArray.Add(skipData);
            }
            
            sheetData.YKeys = sheetData.YKeys.Where((i, index) => !ySkipList.Contains(index)).ToArray();
            sheetData.XKeys = sheetData.XKeys.Where((i, index) => !xSkipList.Contains(index)).ToArray();
            sheetData.XTypes = typeDatas.Where((i, index) => !xSkipList.Contains(index)).Select(i => i.Type).ToArray();
            sheetData.XTypeNames = typeDatas.Where((i, index) => !xSkipList.Contains(index)).Select(i => i.TypeName).ToArray();
            yCount = skipArray[0].Count;
            sheetData.SetDataSize(skipArray.Count, yCount);
            for (int x = 0; x < skipArray.Count; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    sheetData.SetData(x, y, skipArray[x][y]);
                    if(sheetData.GetData(x, y) == "")
                        Debug.Log($"<color=yellow> 시트가 비어 있습니다 {sheetData.XKeys[x]} : {sheetData.YKeys[y]} </color>");
                }
            }
            return sheetData;
        }
        /// <summary> 메타데이터(네임스페이스, 클래스 이름, 리스트 타입) 파싱 및 저장 </summary>
        private static void ParseMetaData(string metaDataRow,ref TableData sheetData)
        {
            var metaData = metaDataRow.Replace("\r", "").Split(',').Where(i => !string.IsNullOrEmpty(i)).Select(part => part.Split(':').Select(p => p.Trim()).ToArray()).ToDictionary(parts => parts[0], parts => parts[1]);
            sheetData.NameSpace = metaData.GetValueOrDefault("_NameSpace");
            sheetData.ClassName = metaData.GetValueOrDefault("_ClassName");
        }

        private static string ParseNameLabel(string str, bool isNotReplaceSlash)
        {
            if (string.IsNullOrEmpty(str)) return "null";
             str = str.Replace(" ", "_").Replace("%", "Per").Replace("^", "Pow").Replace("~", "_").Replace("<", "").Replace(":","_");
             if (!isNotReplaceSlash)
                 str = str.Replace("/", "_");
             
             if(int.TryParse(str[0].ToString(), out _)) str = "_" + str;
             return str;
        }
        
        
        // CSV 한 행을 구분자에 따라 분리하는 메소드. 따옴표 안의 콤마는 구분자로 취급하지 않음
        private static List<string> SplitX(string row)
        {
            var rowData = new List<string>();
            bool isInQuotes = false; // 따옴표 안에 있는지 상태를 추적
            string currentField = string.Empty;

            foreach (char ch in row)
            {
                if (ch == '"')
                {
                    isInQuotes = !isInQuotes; // 따옴표 상태 토글
                }
                else if (ch == ',' && !isInQuotes)
                {
                    rowData.Add(currentField); // 필드 추가
                    currentField = string.Empty;
                }
                else if (ch != '\r') // 캐리지 리턴 무시
                {
                    currentField += ch; // 현재 필드에 문자 추가
                }
            }

            // 마지막 필드 추가
            if (!string.IsNullOrEmpty(currentField) || isInQuotes)
            {
                rowData.Add(currentField);
            }

            return rowData;
        }

        public static bool ExceptionLineFilter(string str)
        {
            var findIndex = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (!str[i].Equals('#')) continue;
                findIndex = i;
                break;
            }
            if (findIndex == -1) return false;
            if (findIndex == 0) return true;
            if(str[findIndex-1] != '=') return true;
            return false;
        }
    }
}