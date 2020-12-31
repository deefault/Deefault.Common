namespace Deefault.Common.Result
{
    public partial class Result
    {
        public static Result Succeed()
        {
            return new Result();
        }

        public static Result<T> Succeed<T>(T value)
        {
            return new Result<T>(value);
        }

        public static Result Fail(string error)
        {
            return new Result(error);
        }

        public static Result<T> Fail<T>(string error)
        {
            return new Result<T>(error, false);
        }
    }

    public partial class Result
    {
        public bool Success { get; }
        public string Error { get; }


        public Result()
        {
            Success = true;
            Error = null;
        }

        public Result(string error, bool success = false)
        {
            Success = false;
            Error = error;
        }

        public static bool operator true(Result x) => x.IsTrue;
        public static bool operator false(Result x) => !x.IsTrue;
        public static bool operator !(Result x) => !x.IsTrue;

        private bool IsTrue => Success;
    }

    public partial class Result<T> : Result
    {
        public T Value { get; }

        public Result(T value) : base()
        {
            Value = value;
        }

        public Result(string error, bool success = false) : base(error, success)
        {
        }
    }

    public class Result<TResult, TError>
        where TError: IResultError
    {
        public TResult Value { get; }
        public bool Success { get; }
        
        public TError Error { get; }

        public Result(TResult value)
        {
            Value = value;
        }

        public Result(TError error)
        {
            Success = false;
            Error = error;
        }
    }
}