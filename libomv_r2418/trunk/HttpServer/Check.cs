using System;
using System.Collections.Generic;

namespace HttpServer
{
    /// <summary>
    /// Design by contract validator
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// "{0} must be specified."
        /// </summary>
        private const string FieldRequired = "FieldRequired";

        /// <summary>
        /// "'{0}' do not equal '{1}'."
        /// </summary>
        private const string FieldNotEqual = "FieldNotEqual";

        /// <summary>
        /// "'{0}' must be between {1} and {2}."
        /// </summary>
        private const string FieldBetween = "FieldBeetween";

        /// <summary>
        /// "'{0}' must be between {1} and {2} characters."
        /// </summary>
        private const string FieldBetweenStr = "FieldBeetwenStr";

        /// <summary>
        /// "'{0}' must be larger or equal to {1}."
        /// </summary>
        private const string FieldMin = "FieldMin";

        /// <summary>
        /// "'{0}' must be larger or equal to {1}."
        /// </summary>
        private const string FieldMinStr = "FieldMinStr";

        /// <summary>
        /// "'{0}' must evaluate as true.
        /// </summary>
        private const string FieldTrue = "FieldTrue";

        /// <summary>
        /// "'{0}' must evaluate as false.
        /// </summary>
        private const string FieldFalse = "FieldFalse";

        /// <summary>
        /// "'{0}' must be less or equal to {1}."
        /// </summary>
        private const string FieldMax = "FieldMax";

        /// <summary>
        /// "'{0}' must be less or equal to {1}."
        /// </summary>
        private const string FieldMaxStr = "FieldMaxStr";

        /// <summary>
        /// "'{0}' must be specified and not empty."
        /// </summary>
        private const string FieldNotEmpty = "FieldNotEmpty";

        /// <summary>
        /// "'{1}' must be assignable from {0}."
        /// </summary>
        private const string FieldType = "FieldType";

        private static readonly Dictionary<string, string> language;

        static Check()
        {
            language = new Dictionary<string, string>();
            language.Add(FieldTrue, "'{0}' must evaluate as true.");
            language.Add(FieldFalse, "'{0}' must evaluate as false.");
            language.Add(FieldRequired, "'{0}' is required.");
            language.Add(FieldNotEqual, "'{0}' do not equal '{1}'.");
            language.Add(FieldBetween, "'{0}' must be between {1} and {2}.");
            language.Add(FieldBetweenStr, "'{0}' must be between {1} and {2} characters.");
            language.Add(FieldMin, "{0} must be larger or equal to {1}.");
            language.Add(FieldMinStr, "{0} must be larger or equal to {1} characters.");
            language.Add(FieldMax, "{0} must be less or equal to {1}.");
            language.Add(FieldMaxStr, "{0} must be less or equal to {1} characters.");
            language.Add(FieldNotEmpty, "'{0}' must not be empty.");
            language.Add(FieldType, "'{1}' must be assignable from {0}.");
        }

        /// <summary>
        /// The specified statement/parameter must be true.
        /// </summary>
        /// <param name="statement">statement/parameter to evaluate.</param>
        /// <param name="messageOrParamName">Name of the message or param.</param>
        /// <exception cref="CheckException">If statement is not true.</exception>
        public static void True(bool statement, string messageOrParamName)
        {
            if (statement)
                return;

            Throw(messageOrParamName, FieldTrue);
        }

        /// <summary>
        /// The specified statement/parameter must be false.
        /// </summary>
        /// <param name="statement">statement/parameter to evaluate.</param>
        /// <param name="messageOrParamName">Name of the message or param.</param>
        /// <exception cref="CheckException">If statement is true.</exception>
        public static void False(bool statement, string messageOrParamName)
        {
            if (!statement)
                return;

            Throw(messageOrParamName, FieldFalse);
        }

        /// <summary>
        /// Two values can't be equal.
        /// </summary>
        /// <param name="value">value/constant to compare to.</param>
        /// <param name="paramValue">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        /// <remarks><paramref name="value"/> and <paramref name="paramValue"/> are both required.</remarks>
        public static void NotEqual(object value, object paramValue, string messageOrParamName)
        {
            Require(value, "value");
            Require(paramValue, messageOrParamName);
            if (value.Equals(paramValue))
                Throw(messageOrParamName, FieldRequired, value.ToString());
        }

        /// <summary>
        /// Value must be between (or equal) min and max
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="max">maximum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Between(int min, int max, int value, string messageOrParamName)
        {
            if (value >= min && value <= max)
                return;

            Throw(messageOrParamName, FieldBetween, min.ToString(), max.ToString());
        }

        /// <summary>
        /// Betweens the specified min.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="max">maximum value.</param>
        /// <param name="value">parameter value to check. May not be null.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Between(int min, int max, string value, string messageOrParamName)
        {
            Between(min, max, value, messageOrParamName, true);
        }

        /// <summary>
        /// Betweens the specified min.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="max">maximum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <param name="required"><paramref name="value"/> may be null if this parameter is false.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Between(int min, int max, string value, string messageOrParamName, bool required)
        {
            if (required) Require(value, messageOrParamName);
            if (value == null || value.Length >= min && value.Length <= max)
                return;

            Throw(messageOrParamName, FieldBetweenStr, min.ToString(), max.ToString());
        }

        /// <summary>
        /// Checks if the value is equal or larger.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Min(DateTime min, DateTime value, string messageOrParamName)
        {
            if (value >= min)
                return;

            Throw(messageOrParamName, FieldMin, min.ToString());
        }

        /// <summary>
        /// Checks if the value is equal or larger.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Min(int min, int value, string messageOrParamName)
        {
            if (value >= min)
                return;

            Throw(messageOrParamName, FieldMin, min.ToString());
        }

        /// <summary>
        /// Checks if the value is equal or larger.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Min(long min, long value, string messageOrParamName)
        {
            if (value >= min)
                return;

            Throw(messageOrParamName, FieldMin, min.ToString());
        }

        /// <summary>
        /// Checks if the value is equal or larger.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="value">parameter value (may not be null).</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Min(int min, string value, string messageOrParamName)
        {
            Min(min, value, messageOrParamName, true);
        }

        /// <summary>
        /// Checks if the value is equal or larger.
        /// </summary>
        /// <param name="min">minimum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <param name="required"><paramref name="value"/> may be null if this parameter is false.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Min(int min, string value, string messageOrParamName, bool required)
        {
            if (required) Require(value, messageOrParamName);
            if (value == null || value.Length >= min)
                return;

            Throw(messageOrParamName, FieldMinStr, min.ToString());
        }

        /// <summary>
        /// Checks if the value is less or equal.
        /// </summary>
        /// <param name="max">maximum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Max(int max, int value, string messageOrParamName)
        {
            if (value <= max)
                return;

            Throw(messageOrParamName, FieldMax, max.ToString());
        }

        /// <summary>
        /// Checks if the value is less or equal.
        /// </summary>
        /// <param name="max">maximum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Max(int max, string value, string messageOrParamName)
        {
            Max(max, value, messageOrParamName, true);
        }

        /// <summary>
        /// Checks if the value is less or equal.
        /// </summary>
        /// <param name="max">maximum value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <param name="required"><paramref name="value"/> may be null if this parameter is false.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Max(int max, string value, string messageOrParamName, bool required)
        {
            if (required) Require(value, messageOrParamName);
            if (value == null || value.Length <= max)
                return;

            Throw(messageOrParamName, FieldMaxStr, max.ToString());
        }

        /// <summary>
        /// Checks if the value is less or equal.
        /// </summary>
        /// <param name="max">max value.</param>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Max(DateTime max, DateTime value, string messageOrParamName)
        {
            if (value <= max)
                return;

            Throw(messageOrParamName, FieldMin, max.ToString());
        }

        /// <summary>
        /// Parameter is required (may not be null).
        /// </summary>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void Require(object value, string messageOrParamName)
        {
            if (value != null)
                return;

            Throw(messageOrParamName, FieldRequired);
        }

        /// <summary>
        /// The specified string may not be null or empty.
        /// </summary>
        /// <param name="value">parameter value.</param>
        /// <param name="messageOrParamName">parameter name, or a error message.</param>
        /// <exception cref="CheckException">If contract fails.</exception>
        public static void NotEmpty(string value, string messageOrParamName)
        {
            if (!string.IsNullOrEmpty(value))
                return;

            Throw(messageOrParamName, FieldNotEmpty);
        }

        private static void Throw(string messageOrParamName, string message, params string[] arguments)
        {
            string[] args = new string[arguments.Length + 1];
            arguments.CopyTo(args, 1);
            args[0] = messageOrParamName;

            if (messageOrParamName.IndexOf(' ') == -1)
            {
                string format = language[message] ?? message;
                throw new CheckException(message, string.Format(format, args), args);
            }

            throw new CheckException(message, messageOrParamName, args);
        }

        /// <summary>
        /// Check if the specified type can be assigned from the parameter type.
        /// </summary>
        /// <param name="type">Type that the parameter must be  (or derive/implement).</param>
        /// <param name="instance">instance</param>
        /// <param name="messageOrParamName">error message or parameter name</param>
        public static void Type(Type type, object instance, string messageOrParamName)
        {
            if (type.IsAssignableFrom(instance.GetType()))
                return;

            Throw(messageOrParamName, FieldType, type.ToString());
        }
    }

    /// <summary>
    /// Exception thrown when a validation fails.
    /// </summary>
    public class CheckException : ArgumentException
    {
        private readonly string _orgString;
        private readonly string[] _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckException"/> class.
        /// </summary>
        /// <param name="orgMessage">The original error message (have not been formatted).</param>
        /// <param name="msg">Formatted message.</param>
        /// <param name="arguments">Message arguments.</param>
        internal CheckException(string orgMessage, string msg, string[] arguments)
            : base(msg, arguments[0])
        {
            _orgString = orgMessage ?? string.Empty;
            _arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckException"/> class.
        /// </summary>
        /// <param name="orgMessage">The original error message (have not been formatted).</param>
        /// <param name="msg">Formatted message.</param>
        internal CheckException(string orgMessage, string msg)
            : base(msg)
        {
            _orgString = orgMessage ?? string.Empty;
            _arguments = new string[] { string.Empty };
        }

        /// <summary>
        /// Unformatted error message, {0} have not been replaced with parameter name.
        /// </summary>
        /// <remarks>
        /// Can be used if you want to translate messages.
        /// </remarks>
        public string OrgString
        {
            get { return _orgString; }
        }

        /// <summary>
        /// Arguments to string to format. First argument is parameter name.
        /// </summary>
        public string[] Arguments
        {
            get { return _arguments; }
        }
    }
}
