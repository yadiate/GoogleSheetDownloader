using System;



namespace RIPinc.GoogleSheet
{
    [Serializable]
    public struct TableData
    {
        public string   TableName;
        public string   NameSpace;
        public string   ClassName;
        
        
        public string[]    XKeys;
        public string[]    YKeys;
        public ValueType[] XTypes;
        public string[]    XTypeNames;
            
        private RowData[] Data;
        
        public string GetData(int row, int column)
        {
            return Data[row].Column[column];
        }
        
        public string SetData(int row, int column, string value)
        {
            Data[row].Column[column] = value;
            return Data[row].Column[column];
        }
        
        public void SetDataSize(int row, int column)
        {
            Data = new RowData[row];
            for (var i = 0; i < row; i++)
            {
                Data[i].Column = new string[column];
            }
        }
        
        public int GetRowLength()
        {
            return Data.Length;
        }
        public int GetColumnLength()
        {
            return Data[0].Column.Length;
        }

        public bool RowDataIsEmpty()
        {
            if(Data == null || Data.Length == 0) return true;

            return false;
        }
        
        [Serializable]
        public struct RowData
        {
            public string[] Column;
        }
    }



    public enum ValueType
    {
        String,Int,Float,Bool,Enum,Range,Vector2,Probability,EnumFlag,IfElse,RangeMin,RangeMax
    }
}