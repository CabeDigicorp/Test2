using ModelData.Dto.Error;

namespace JoinApi.Utilities
{
    public class ErrorDtoBuilder
    {
        public static ErrorDtoBuilder New
        {
            get
            {
                return new();
            }
        }

        private IDictionary<string, string> _errorData = new Dictionary<string, string>();
        public ErrorDtoBuilder Add(string key, string value)
        {
            _errorData[key] = value;
            return this;
        }

        public ErrorDto Build()
        {
            return new ErrorDto { ErrorData = new Dictionary<string,string>(_errorData) };
        }
    }
}
