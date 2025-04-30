using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GoogleSheet.Playable
{
    [CreateAssetMenu(fileName = "LabelToLabelSetting", menuName = "RIPinc/GoogleSheet/LabelToLabelSetting")]
    public class LabelToLabelSetting : ScriptableObject
    {
        private Dictionary<string, string> _labelToLabel;

        public Dictionary<string, string> LabelToLabel
        {
            get
            {
                _labelToLabel ??= new Dictionary<string, string>();
                if(_labelToLabel.Count != LabelToLabels.Count)
                {
                    _labelToLabel.Clear();
                    foreach (var labelToLabel in LabelToLabels)
                        _labelToLabel.Add(labelToLabel.Label, labelToLabel.ReplaceLabel);
                }

                return _labelToLabel;
            }
        }

        [SerializeField]
        public List<LabelToLabel> LabelToLabels;

        public void SetLabelToLabel(Dictionary<string, string> labelToLabel)
        {
            LabelToLabels ??= new List<LabelToLabel>();
            foreach (var keyValuePair in labelToLabel)
            {
                LabelToLabels.Add(new LabelToLabel()
                {
                    Label = keyValuePair.Key,
                    ReplaceLabel = keyValuePair.Value
                });
            }
        }
        
        public string ReplaceLabel(string data)
        {
            var keys = LabelToLabel.Keys.ToList();
            keys.Sort((a, b) => b.Length.CompareTo(a.Length));
            
            for (var i = 0; i < keys.Count; i++)
                data = data.Replace(keys[i], LabelToLabel[keys[i]]);
            
            return data;
        }
    }
    
    [Serializable]
    public class LabelToLabel
    {
        [SerializeField]
        public string Label;
        
        [SerializeField]
        public string ReplaceLabel;
    }
}
