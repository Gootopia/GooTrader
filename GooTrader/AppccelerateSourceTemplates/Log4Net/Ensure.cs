using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This class is required by the Appccelerate log4net extension
public static class Ensure
{
    /// <summary>
    /// Ensures that the specified argument is not null.
    /// </summary>
    /// <param name="argumentName">Name of the argument.</param>
    /// <param name="argument">The argument.</param>
    public static void ArgumentNotNull(object argument, string argumentName)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }
}
