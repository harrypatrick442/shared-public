namespace Core.Layout
{
    public class CellsLocation<TPayload>:CellsLocation// where TPayload:IGridOrganisable
    {
        public new CellsDimensions<TPayload> CellsDimensions { get { 
                return (CellsDimensions < TPayload >)_CellsDimensions; } }
        public TPayload GridOrganisable { get { return CellsDimensions.GridOrganisable; } }
        public CellsLocation(int columnIndex, int rowIndex, CellsDimensions<TPayload> cellsDimensions):
            base(columnIndex, rowIndex, cellsDimensions)
        {
        }
    }
    public class CellsLocation
    {
        private int _ColumnIndex;
        private int _RowIndex;
        public int ColumnIndex { get { return _ColumnIndex; } }
        public int RowIndex { get { return _RowIndex; } }
        protected CellsDimensions _CellsDimensions;
        public CellsDimensions CellsDimensions { get { return _CellsDimensions; } }
        public CellsLocation(int columnIndex, int rowIndex, CellsDimensions cellsDimensions)
        {
            _ColumnIndex = columnIndex;
            _RowIndex = rowIndex;
            _CellsDimensions = cellsDimensions;
        }
    }
}