namespace RIPinc.GoogleSheet
{
    public interface ITableAddress
    {
        string TableName();
        string SheetName();
        void InGameSetup(GoogleSheetManager sheetManager);
        void OnDestroy();
    }
}