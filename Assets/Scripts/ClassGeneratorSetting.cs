
using System;
using UnityEngine;

namespace RIPinc.GoogleSheet
{
    [Serializable]
    public struct ClassGeneratorSetting
    {
        // [Title("Class Generator")] 
        // [LabelText("기본 네임 스페이스")]
        public string ClassNamespace;
        
        // [LabelText("클래스 생성 경로")][FolderPath]
        public string ClassSavePath;
        
        // [LabelText("클래스명 접두사")]
        public string ClassPrefix;
        
        // [LabelText("데이터 [Serializable] 붙이기")] 
        public bool IsSerializable;
        // [LabelText("구조체로 생성")]               
        public bool IsStructMode;
        
        // [LabelText("클래스 Partial")]
        public bool IsPartialClass;
        
        //
        // [InfoBox("Enum 코드에 Temp 글자가 포함되어 있으면 제거합니다")]
        // [LabelText("Enum Temp 제거")] 
        public bool IsEnumTempSeparation;
    }
}