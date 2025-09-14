namespace Dashboard.Common
{
    public class DataResult<T>
    {
        public int Status { get; set; } = 500;
        public string Message { get; set; } = string.Empty;
        public List<string>? Messages { get; set; }
        public T Data { get; set; } = default!;
        public int TotalCount { get; set; }
        public DataResult()
        {
        }
        public DataResult(T data)
        {
            Data = data;
        }

        public DataResult(int status, string message)
        {
            Status = status;
            Message = message;
        }

        public DataResult(int status, string message, T data)
        {
            Status = status;
            Message = message;
            Data = data;
        }

        public DataResult(int status, string message, T data, int totalCount)
        {
            Status = status;
            Message = message;
            Data = data;
            TotalCount = totalCount;
        }
    }
}
