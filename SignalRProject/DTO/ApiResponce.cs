using Microsoft.AspNetCore.Components.Web;

namespace SignalRProject.DTO
{
    public class ApiResponse<TData>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public TData? Data { get; set; }
        public object? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public static ApiResponse<TData> Create(bool success, int statusCode, string message, TData? data = default, object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = success,
                StatusCode = statusCode, 
                Message = message,
                Data = data,
                Errors = errors
            };
        }

        public static ApiResponse<TData> Ok(TData data) =>
            Create(true, 200, "Success", data);
        
        public static ApiResponse<TData> Ok(TData data, string message) =>
            Create(true, 200, message, data);

        public static ApiResponse<TData> Ok(string message) =>
            Create(true, 200, message, default);

        public static ApiResponse<TData> Created(TData data, string message = "Resource created successfully") =>
            Create(true, 201, message, data);

        public static ApiResponse<TData> Accepted(string message = "Request accepted for processing") =>
            Create(true, 202, message);

        public static ApiResponse<TData> NoContent() =>
            Create(true, 204, "No Content");

        public static ApiResponse<TData> BadRequest(string message) =>
            Create(false, 400, message);

        public static ApiResponse<TData> BadRequest(string message, object errors) =>
            Create(false, 400, message, default, errors);

        public static ApiResponse<TData> ValidationFailed(IEnumerable<string> errors) =>
            Create(false, 400, "Validation failed", default, errors);

        public static ApiResponse<TData> Unauthorized(string message = "Unauthorized") =>
            Create(false, 401, message);

        public static ApiResponse<TData> Forbidden(string message = "Access denied") =>
            Create(false, 403, message);

        public static ApiResponse<TData> NotFound(string message = "Resource not found") =>
            Create(false, 404, message);

        public static ApiResponse<TData> Conflict(string message = "Resource already exists or conflict occurred") =>
            Create(false, 409, message);

        public static ApiResponse<TData> Unprocessable(object errors, string message = "Unprocessable entity") =>
            Create(false, 422, message, default, errors);

        public static ApiResponse<TData> InternalServerError(string message = "An unexpected error occurred") =>
            Create(false, 500, message);

        public static ApiResponse<TData> InternalServerError(Exception ex, string message = "An unexpected error occurred") =>
            Create(false, 500, message, default, new { ExceptionMessage = ex.Message, StackTrace = ex.StackTrace });
    }
}
