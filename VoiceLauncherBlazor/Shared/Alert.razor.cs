using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using System.Timers;
using VoiceLauncherBlazor.Data;

namespace VoiceLauncherBlazor.Shared
{
    public partial class Alert : IDisposable
    {
        [Parameter] public string Title { get; set; } = "Title of alert goes here";
        [Parameter] public string Display { get; set; } = "The message goes here";
        [Parameter] public int Duration { get; set; } = 0;
        [Parameter] public string AlertType { get; set; } = "info";
        [Parameter] public bool Show { get; set; } = false;

        [Inject]
        NotifierService Notifier { get; set; }
        Timer timer;

        protected override async Task OnInitializedAsync()
        {
            await Task.Run(() => Show = true);
            if (Duration > 0)
            {
                timer = new Timer(Duration);
                timer.Start();
                timer.Elapsed += async (sender, e) => await TimerTick();
                Notifier.Notify += OnNotify;
            }
        }
        protected override Task OnParametersSetAsync()
        {
            if (Duration > 0)
            {
                timer.Start();
            }
            return Task.CompletedTask;
        }
        private async Task TimerTick()
        {
            Show = false;
            timer.Stop();
            await OnNotify("Hide Alert", 0);
            Dispose();
        }
        private void Hide()
        {
            Show = false;
            Dispose();
        }
        public async Task OnNotify(string key, int value)
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            if (Notifier != null)
            {
                Notifier.Notify -= OnNotify;
            }
            if (timer != null)
            {
                timer.Elapsed -= async (sender, e) => await TimerTick();
            }
        }

    }
}
