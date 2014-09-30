using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockBonds
{
    public interface IClient : IDisposable
    {
        void Subscribe(string isin);

        void Unsubscribe(string isin);

        Data GetNext();
    }
}
