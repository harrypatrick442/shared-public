namespace Core.Layout
{
    public abstract class GridOrganiserBase
    {
        protected int _NColumns;
        protected int _NRows;
        public GridOrganiserBase(int nColumns, int nRows)
        {
            _NColumns = nColumns;
            _NRows = nRows;
        }
        public abstract CellsLocation[] Organise(IGridOrganisable[] gridOrganisables, out IGridOrganisable[] gridOrganisablesThatDidntFit);
    }
}