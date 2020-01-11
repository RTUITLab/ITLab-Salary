using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Shared.Exceptions
{
    public class BadRequestException : BusinessException
    {
        public BadRequestException() : base()
        {

        }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
