using EEMathLib.DTO;
using EEMathLib.MatrixMath;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestUnit
{
    public class MxTest
    {
        public MxDTO MxData =>
            new MxDTO
            {
                Matrices = [ 
                    new MxDTO
                    {
                        ID = "MxTest1",
                        RowSize = 2,
                        ColumnSize = 2,
                        Entries = [ 
                            1, 2, 
                            3, 4 
                        ]
                    },
                    new MxDTO
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

            var o = JsonSerializer.Deserialize<MxDTO>(json, new JsonSerializerOptions
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
            Assert.Equal(m1[0, 1], 2);
            Assert.Equal(m1[1, 1], 4);
        }

        [Fact]
        public void Should_run_ex1()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex1());
        }

        [Fact]
        public void Should_run_ex2()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex2());
        }

        [Fact]
        public void Should_run_ex3()
        {
            var ex = new MxExamples();
            Assert.True(ex.Ex3());
        }
    }
}
