using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIReplay.CommandLine
{
    public class Exceptions
    {
        public class PIServerNotFoundException : Exception
        {
            public PIServerNotFoundException() : base("The PI Data Archive could not be found") { }
        }

        public class InvalidParameterException : Exception
        {
            public InvalidParameterException(string parameter = null, string message=null) : base($"The provided parameter is invalid {parameter}. {message}") { }
        }
    }
}
