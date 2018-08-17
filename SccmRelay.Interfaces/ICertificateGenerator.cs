using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SccmRelay.Interface
{
    [ServiceContract(Namespace = "urn:ps")]
    public interface ICertificateGenerator
    {
        [OperationContract]
        byte[] GetCertificate(string hostname);
    }
}