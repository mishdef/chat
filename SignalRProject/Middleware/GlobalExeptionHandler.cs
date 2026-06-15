using SignalRProject.DTO;

namespace SignalRProject.Middleware
{
    public class GlobalExeptionHandler
    {
        private readonly RequestDelegate _next;

        public GlobalExeptionHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"[ERROR]");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($": {ex.Message}\r\n");
                Console.Beep();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[STACKTRACE]: {ex.StackTrace}");

                context.Response.StatusCode = ex switch
                {
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    ApplicationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                await context.Response.WriteAsJsonAsync(
                    new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = context.Response.StatusCode,
                        Errors = ex.Message,
                    }
                );
            }
        }
    }
}
