namespace EEMathLib.DTO
{
    public enum ErrorEnum
    {
        NoError,
        MaxIteration,
        ZeroDiagEntry,
        Divergence
    }

    public class Result<T>
    {
        public bool IsError => Error != ErrorEnum.NoError;
        public T Data { get; set; }
        public int IterationStop { get; set; }
        public ErrorEnum Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}
