using System;
using System.Collections.Generic;
using System.Text;

namespace ITLab.Salary.Shared.Exceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message) : base(message)
        {

        }
        public NotFoundException(string message, Exception innerExecption) : base(message, innerExecption)
        {

        }

        public NotFoundException() : base()
        {
        }
    }
}
