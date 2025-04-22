namespace RIPinc.GoogleSheet
{
    public abstract class PlayableSheetData :IPlayableSheetData
    {
        private string _keyName;
        private float _probability;
        public string GetKeyName() => _keyName;
        public void SetKeyName(string keyName) => _keyName = keyName;
        public float GetProbability() => _probability;
        public void SetProbability(float probability) => _probability = probability;
        public abstract void  InGameSetup(TableData sheetData, int column);
        public virtual void LateSetup(){}
        
        protected static readonly string[] _boolTrue = {"true","1"};
        public static bool BoolParse(string value)
        {
            value = value.ToLower();
            foreach (var s in _boolTrue)
            {
                if (s.Equals(value))
                    return true;
            }

            return false;
        }
    }
    
    public interface IPlayableSheetData
    {
        string GetKeyName();
        void SetKeyName(string keyName);
        float GetProbability();
        void SetProbability(float probability);
        
        void InGameSetup(TableData sheetData, int column);
        void LateSetup(){}
        public static readonly string[] _boolTrue = { "true", "1" };
        public static bool BoolParse(string value)
        {
            value = value.ToLower();
            foreach (var s in _boolTrue)
            {
                if (s.Equals(value))
                    return true;
            }

            return false;
        }
        public static int IntParse(string value)
        {
            int.TryParse(value, out int result);
            return result;
        }
    }

}