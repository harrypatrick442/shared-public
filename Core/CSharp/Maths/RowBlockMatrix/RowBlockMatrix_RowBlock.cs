namespace Core.Maths
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Drawing2D;
    using System.IO;

    namespace Core.Maths
    {
        public class RowBlockMatrix_RowBlock
        {
            public int NRowBlock { get; }
            public int NRows { get; }
            public int NColumns { get; }
            private bool _Loaded;
            public bool Loaded { get { return _Loaded; } }
            public bool RequiresSaveOnUnload { get; set; }
            public LinkedListNode<RowBlockMatrix_RowBlock> LinkedListNode { get; set; }
            private string _FilePath;
            public RowBlockMatrix_RowBlock(int nRowBlock, int nRows, int nColumns, string filePath)
            {
                NRowBlock = nRowBlock;
                NRows = nRows;
                NColumns = nColumns;
                _FilePath = filePath;
                _Loaded = false;
                RequiresSaveOnUnload = false;
            }
            private double[][] _Data;
            public double[][] Data
            {
                get
                {
                    if (Data == null) throw new Exception("Not loaded");
                    return _Data;
                }
            }
            public void Unload()
            {
                if (RequiresSaveOnUnload) {
                    SaveToDisk();
                }
                _Data = null;
                _Loaded = false;
            }
            public void SaveToDisk()
            {
                if (Data == null)
                {
                    throw new Exception("Was not loaded");
                }
                using (FileStream fs = new FileStream(_FilePath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        for (int i = 0; i < NRows; i++)
                        {
                            for (int j = 0; j < NColumns; j++)
                            {
                                double value = _Data[NRows][NColumns];
                                writer.Write(value);
                            }
                        }
                    }
                }
                RequiresSaveOnUnload = false;
            }
            public void Load()
            {
                bool newData = _Data == null;
                if (newData)
                    _Data = new double[NRows][];
                using (FileStream fs = new FileStream(_FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        for (int i = 0; i < NRows; i++)
                        {
                            double[] row;
                            if (newData)
                            {
                                row = new double[NColumns];
                                _Data[i] = row;
                            }
                            else
                            {
                                row = _Data[i];
                            }
                            for (int j = 0; j < NColumns; j++)
                            {
                                row[j] = reader.ReadDouble();
                            }
                        }
                    }
                }
                _Loaded = true;
            }
            public void Dispose()
            {
                _Data = null;
                _Loaded = false;
                File.Delete(_FilePath);
            }
        }
    }

}
