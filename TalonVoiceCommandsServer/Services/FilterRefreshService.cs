using System;
using System.Threading.Tasks;

namespace TalonVoiceCommandsServer.Services
{
    public class FilterRefreshService
    {
        public event Func<Task>? OnRefreshRequested;

        public async Task RequestRefreshAsync()
        {
            if (OnRefreshRequested != null)
            {
                await OnRefreshRequested.Invoke();
            }
        }
    }
}
