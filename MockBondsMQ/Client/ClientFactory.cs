using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockBonds
{
    public class ClientFactory
    {
        static public IClient CreateClient()
        {
            return new Client();
        }
    }
}
