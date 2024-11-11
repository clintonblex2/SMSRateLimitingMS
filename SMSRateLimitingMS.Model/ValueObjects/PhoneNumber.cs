using SMSRateLimitingMS.Domain.Common;

namespace SMSRateLimitingMS.Domain.ValueObjects
{
    public class PhoneNumber : ValueObject
    {
        public string Value { get; }

        private PhoneNumber(string value)
        {
            Value = value;
        }

        public static Result<PhoneNumber> Create(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Result<PhoneNumber>.Failure("Phone number cannot be empty");

            if (!value.StartsWith('+'))
                return Result<PhoneNumber>.Failure("Phone number must start with '+'");

            if (value.Length < 8 || value.Length > 15)
                return Result<PhoneNumber>.Failure("Phone number length must be between 7 and 14 digits after '+'");

            if (!value.Skip(1).All(char.IsDigit))
                return Result<PhoneNumber>.Failure("Phone number must only contain digits after '+'");

            return Result<PhoneNumber>.Success(new PhoneNumber(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
