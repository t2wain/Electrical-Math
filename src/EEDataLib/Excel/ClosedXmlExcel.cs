using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using EEDataLib.Excel.Common;
using System;
using System.Data;
using System.Linq;

namespace EEDataLib.Excel.ClosedXml
{
    public class ClosedXmlFactory : WorkbookFactory
    {
        public override IExWorkbook CreateWorkbook(string fileName, string templateFile = "")
        {
            XLWorkbook wb;
            if (!string.IsNullOrEmpty(fileName))
                wb = XLWorkbook.OpenFromTemplate(fileName);
            else wb = new XLWorkbook();
            wb.SaveAs(fileName);
            return new ClosedXmlWorkbook(wb);
        }

        public override IExWorkbook OpenWorkbook(string fileName) 
        {
            var wb = new XLWorkbook(fileName);
            return new ClosedXmlWorkbook(wb);
        }
    }

    class ClosedXmlWorkbook : IExWorkbook
    {
        IXLWorkbook _wb = null;

        internal ClosedXmlWorkbook(IXLWorkbook wb)
        {
            _wb = wb;
        }

        public IExWorksheet AddWorksheet(string sheetName, string beforeSN = "", string afterSN = "")
        {
            throw new NotImplementedException();
        }

        public IExWorksheet CopyWorksheet(string sheetName, string templateSN, string beforeSN = "", string afterSN = "")
        {
            throw new NotImplementedException();
        }

        public IExWorksheet GetWorksheet(string sheetName)
        {
            var ws = _wb.Worksheets.Worksheet(sheetName);
            return new ClosedXmlWS(ws);
        }

        public void MoveSheet(string sheetName, string beforeSN = "", string afterSN = "")
        {
            throw new NotImplementedException();
        }

        public void RemoveWorksheet(string sheetName)
        {
            throw new NotImplementedException();
        }

        public void Save() =>
            _wb.Save();

        public void Dispose()
        {
            if (_wb != null) 
                _wb.Dispose();
            _wb = null;
        }
    }

    class ClosedXmlWS : IExWorksheet
    {
        private IXLWorksheet _sheet;

        public ClosedXmlWS(IXLWorksheet sheet)
        {
            _sheet = sheet;
        }

        internal IXLWorksheet Worksheet => _sheet;

        public string Name
        {
            get => _sheet.Name;
            set => _sheet.Name = value;
        }

        public IExRange GetRange(string rngAddr, bool isNameRange = false)
        {
            var rng = _sheet.GetRange(rngAddr);
            if (rng != null)
                return new ClosedXmlRange(rng);
            else return null;
        }

        public void SetValue(string rngAddr, object value, bool isNameRange = false) =>
            _sheet.GetRange(rngAddr, isNameRange).FirstCell().SetValue(value);

        public void DeleteUnUseRows(IExRange bottomUsed)
        {
            //var lstRow = _sheet.Dimension?.End?.Row;
            //if (lstRow != null && bottomUsed is EpplusRange rng)
            //{
            //    var lstUsedRow = new ExcelCellAddress(rng.Range.Address).Row;
            //    _sheet.DeleteRow(lstUsedRow + 1, lstRow.Value - lstUsedRow);
            //}
        }
        
        public IExTable GetTable(string tableName)
        {
            try
            {
                var tbl = _sheet.Tables.Table(tableName);
                return new ClosedXmlTable(tbl);
            }
            catch { }
            return null;
        }

        public void Dispose() =>
            _sheet = null;

    }

    class ClosedXmlRange : IExRange
    {
        IXLRange _range;

        public ClosedXmlRange(IXLRange range)
        {
            _range = range;
        }

        internal IXLRange Range => _range;

        public object Value
        {
            get => _range.FirstCell().GetValue();
            set => _range.FirstCell().SetValue(value);
        }

        public void SetValue(int rowOffset, int colOffset, object value) =>
            _range.FirstCell().Offset(rowOffset, colOffset).SetValue(value);

        public void SetFormat(CellFormat format)
        {
            throw new NotImplementedException();
        }

        public void SetFormat(int rowOffset, int colOffset, CellFormat format)
        {
            throw new NotImplementedException();
        }

        public IExRange Offset(int row, int col) =>
            new ClosedXmlRange(_range.FirstCell().Offset(row, col).AsRange());

        public IExRange Offset(int row, int col, int numOfRows, int numOfCols) =>
            new ClosedXmlRange(_range.FirstCell().Offset(row, col, numOfRows, numOfCols));

        public void Copy(IExRange rangeDest)
        {
            var rng = rangeDest as ClosedXmlRange;
            _range.CopyTo(rng.Range);
        }

        public void Dispose() => _range = null;
    }

    class ClosedXmlTable : IExTable
    {
        IXLTable _tbl;

        public ClosedXmlTable(IXLTable tbl)
        {
            _tbl = tbl;
        }

        public DataTable ToDataTable() =>
            _tbl.AsNativeDataTable();

        public void Dispose()
        {
            _tbl = null;
        }
    }


    static class ClosedXmlExtensions
    {
        #region IXLWorksheet

        public static IXLRange GetRange(this IXLWorksheet sheet, string rngAddr, bool isNameRange = false)
        {
            try
            {
                if (isNameRange && sheet.GetDefinedName(rngAddr).Ranges.FirstOrDefault() is IXLRange t)
                    return t;
                else return sheet.Range(rngAddr);
            }
            catch { }
            return null;
        }

        public static IXLDefinedName GetDefinedName(this IXLWorksheet sheet, string name)
        {
            var dn = sheet.Workbook.DefinedNames
                .Where(n => n.Name == name)
                .FirstOrDefault();

            if (dn == null)
                dn = sheet.DefinedNames
                    .Where(n => n.Name == name)
                    .FirstOrDefault();

            return dn;
        }

        #endregion

        #region IXLCell

        public static void SetValue(this IXLCell cell, object value) => cell.SetValue(Parse(value));

        public static object GetValue(this IXLCell cell) => Parse(cell.Value);

        public static IXLCell Offset(this IXLCell cell, int rowOffset, int colOffset)
        {
            var c = cell;
            c = Math.Sign(rowOffset) == 1 ? c.CellBelow(rowOffset) : c.CellAbove(-rowOffset);
            c = Math.Sign(colOffset) == 1 ? c.CellRight(colOffset) : c.CellLeft(-colOffset);
            return c;
        }

        public static IXLRange Offset(this IXLCell cell, int row, int col, int numOfRows, int numOfCols)
        {
            var c1 = cell.Offset(row, col);
            var c2 = c1.Offset(numOfRows, numOfCols);
            var rng = cell.AsRange().Range(c1.Address, c2.Address);
            return rng;
        }

        static XLCellValue Parse(object value)
        {
            XLCellValue cv = Blank.Value;
            switch (value)
            {
                case string v:
                    cv = v;
                    break;
                case int v:
                    cv = v;
                    break;
                case double v:
                    cv = v;
                    break;
            }
            return cv;
        }

        static object Parse(XLCellValue value)
        {
            object v = null;
            switch (value.Type)
            {
                case XLDataType.Text:
                    v = value.GetText();
                    break;
                case XLDataType.Number:
                    v = value.GetNumber();
                    break;
                case XLDataType.Boolean:
                    v = value.GetBoolean();
                    break;
            }
            return v;
        }

        #endregion
    }
}