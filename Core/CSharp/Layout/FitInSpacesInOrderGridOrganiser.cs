using System.Linq;
using System.Collections.Generic;

namespace Core.Layout
{
    public class FitInSpacesInOrderGridOrganiser : GridOrganiserBase
    {
        public FitInSpacesInOrderGridOrganiser(int nColumns, int nRows) : base(nColumns, nRows)
        {

        }

        public override CellsLocation[] Organise(IGridOrganisable[] gridOrganisables, out IGridOrganisable[] gridOrganisablesThatDidntFit)
        {
            bool[,] takenGrid = new bool[_NColumns, _NRows];
            List<CellsLocation> cellLocationsForPositionedGridOrganisables = new List<CellsLocation>();
            int nGridOrganisables = gridOrganisables.Length;
            int indexGridOrganisable = 0;
            while (indexGridOrganisable < nGridOrganisables)
            {
                bool positionedGridOrganisable = false;
                IGridOrganisable gridOrganisable = gridOrganisables[indexGridOrganisable];
                CellsDimensions[] possibleCellsDimensionss = gridOrganisable.PossibleCellsDimensionss;
                foreach (CellsDimensions possibleCellsDimensions in possibleCellsDimensionss)
                {
                    if (positionedGridOrganisable)
                        break;
                    for (int rowIndex = 0; rowIndex < _NRows; rowIndex++)
                    {
                        if (positionedGridOrganisable)
                            break;
                        for (int columnIndex = 0; columnIndex < _NColumns; columnIndex++)
                        {
                            if (positionedGridOrganisable)
                                break;
                            bool cellsDimensionsFitGridWithThisStartPosition = DoesCellsDimensionsFItGridWithStartPosition(takenGrid,
                                startRow: rowIndex, startColumn: columnIndex, cellsDimensions: possibleCellsDimensions);
                            if (!cellsDimensionsFitGridWithThisStartPosition)
                                continue;
                            MarkTakenGridOccupiedFOrCellsDimensions(takenGrid, rowIndexTopLeft: rowIndex,
                                columnIndexTopLeft: columnIndex, cellsDimensions: possibleCellsDimensions);
                            cellLocationsForPositionedGridOrganisables.Add(
                                new CellsLocation(
                                    cellsDimensions: possibleCellsDimensions,
                                    rowIndex: rowIndex,
                                    columnIndex: columnIndex
                                )
                            );
                            positionedGridOrganisable = true;
                        }
                    }
                }
                if (!positionedGridOrganisable)
                    break;
            }
            indexGridOrganisable += 1;
            gridOrganisablesThatDidntFit = gridOrganisables.Skip(indexGridOrganisable).ToArray();
            return cellLocationsForPositionedGridOrganisables.ToArray();
        }

        private void MarkTakenGridOccupiedFOrCellsDimensions(bool[,] takenGrid, CellsDimensions cellsDimensions, int columnIndexTopLeft, int rowIndexTopLeft)
        {
            for (int rowIndex = rowIndexTopLeft; rowIndex < rowIndexTopLeft + cellsDimensions.NRows; rowIndex++)
            {
                for (int columnIndex = columnIndexTopLeft; columnIndex < cellsDimensions.NColumns; columnIndex++)
                {
                    takenGrid[columnIndex, rowIndex] = true;
                }
            }
        }

        private bool DoesCellsDimensionsFItGridWithStartPosition(bool[,] takenGrid, int startColumn, int startRow, CellsDimensions cellsDimensions)
        {
            for (int rowIndex = startRow; rowIndex < startRow + cellsDimensions.NRows; rowIndex++)
            {
                if (rowIndex >= _NRows)
                    return false;
                for (int columnIndex = startColumn; columnIndex < startColumn + cellsDimensions.NColumns; columnIndex++)
                {
                    if (columnIndex >= _NColumns)
                        return false;
                    if (takenGrid[columnIndex, rowIndex])
                        return false;
                }
            }
            return true;
        }
    }
}