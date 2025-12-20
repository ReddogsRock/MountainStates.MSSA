using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace MountainStates.MSSA.Module.HelloWorld
{
    public class Interop
    {
        private readonly IJSRuntime _jsRuntime;

        public Interop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
    }
}
