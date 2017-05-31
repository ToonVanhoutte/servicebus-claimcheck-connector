using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.ApiApps.ClaimCheck
{
    public interface IClaimCheckProvider
    {
        Task<string> StoreMessage(Byte[] messageContent);

        Task<byte[]> RetrieveMessage(string messageReference);
    }
}
