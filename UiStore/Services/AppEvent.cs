

using static UiStore.Services.AppStatusInfo;

namespace UiStore.Services
{
    internal class AppEvent
    {
        public AppEvent()
        {
            ActiveStatus = new OnAppAction<bool>();
            EnableStatus = new OnAppAction<bool>();
            AutoRunAction = new OnAppAction<bool>();
            ExtractAction = new OnAppAction<ExtractState>();
            UpdateAction = new OnAppAction<UpdateState>();
            NewVersionStatus = new OnAppAction<bool>();
            RunningStatus = new OnAppAction<(bool, bool)>();
        }
        public OnAppAction<bool> ActiveStatus { get; private set; }
        public OnAppAction<bool> EnableStatus { get; private set; }
        public OnAppAction<bool> AutoRunAction { get; private set; }
        public OnAppAction<UpdateState> UpdateAction { get; private set; }
        public OnAppAction<ExtractState> ExtractAction { get; private set; }
        public OnAppAction<bool> NewVersionStatus { get; private set; }
        public OnAppAction<(bool, bool)> RunningStatus { get; private set; }


    }
}
