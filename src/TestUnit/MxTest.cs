using EEMathLib.DTO;
using EEMathLib.MatrixMath;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestUnit
{
    public class MxTest
    {
        public MxDTO<double> MxData =>
            new MxDTO<double>
            {
                Matrices = [ 
                    new MxDTO< double >
                    {
                        ID = "MxTest1",
                        RowSize = 2,
                        ColumnSize = 2,
                        Entries = [ 
                            1, 2, 
                            3, 4 
                        ]
                    },
                    new MxDTO< double >
                    {
                        ID = "MxTest2",
                        RowSize = 2,
                        ColumnSize = 2,
                        Entries = [ 
                            5, 6, 
                            7, 8 
                        ]
                    }
                ]
            };

        [Fact]
        public void Should_save_to_JSON()
        {
            var d = MxData;

            var json = JsonSerializer
                .Serialize(d, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var o = JsonSerializer.Deserialize<MxDTO<double>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });

            Assert.Equal(d.Matrices.Length, o.Matrices.Length);
        }

        [Fact]
        public void Should_build_matrix()
        {
            var d = MxData;

            var m1 = MX.ParseMatrix(d.Matrices[0]);

            Assert.NotNull(m1);
            Assert.Equal(m1[0, 0], 1);
            Assert.Equal(m1[0, 1], 2);
            Assert.Equal(m1[1, 0], 3);
            Assert.Equal(m1[1, 1], 4);
        }

        [Fact]
        public void Solve_Ax_b()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex1());
        }

        [Fact]
        public void Solve_Ax_b_with_LU()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex2());
        }

        [Fact]
        public void Solve_Ax_b_with_Gauss_Seidel_iteration()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex3());
        }

        [Fact]
        public void Solve_Ax_b_with_Gauss_Seidel_iteration_matrix()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex3a());
        }

        [Fact]
        public void Solve_Ax_b_diverge_with_Gauss_Seidel_iteration()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex4());
        }
    }
}
