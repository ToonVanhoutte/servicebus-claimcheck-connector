using System;
using System.Threading.Tasks;

namespace TVH.ClaimCheck
{
    public interface IClaimCheckProvider
    {
        Task<string> StoreMessage(Byte[] messageContent);

        Task<byte[]> RetrieveMessage(string messageReference);
    }
}
