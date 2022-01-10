#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBSnorro.Tests.IntertestDependency
{
    public class InvalidTestConfigurationException : Exception
    {
        public InvalidTestConfigurationException() : base() { }
        public InvalidTestConfigurationException(string? message) : base(message) { }
        public InvalidTestConfigurationException(string? message, Exception innerException) : base(message, innerException) { }
    }

}
