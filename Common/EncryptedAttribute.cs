using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dashboard.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EncryptedAttribute : Attribute
    {

    }
}