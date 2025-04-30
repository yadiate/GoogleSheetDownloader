using System;
using System.Collections.Generic;

namespace RIPinc.GoogleSheet
{
    [Serializable]
    public struct SheetData
    {
        // [LabelText("시트 이름 ## 클래스에 시트 이름과 동일해야함")]
        public string      SheetName;
        public string[]    TableNames;
        public TableData[] Tables;
        


        public void Editor_EmptyOrIgnoreRemove()
        {
            var removeIndex = new List<int>();
            for (var i = 0; i < Tables.Length; i++)
            {
                if (Tables[i].RowDataIsEmpty())
                {
                    removeIndex.Add(i);
                }
            }

            if (removeIndex.Count == 0) return;


            removeIndex.Reverse();

            var tableNames = new List<string>(TableNames);
            var sheetDatas = new List<TableData>(Tables);
            foreach (var i in removeIndex)
            {
                tableNames.RemoveAt(i);
                sheetDatas.RemoveAt(i);
            }

            TableNames = tableNames.ToArray();
            Tables = sheetDatas.ToArray();
        }
    }
}
