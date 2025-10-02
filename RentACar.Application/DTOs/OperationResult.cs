namespace RentACar.Application.DTOs
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static OperationResult<T> SuccessResult(T? data, string message = "Success")
            => new OperationResult<T> { Success = true, Message = message, Data = data };

        public static OperationResult<T> Failure(string message)
            => new OperationResult<T> { Success = false, Message = message, Data = default };
    }
}
