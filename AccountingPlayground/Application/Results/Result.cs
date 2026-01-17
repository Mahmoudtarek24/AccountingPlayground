using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AccountingPlayground.Application.Results
{
    public class Result
    {
        public bool IsSuccess { get; }      
        public string Message { get; } // $" {Product} retrieved successfully" ,  { get; } replace { get; set; }   to make it immutable after create it 
        public Error Error { get; set; }    

        public Result(string message = "", bool result = true, Error error = null)
        {
            IsSuccess = result;
            Message = message;     
            this.Error = error ;       
        }

        public static Result Success(string message)
            => new Result(message);
        
        public static Result Failure(Error error, string message)
            => new Result(message, false,error);      
    }
    public class Result<T> : Result
    {
        public T Data { get; set; }

        public Result(T? value, string message = "", bool isSuccess = true, Error? error = null) : base( message, isSuccess, error) 
        {
            this.Data = value ;
        }

        public static Result<T> Success<T>(T data, string message = null)
            => new Result<T>(data,message);

        public static Result<T> Failure<T>(Error error ,string message)
            => new Result<T> (default,message,false,error);        
    }   

    public class Error 
    {
        public string Code { get; set; }
        public ErrorType ErrorType { get; }

        public Error(ErrorType errorType, string code) 
        {
            this.ErrorType = errorType; 
            this.Code = code;       
        }

        public static readonly Error None = new Error(ErrorType.None, ErrorCodes.None);

        public static Error NotFound(string code)
            => new Error(ErrorType.NotFound, code); 

    }

    public enum ErrorType
    {
        None,
        NotFound,
        Validation,
        Unauthorized,
        Conflict,
        Internal
    }       
}
