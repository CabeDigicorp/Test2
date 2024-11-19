

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Commons
{

    public class BindingErrorException : Exception
    {
        public string SourceObject { get; set; }
        public string SourceProperty { get; set; }
        public string TargetElement { get; set; }
        public string TargetProperty { get; set; }

        public BindingErrorException()
            : base() { }

        public BindingErrorException(string message)
            : base(message) { }

    }

    public class BindingErrorTraceListener : TraceListener
    {
        private const string BindingErrorPattern = @"^BindingExpression path error(?:.+)'(.+)' property not found(?:.+)object[\s']+(.+?)'(?:.+)target element is '(.+?)'(?:.+)target property is '(.+?)'(?:.+)$";

        public override void Write(string s) { }

        public override void WriteLine(string message)
        {
            var xx = new BindingErrorException(message);

            var match = Regex.Match(message, BindingErrorPattern);
            if (match.Success)
            {
                xx.SourceObject = match.Groups[2].ToString();
                xx.SourceProperty = match.Groups[1].ToString();
                xx.TargetElement = match.Groups[3].ToString();
                xx.TargetProperty = match.Groups[4].ToString();
            }

            throw xx;
        }
    }

}