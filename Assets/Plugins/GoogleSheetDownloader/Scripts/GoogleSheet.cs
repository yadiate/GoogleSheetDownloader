using GoogleSheet.Playable;
using UnityEngine;

namespace RIPinc.GoogleSheet
{
    [PreferBinarySerialization][CreateAssetMenu(fileName = "GoogleSheet", menuName = "RIPinc/GoogleSheet/GoogleSheet")]
    public class GoogleSheet : ScriptableObject
    {
        // [TitleGroup("구글 시트 URL")]
        [SerializeField]
        private string _url;
        public string Url => _url;
        

        // [SerializeField,PropertyOrder(99)]
        [SerializeField]
        private SheetData _data;

        public SheetData Data => _data;
        
        // [HideLabel]
        [SerializeField]
        private ClassGeneratorSetting _classGeneratorSetting;
        public ClassGeneratorSetting ClassGeneratorSetting => _classGeneratorSetting;

        // [TitleGroup("구글 시트 URL")]
        // [LabelText("Slash 변환 안 함")]
        public bool IsNotReplaceSlash;
        
        
        // [Title("라벨 자동 변경")] 
        // [LabelText("라벨 가져오기")] 
        public LabelToLabelSetting LabelToLabelSetting;
        // [LabelText("라벨 내보내기")] 
        public LabelToLabelSetting LabelToLabelExportSetting;
        
        // [LabelText("Ignore Sheet Name")] public string[] IgnoreSheetNames;
        
        public void SetData(SheetData data)
        {
            _data = data;
            _data.SheetName = name;
        }
    }
}