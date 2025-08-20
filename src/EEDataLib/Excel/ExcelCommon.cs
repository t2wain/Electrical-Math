using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;

namespace EEDataLib.Excel.Common
{
    public abstract class WorkbookFactory
    {
        public abstract IExWorkbook CreateWorkbook(string fileName, string templateFile = "");
        public abstract IExWorkbook OpenWorkbook(string fileName);
    }

    public interface IExWorkbook : IDisposable
    {
        IExWorksheet GetWorksheet(string sheetName);
        IExWorksheet AddWorksheet(string sheetName, string beforeSN = "", string afterSN = "");
        IExWorksheet CopyWorksheet(string sheetName, string templateSN, string beforeSN = "", string afterSN = "");
        void RemoveWorksheet(string sheetName);
        void MoveSheet(string sheetName, string beforeSN = "", string afterSN = "");
        void Save();
    }

    public interface IExWorksheet : IDisposable
    {
        string Name { get; set; }
        IExRange GetRange(string rngAddr, bool isNameRange = false);
        void SetValue(string rngAddr, object value, bool isNameRange = false);
        void DeleteUnUseRows(IExRange bottomLeftUsed);
        IExTable GetTable(string tableName);
    }

    public interface IExRange : IDisposable
    {
        object Value { get; set; }
        void SetValue(int rowOffset, int colOffset, object value);
        void SetFormat(CellFormat format);
        void SetFormat(int rowOffset, int colOffset, CellFormat format);
        IExRange Offset(int row, int col);
        IExRange Offset(int row, int col, int numOfRows, int numOfCols);
        void Copy(IExRange rangeDest);
    }

    public interface IExTable : IDisposable
    {
        DataTable ToDataTable();
    }
}
