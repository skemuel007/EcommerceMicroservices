namespace Catalog.API.Dtos.Response
{
    public class ApiResponse<T>
    {

        public ApiResponse()
        {

        }
        public ApiResponse(T data, string message, bool status)
        {
            Status = status;
            Message = message;
            Data = data;

        }
        public bool Status { get; set; } = true;
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
