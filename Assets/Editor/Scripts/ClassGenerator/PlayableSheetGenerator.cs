using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using GoogleSheet.Playable;
using UnityEditor;
using UnityEngine;

namespace RIPinc.GoogleSheet.ClassGenerator
{
    public static class PlayableSheetGenerator
    {
        public static async UniTask Generate(GoogleSheet sheetData, LabelToLabelSetting setting = null)
        {
            var name = sheetData.name;
            var data = sheetData.Data;

            var generator = sheetData.Data.Tables.Select(i => new PlayableSheetGeneratorData(i, sheetData).Create()).ToList();
            await UniTask.WhenAll(generator);
            Debug.Log("생성 완료");
        }
    }
    
    public class PlayableSheetGeneratorData
    {
        private TableData             _tableData;
        private string                _sheetName;
        private string                _className;
        private string                _nameSpace;
        private string                _createFilePath;
        private ClassGeneratorSetting _setting;
        
        public PlayableSheetGeneratorData(TableData tableData, GoogleSheet sheetData)
        {
            _sheetName       = sheetData.name;
            _tableData       = tableData;
            _setting = sheetData.ClassGeneratorSetting;
            _className       = tableData.TableName;
            
            _setting = sheetData.ClassGeneratorSetting;
            
            if (!string.IsNullOrEmpty(tableData.ClassName))
                _className = tableData.ClassName;
            _className = sheetData.ClassGeneratorSetting.ClassPrefix + _className;
            

            if (!string.IsNullOrEmpty(tableData.NameSpace))
                _nameSpace = tableData.NameSpace;
            else
                _nameSpace = sheetData.ClassGeneratorSetting.ClassNamespace;
            
            if(string.IsNullOrEmpty(_nameSpace))
                _nameSpace = _sheetName + "." + _className;
            
            _nameSpace      = _nameSpace.Replace("Scripts.", "");
            _createFilePath = _setting.ClassSavePath + "/" + _className + ".cs";
            
        }
        
        public async UniTask Create()
        {
            Directory.CreateDirectory(_setting.ClassSavePath);
            
            if (File.Exists(_createFilePath))
                File.Delete(_createFilePath);
            
            Steam       = new(_createFilePath);
            BracesCount = 0;

            Add("using System;");
            Add("using RIPinc.GoogleSheet;");
            if(_setting.IsStructMode)
            {
                Add("using Unity.Collections;");
#if ODIN_INSPECTOR
                Add("using Sirenix.OdinInspector;");
                Add("using UnityEngine;");
#endif
            }
            
            Add("using Range = RIPinc.GoogleSheet.Range;");
            Space();
            Add($"namespace {_nameSpace}");
            S();
            {
                Space();

                var dataClassName = $"{_className}Data";
                var modeName      = $"{_className}Mode";
                var partial = _setting.IsPartialClass ? "partial " : "";
                
                Add($"public {partial}class {_className} : PlayableSheet<{_className},{dataClassName},{modeName}>");
                S();
                {
                    Space();

                    Add($"public override string TableName() =>  \"{_tableData.TableName}\";");
                    Add($"public override string SheetName() =>  \"{_sheetName}\";");

                    Space();
                    Space();
                }
                E();

                Add($"public enum {modeName}");
                S();
                {
                    var enumKeys = _tableData.YKeys.Distinct().ToArray();
                    for (var i = 0; i < enumKeys.Length; i++)
                    {
                        var isLast = i == enumKeys.Length - 1;
                        if(!_setting.IsEnumTempSeparation)
                            Add($"{enumKeys[i]}{(isLast ? "" : ",")}");
                        else if(!enumKeys[i].Contains("Temp"))
                            Add($"{enumKeys[i]} = {i}{(isLast ? "" : ",")}");
                    }
                }
                E();

                if(_setting.IsSerializable)
                    Add($"[Serializable]");
                
                if(!_setting.IsStructMode)
                    Add($"public {partial}class {dataClassName} : PlayableSheetData");
                else
                    Add($"public struct {dataClassName} : IPlayableSheetData");
                
                S();
                {
                    
                    if (_setting.IsStructMode)
                    {
                        Space();
                        var odin = "";
#if ODIN_INSPECTOR
                        odin = "[HideInInspector] ";
#endif
                        Add(odin +"public FixedString128Bytes  _keyName;");
                        Add(odin +"public float _probability;");
                        Add("public string GetKeyName() => _keyName.ToString();");
                        Add("public void SetKeyName(string keyName) => _keyName = keyName;");
                        Add("public float GetProbability() => _probability;");
                        Add("public void SetProbability(float probability) => _probability = probability;");
                    }
                    
                    Space();

                    FiledEnumCreate(_setting.IsEnumTempSeparation);

                    if(!_setting.IsStructMode)
                        Add($"public override void InGameSetup(TableData tableData, int index)");
                    else
                        Add($"public void InGameSetup(TableData tableData, int index)");
                    S();
                    {
                        for (var i = 0; i < _tableData.XTypes.Length; i++)
                        {
                            var type = _tableData.XTypes[i];
                            var typeName = _tableData.XTypeNames[i];
                            var xKey = _tableData.XKeys[i].Replace(" ", "_").Replace("/","_");
                            if(type == ValueType.String)
                            {
                                if(!_setting.IsStructMode)
                                    Add($"{xKey} = tableData.GetData({i},index);");
                                else
                                    Add($"{xKey} = tableData.GetData({i},index);");
                            }
                            else if(type == ValueType.Bool)
                            {
                                if(!_setting.IsStructMode)
                                    Add($"{xKey} = BoolParse(tableData.GetData({i},index));");
                                else
                                    Add($"{xKey} = IPlayableSheetData.BoolParse(tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.Vector2)
                            {
                                Add($"{xKey} = new Vector2(tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.IfElse)
                            {
                                Add($"{xKey} = {GetTypeToField(type, typeName)}.Parse(tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.Probability)
                            {
                                Add($"{xKey} = {GetTypeToField(type, typeName)}.Parse(tableData.GetData({i},index));");
                                Add("SetProbability(" + xKey + ");");
                            }
                            else if (type == ValueType.Enum)
                            {
                                Add($"{xKey} = ({xKey}Mode)Enum.Parse(typeof({xKey}Mode),tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.EnumFlag)
                            {
                                Add($"{xKey} = ({xKey}Mode)Enum.Parse(typeof({xKey}Mode),tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.Probability)
                            {
                                Add($"{xKey} = {GetTypeToField(type, typeName)}.Parse(tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.RangeMin)
                            {
                                Add($"{xKey.Remove(xKey.Length-1)} = new Range(tableData.GetData({i},index],tableData.GetData({i + 1},index));");
                            }
                            else if (type == ValueType.RangeMax)
                            {
                                
                            }
                            else if (type == ValueType.Int)
                            {
                                Add($"{xKey} = IPlayableSheetData.IntParse(tableData.GetData({i},index));");
                            }
                            else if (type == ValueType.EnumGenenric)
                            {
                                Add($"{xKey} = ({typeName})Enum.Parse(typeof({typeName}),tableData.GetData({i},index));");
                            }
                            else
                            {
                                if (_setting.IsExceptionalHandling)
                                {
                                    Add($"var __{xKey} = tableData.GetData({i},index);");
                                    Add($"{xKey} = __{xKey} != \"\" ? {GetTypeToField(type, typeName)}.Parse(__{xKey}) : default;");
                                }
                                else
                                {
                                    Add($"{xKey} = {GetTypeToField(type, typeName)}.Parse(tableData.GetData({i},index));");
                                }
                                
                            }
                        }
                    }
                    E();
                }
                E();

            }
            E();

            Steam.Close();
            AssetDatabase.Refresh();
        }

        private void FiledEnumCreate(bool tempMode = false)
        {
            var  enumListSaveData = new List<string>[_tableData.XTypes.Length];
            for (var i = 0; i < _tableData.XTypes.Length; i++)
            {
                var type = _tableData.XTypes[i];
                if (type == ValueType.Enum | type == ValueType.EnumFlag)
                {
                    var length = _tableData.GetColumnLength();
                    
                    var xKey   = _tableData.XKeys[i];
                    xKey = xKey.Replace(" ", "_").Replace("/","_");

                    Add($"public {xKey}Mode {xKey};");

                    #region EnumFlagMake

                    var enumList = new List<string>();
                    enumList.Add("None");
                    var isFlag = type == ValueType.EnumFlag;
                    for (var j = 0; j < length; j++)
                    {
                        
                        var enumName = _tableData.GetData(i, j).Split(',');
                        if(enumName.Length > 1)
                        {
                            isFlag = true;
                            foreach (var s in enumName)
                            {
                                if(!enumList.Contains(s))
                                    enumList.Add(s);
                            }
                        }
                        else if(!enumList.Contains(enumName[0]))
                            enumList.Add(_tableData.GetData(i, j).Replace(" ", "_").Replace("/","_"));
                    }
                    enumList            = enumList.ToList();
                    enumList.RemoveAll(string.IsNullOrEmpty);
                    enumListSaveData[i] = enumList;
                    if(isFlag)
                    {
                        _tableData.XTypes[i] = ValueType.EnumFlag;
                    }
                          
                    #endregion

     
                    
                    if(!isFlag)
                    {
                        Add($"public enum {xKey}Mode");
                    }
                    else
                    {
                        Add($"[Flags]");
                        Add($"public enum {xKey}Mode");
                    }
                    S();
                    {
                        if(!isFlag)
                        {
                            var modeKeys = new List<string>();
                            var enumIndex    = 0;
                            for (var j = 0; j < length; j++)
                            {
                                var isLast = j == length - 1;
                                var enumName = _tableData.GetData(i, j).Replace(" ", "_").Replace("/","_");
                                if(!modeKeys.Contains(enumName))
                                {
                                    if(!tempMode)
                                        Add($"{enumName} = {enumIndex}{(isLast ? "" : ",")}");
                                    else if(!enumName.Contains("Temp"))
                                        Add($"{enumName} = {enumIndex}{(isLast ? "" : ",")}");
                                    
                                    modeKeys.Add(enumName);
                                    enumIndex++;
                                }
                            }
                        }
                        else
                        {
                            Add("None = 0,");
                            for (var j = 1; j < enumList.Count; j++)
                            {
                                Add($"{enumList[j]} = 1 << {j},");
                            }
                        }
                    }
                    E();
                    continue;
                }
                        
                if(_tableData.XTypes[i] != ValueType.RangeMax && _tableData.XTypes[i] != ValueType.RangeMin)
                {
                    Add($"public {GetTypeToField(_tableData.XTypes[i], _tableData.XTypeNames[i])} {_tableData.XKeys[i].Replace(" ", "_").Replace("/", "_")};");
                }
                        
                if(_tableData.XTypes[i] == ValueType.RangeMin && i < _tableData.XTypes.Length - 1)
                    Add($"public Range {_tableData.XKeys[i].Remove(_tableData.XKeys[i].Length-1)};");
            }
        }


        private string GetTypeToField(ValueType type, string typeString)
        {
            switch (type)
            {
                case ValueType.String:
                {
                    bool isGeneric = typeString.Contains('<');
                    if (isGeneric)
                    {
                        string t = typeString.Split('<').Last();
                        return t.Remove(t.Length - 1, 1);
                    }
                    else
                    {
                        if (!_setting.IsStructMode)
                            return "string";
                        return "FixedString128Bytes";
                    }
                }
                case ValueType.Int:
                    return "int";
                case ValueType.Float:
                    return "float";
                case ValueType.Bool:
                    return "bool";
                case ValueType.Range:
                    return "Range";
                case ValueType.Vector2:
                    return "Vector2";
                case ValueType.Probability:
                    return "float";
                case ValueType.IfElse:
                    return "float";
                case ValueType.RangeMin:
                    return "Range";
                case ValueType.EnumGenenric:
                    return typeString;
            }

            return "";
        }
        
        private StreamWriter Steam { get; set; }

        public void Space()
        {
            Steam.WriteLine("");
        }
        
        public void Add(string text)
        {
            var contents = "";
            for (int i = 0; i < BracesCount; i++)
                contents += "   ";
            
            contents += text;
            Steam.WriteLine(contents);
        }
        public int BracesCount { get; set; }
        public  void S()
        {
            Add("{");
            BracesCount++;
        }
        public  void E()
        {
            BracesCount--;
            Add("}");
        }
        
        public static string FindCommonSubstrings(string str1, string str2)
        {
            var substrings1 = GetAllSubstrings(str1);
            var substrings2 = GetAllSubstrings(str2);

            substrings1.IntersectWith(substrings2); // 교집합 찾기

            return substrings1.Aggregate("", (current, s) => current + s);
        }

        public static HashSet<string> GetAllSubstrings(string str)
        {
            var substrings = new HashSet<string>();

            for (int i = 0; i < str.Length; i++)
            {
                for (int j = i + 1; j <= str.Length; j++)
                {
                    substrings.Add(str.Substring(i, j - i));
                }
            }

            return substrings;
        }
    }
}