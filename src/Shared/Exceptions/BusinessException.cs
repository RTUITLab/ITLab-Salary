using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Shared.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {

        }
        public BusinessException(string message, Exception innerExecption) : base(message, innerExecption)
        {

        }

        public BusinessException():base()
        {
        }
    }
}
