namespace SMSRateLimitingMS.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccessful { get; }
        public T? Value { get; }
        public string Error { get; }

        private Result(bool isSuccessful, T? value, string error)
        {
            IsSuccessful = isSuccessful;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value)
            => new(true, value, string.Empty);

        public static Result<T> Failure(string error)
            => new(false, default, error);
    }
}
