using System.Net;

namespace RelationshipService.Domain.Models;

public class Result : IResult
{
    public bool IsSuccess { get; }

    public HttpStatusCode StatusCode {  get; }

    public Error Error {  get; }

    public List<string> Errors {  get; }

    protected Result(bool isSuccess, HttpStatusCode statusCode, Error error = null, List<string> errors = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Error = error;
        Errors = errors;
    }

    public static Result Success(HttpStatusCode statusCode = HttpStatusCode.OK) => new Result(true, statusCode);
    public static Result Failure(Error error, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new Result(false, statusCode, error); // error kodu ve mesaji enum olarak tanımlanabilir, bu sayede merkezi bir hata yönetimi sağlanır.
    public static Result Failure(HttpStatusCode statusCode, List<string> errors)
        => new Result(false, statusCode, new Error("Validation.Error", "One or more validation errors occurred."), errors);
}

public class Result<T> : Result, IResult<T>
{
    public T Value { get; }
    private Result(bool isSuccess, HttpStatusCode statusCode, T value, Error error = null, List<string> errors = null) : base(isSuccess, statusCode, error, errors)
    {
        Value = value;
    }
    public static Result<T> Success(T value, HttpStatusCode statusCode = HttpStatusCode.OK) => new Result<T>(true, statusCode, value );
    public static new Result<T> Failure(Error error, HttpStatusCode statusCode = HttpStatusCode.BadRequest) => new Result<T>(false, statusCode, default, error);
        public static new Result<T> Failure(HttpStatusCode statusCode, List<string> errors)
            => new Result<T>(false, statusCode, default, new Error("Validation.Error", "One or more validation errors occurred."), errors);
}

public interface IResult
{
    bool IsSuccess { get; }
    HttpStatusCode StatusCode { get; } // HTTP Status Code için
    Error Error { get; }    // Tekil hata detayları
    List<string> Errors { get; } // Koleksiyon bazlı hatalar (örn: Validation)
}

public interface IResult<out T> : IResult
{
    T Value { get; }
}