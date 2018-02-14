using System;
using System.Collections.Generic;

namespace Lasm.Core.Collections
{
    public sealed class Database
    {
        public List<Cell> cells;

        public int columns { get; private set; }
        public int rows { get; private set; }
        public int depth { get; private set; }

        public Database(int columns, int rows, int depth)
        {
            this.columns = columns;
            this.rows = rows;
            this.depth = depth;
        }

        public class Cell
        {
            public int column;
            public int row;
            public int depth;
            public object value;

            public Cell(int column, int row, int depth, object value)
            {
            }
        }

        public void AddColumns(int columns, int atColumn, object defaultValue)
        {
            bool after = false;

            foreach (Cell cell in cells)
            {
                if (cell.column < columns)
                {
                    cells.Add(new Cell(atColumn, 0, 0, defaultValue));
                }
                else
                {
                    if (cell.column == this.columns)
                    {
                        after = false;
                    }
                    break;
                }
            }

            for (int h = 1; h <= this.depth; h++)
            {
                for (int i = atColumn;  i <= atColumn + columns; i++)
                {
                    for (int j = 1; j <= this.rows; j++)
                    {
                        cells.Add(new Cell(i, j, h, defaultValue));
                    }
                }
            }

            if (after)
            {
                foreach (Cell cell in this.cells)
                {
                    if (cell.column >= atColumn)
                    {
                        cells.Add(new Cell(atColumn + columns, this.rows, this.depth, defaultValue));
                    }
                }
            }
            
            this.columns += columns;

        }

        public void AddRows(int rows, int atRow, object defaultValue)
        {
            bool after = false;

            foreach (Cell cell in cells)
            {
                if (cell.row < rows)
                {
                    cells.Add(new Cell(atRow, 0, 0, defaultValue));
                }
                else
                {
                    if (cell.row == this.rows)
                    {
                        after = false;
                    }
                    break;
                }
            }

            for (int h = 1; h <= this.depth; h++)
            {
                for (int i = atRow; i <= atRow + rows; i++)
                {
                    for (int j = 1; j <= this.columns; j++)
                    {
                        cells.Add(new Cell(j, i, h, defaultValue));
                    }
                }
            }

            if (after)
            {
                foreach (Cell cell in this.cells)
                {
                    if (cell.row >= atRow)
                    {
                        cells.Add(new Cell(atRow + rows, this.rows, this.depth, defaultValue));
                    }
                }
            }
            
            this.rows += rows;
        }

        public void AddDepth(int depth, int atDepth, object defaultValue)
        {
            bool after = false;

            foreach (Cell cell in cells)
            {
                if (cell.depth < depth)
                {
                    cells.Add(new Cell(atDepth, 0, 0, defaultValue));
                }
                else
                {
                    if (cell.depth == this.depth)
                    {
                        after = false;
                    }
                    break;
                }
            }

            for (int h = 1; h <= this.columns; h++)
            {
                for (int i = atDepth; i <= atDepth + depth; i++)
                {
                    for (int j = 1; j <= this.rows; j++)
                    {
                        cells.Add(new Cell(j, i, h, defaultValue));
                    }
                }
            }

            if (after)
            {
                foreach (Cell cell in this.cells)
                {
                    if (cell.depth >= atDepth)
                    {
                        cells.Add(new Cell(atDepth + depth, this.depth, this.depth, defaultValue));
                    }
                }
            }
            
            this.depth += depth;

        }

        public void Clear()
        {
            foreach (Cell cell in cells)
            {
                cell.value = null;
            }
        }

        public bool ContainsValue(object value)
        {
            bool doesContain = false;

            foreach (Cell cell in cells)
            {
                if (cell.value == value)
                {
                    doesContain = true;
                    break;
                }
            }

            return doesContain;
        }

        public bool ContainsCell(int row, int column, int depth)
        {
            bool doesContain = false;

            foreach (Cell cell in cells)
            {
                if (cell.row == row && cell.column == column && cell.depth == depth)
                {
                    doesContain = true;
                    break;
                }
            }

            return doesContain;
        }

        public Cell FindCell(object value)
        {
            Cell valueCell = null;

            foreach (Cell cell in cells)
            {
                if (cell.value == value)
                {
                    valueCell = cell;
                    break;
                }
            }

            return valueCell;
        }

        public List<Cell> FindCells(object value)
        {
            List<Cell> valueCells = null;

            foreach (Cell cell in cells)
            {
                if (cell.value == value)
                {
                    valueCells.Add(cell);
                }
            }

            return valueCells;
        }

        public object GetCell (int column, int row, int depth)
        {
            object valueOut = null;

            foreach (Cell cell in cells){
                if (cell.column == column && cell.row == row && cell.depth == depth)
                {
                    valueOut = cell.value;
                }
            }

            return valueOut;
        }

        public object GetLastCell()
        {
            return cells[cells.Count].value;
        }

        public void PullCells(int columnStartIndex, int rowStartIndex, int depthStartIndex, int columns, int rows, int depth, out Database newDatabase)
        {
            Database newDatabaseTemp = new Database(0, 0, 0);

            foreach (Cell cell in cells)
            {
                if (cell.column >= columnStartIndex && cell.column <= (columnStartIndex + (columns - 1)) &&
                    cell.row >= rowStartIndex && cell.row <= (rowStartIndex + (rows - 1)) &&
                    cell.depth >= depthStartIndex && cell.depth <= (depthStartIndex + (depth - 1)))
                {
                    newDatabaseTemp.cells.Add(cell);
                    cell.value = null;
                }
            }

            newDatabase = newDatabaseTemp;
        }

        public void RemoveColumns (int columnStartIndex, int columns, out Database database)
        {
            Database newDatabase = new Database(0,0,0);

            foreach (Cell cell in cells)
            {
                if (cell.column < columnStartIndex)
                {
                    newDatabase.cells.Add(cell);
                }
                else
                {
                    if (cell.column >= columnStartIndex + columns)
                    {
                        newDatabase.cells.Add(cell);
                    }
                }
            }

           this.cells = newDatabase.cells;
            this.rows = newDatabase.cells[newDatabase.cells.Count].row;
            this.columns = databaseIn.GetValue<Database>().columns - columns.GetValue<int>();
            newDatabase.depth = databaseIn.GetValue<Database>().depth;

            database = this;
        }
    }
}
