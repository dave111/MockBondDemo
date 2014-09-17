using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BondDataService
{
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IProducer
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(string name);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(string name);
    }

    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void NewData(BondData data);
    }
}
