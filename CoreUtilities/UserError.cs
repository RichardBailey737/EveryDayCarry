using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities
{
    /// <summary>
    /// This is a special class of exception with the intention of providing feedback to the user.  It allows us to catch and distinguish feedback messages from normal errors.
    /// </summary>
    public class UserError : Exception
    {
        public UserError(string message) : base(message)
        {
        }

        public UserError(string message, string ErrorCode) : base(message)
        {
            this.ErrorCode = ErrorCode;
        }

        public UserError(string message, string ErrorCode, string propertyName) : base(message)
        {
            this.ErrorCode = ErrorCode;
            this.PropertyName=propertyName;
        }

        public UserError(string message, string ErrorCode, string propertyName, string AttemptedValue) : base(message)
        {
            this.ErrorCode = ErrorCode;
            this.PropertyName=propertyName;
            this.AttemptedValue=AttemptedValue;
        }

        public string ErrorCode { get; set; }
        public string PropertyName { get; set; }
        public string AttemptedValue { get; set; }

    }

    /// <summary>
    /// An error class specifically for when we cannot find a record.
    /// </summary>
    public class RecordNotFound : UserError
    {
        public RecordNotFound(string message) : base(message, "RecordNotFound") { }
        public RecordNotFound(string message, string PropertyName) : base(message, "RecordNotFound", PropertyName) { }
    }
}
