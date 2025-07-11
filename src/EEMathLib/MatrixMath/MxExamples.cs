using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;

namespace EEMathLib.MatrixMath
{
    public class MxExamples
    {
        public bool Ex1()
        {
            var m = new MxDTO
            {
                Matrices = new[] {
                    new MxDTO {
                        ID = "A",
                        RowSize = 3,
                        ColumnSize = 3,
                        EntriesType = MxDTO.ROW_ENTRIES,
                        Entries = new double[]
                        {
                            1, -1, -1,
                            3, -3,  2,
                            2, -1,  1
                        }
                    },
                    new MxDTO
                    {
                        ID = "b",
                        RowSize = 3,
                        ColumnSize = 1,
                        EntriesType = MxDTO.COLUMN_ENTRIES,
                        Entries = new double[]
                        {
                            2, 16, 9,
                        }
                    },
                    new MxDTO
                    {
                        ID = "x",
                        RowSize = 3,
                        ColumnSize = 1,
                        EntriesType = MxDTO.COLUMN_ENTRIES,
                        Entries = new double[]
                        {
                            3, -1, 2
                        }
                    }
                }
            };

            var A = MX.ParseMatrix(m.Matrices[0]);

            var rank = A.Rank();  // 3
            var noOfVariables = A.ColumnCount; // 3
            var uniqueSolutions = rank == noOfVariables;

            var b = MX.ParseMatrix(m.Matrices[1]);
            var x = MX.ParseMatrix(m.Matrices[2]);

            // A must be square and of full rank for a unique solution
            var res = A.Solve(b);
            // Round the result to avoid floating point precision issues
            res.MapInplace(entry => Math.Round(entry, 0), Zeros.Include);
            var c = res.Equals(x);
            return c;
        }

        public bool Ex2()
        {
            var m = new MxDTO
            {
                Matrices = new[] {
                    new MxDTO {
                        ID = "A",
                        RowSize = 3,
                        ColumnSize = 3,
                        EntriesType = MxDTO.ROW_ENTRIES,
                        Entries = new double[]
                        {
                             2,  1,  3,
                             4, -1,  3,
                            -2,  5,  5
                        }
                    },
                    new MxDTO
                    {
                        ID = "b",
                        RowSize = 3,
                        ColumnSize = 1,
                        EntriesType = MxDTO.COLUMN_ENTRIES,
                        Entries = new double[]
                        {
                            1, -4, 9
                        }
                    },
                    new MxDTO
                    {
                        ID = "x",
                        RowSize = 3,
                        ColumnSize = 1,
                        EntriesType = MxDTO.COLUMN_ENTRIES,
                        Entries = new double[]
                        {
                            0.5, 3, -1
                        }
                    },
                }
            };

            var A = MX.ParseMatrix(m.Matrices[0]);

            var rank = A.Rank();
            var noOfVariables = A.ColumnCount;
            var noOfFreeVariables = noOfVariables - rank;
            var uniqueSolutions = rank == noOfVariables;

            var lu = A.LU();
            var p = lu.P;
            var l = lu.L;
            var u = lu.U;

            var ivn = A.Inverse();
            var inv2 = lu.Inverse();

            var b = MX.ParseMatrix(m.Matrices[1]);
            var res = lu.Solve(b);
            res.MapInplace(entry => Math.Round(entry, 1));

            var x = MX.ParseMatrix(m.Matrices[2]);
            var c = res.Equals(x);

            return c;
        }


        public bool Ex3()
        {
            var m = new MxDTO
            {
                Matrices = new[] {
                    new MxDTO {
                        ID = "A",
                        RowSize = 2,
                        ColumnSize = 2,
                        EntriesType = MxDTO.ROW_ENTRIES,
                        Entries = new double[]
                        {
                             10, 5,
                              2, 9,
                        }
                    },
                    new MxDTO
                    {
                        ID = "y",
                        RowSize = 2,
                        ColumnSize = 1,
                        EntriesType = MxDTO.COLUMN_ENTRIES,
                        Entries = new double[]
                        {
                            6, 3
                        }
                    },
                    new MxDTO
                    {
                        ID = "x",
                        RowSize = 2,
                        ColumnSize = 1,
                        EntriesType = MxDTO.COLUMN_ENTRIES,
                        Entries = new double[]
                        {
                            0.4875, 0.225
                        }
                    },
                }
            };

            var A = MX.ParseMatrix(m.Matrices[0]);
            var y = MX.ParseMatrix(m.Matrices[1]);

            var res = MX.GaussSeidel(A, y, 1e-4, 10);

            res.MapInplace(entry => Math.Round(entry, 4));
            var x = MX.ParseMatrix(m.Matrices[2]);
            var c = res.Equals(x);

            return c;
        }

    }
}
