using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace xUnitTest
{
    public class Class2
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, 3);
        }

    }
}
