namespace EEMathLib.DTO
{
    public class MxDTO
    {
        public const string ROW_ENTRIES = "ROW";
        public const string COLUMN_ENTRIES = "COL";

        public string ID { get; set; }
        public int RowSize { get; set; }
        public int ColumnSize { get; set; }
        public double[] Entries { get; set; }
        public string EntriesType { get; set; } = MxDTO.ROW_ENTRIES; // ROW_ENTRIES or COLUMN_ENTRIES
        public MxDTO[] Matrices { get; set; }
    }
}
