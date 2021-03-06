﻿using CSCore.CoreAudioAPI;
using Digimezzo.Utilities.Helpers;
using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Base;
using Dopamine.Common.Services.Dialog;
using Dopamine.Common.Services.ExternalControl;
using Dopamine.Common.Services.Notification;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Taskbar;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsPlaybackViewModel : BindableBase
    {
        public class OutputDevice
        {
            public string Name { get; set; }
            public MMDevice Device { get; set; }

            public override string ToString()
            {
                return this.Name;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }

                return this.Name.Equals(((OutputDevice)obj).Name);
            }

            public override int GetHashCode()
            {
                return new { this.Name, }.GetHashCode();
            }
        }

        private ObservableCollection<NameValue> latencies;
        private NameValue selectedLatency;
        private IPlaybackService playbackService;
        private ITaskbarService taskbarService;
        private INotificationService notificationService;
        private IDialogService dialogService;
        protected IExternalControlService externalControlService;
        private bool checkBoxWasapiExclusiveModeChecked;
        private bool checkBoxShowNotificationWhenPlayingChecked;
        private bool checkBoxShowNotificationWhenPausingChecked;
        private bool checkBoxShowNotificationWhenResumingChecked;
        private bool checkBoxShowNotificationControlsChecked;
        private bool checkBoxShowProgressInTaskbarChecked;
        private bool checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked;
        private bool checkBoxEnableExternalControlChecked;
        private bool checkBoxEnableSystemNotificationChecked;
        private bool checkBoxLoopWhenShuffleChecked;
        private ObservableCollection<NameValue> notificationPositions;
        private NameValue selectedNotificationPosition;
        private ObservableCollection<int> notificationSeconds;
        private int selectedNotificationSecond;
        private ObservableCollection<OutputDevice> outputDevices;
        private OutputDevice selectedOutputDevice;

        public DelegateCommand ShowTestNotificationCommand { get; set; }

        public bool IsNotificationEnabled => (this.CheckBoxShowNotificationWhenPlayingChecked || this.CheckBoxShowNotificationWhenPausingChecked || this.CheckBoxShowNotificationWhenResumingChecked) && !this.CheckBoxEnableSystemNotificationChecked;

        public ObservableCollection<NameValue> Latencies
        {
            get => this.latencies;
            set => SetProperty<ObservableCollection<NameValue>>(ref this.latencies, value);
        }

        public NameValue SelectedLatency
        {
            get => this.selectedLatency;
            set
            {
                SettingsClient.Set<int>("Playback", "AudioLatency", value.Value);
                SetProperty<NameValue>(ref this.selectedLatency, value);

                if (this.playbackService != null)
                {
                    this.playbackService.Latency = value.Value;
                }
            }
        }

        public bool CheckBoxWasapiExclusiveModeChecked
        {
            get { return this.checkBoxWasapiExclusiveModeChecked; }
            set
            {
                if (value)
                {
                    this.ConfirmEnableExclusiveMode();
                }
                else
                {
                    this.ApplyExclusiveMode(false);
                }
            }
        }

        public bool CheckBoxShowNotificationWhenPlayingChecked
        {
            get => this.checkBoxShowNotificationWhenPlayingChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationWhenPlayingChecked, value);
                this.notificationService.ShowNotificationWhenPlaying = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxShowNotificationWhenPausingChecked
        {
            get => this.checkBoxShowNotificationWhenPausingChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationWhenPausingChecked, value);
                this.notificationService.ShowNotificationWhenPausing = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxShowNotificationWhenResumingChecked
        {
            get => this.checkBoxShowNotificationWhenResumingChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationWhenResumingChecked, value);
                this.notificationService.ShowNotificationWhenResuming = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public bool CheckBoxShowNotificationControlsChecked
        {
            get => this.checkBoxShowNotificationControlsChecked;
            set
            {
                SetProperty<bool>(ref this.checkBoxShowNotificationControlsChecked, value);
                this.notificationService.ShowNotificationControls = value;
            }
        }

        public bool CheckBoxShowProgressInTaskbarChecked
        {
            get => this.checkBoxShowProgressInTaskbarChecked;
            set
            {
                SettingsClient.Set<bool>("Playback", "ShowProgressInTaskbar", value);
                SetProperty<bool>(ref this.checkBoxShowProgressInTaskbarChecked, value);

                if (this.taskbarService != null && this.playbackService != null)
                {
                    this.taskbarService.SetShowProgressInTaskbar(value);
                }
            }
        }

        public bool CheckBoxLoopWhenShuffleChecked
        {
            get => this.checkBoxLoopWhenShuffleChecked;
            set
            {
                SettingsClient.Set<bool>("Playback", "LoopWhenShuffle", value);
                SetProperty<bool>(ref this.checkBoxLoopWhenShuffleChecked, value);
            }
        }

        public bool CheckBoxShowNotificationOnlyWhenPlayerNotVisibleChecked
        {
            get => this.checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked;
            set
            {
                SettingsClient.Set<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible", value);
                SetProperty<bool>(ref this.checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked, value);
            }
        }

        public ObservableCollection<NameValue> NotificationPositions
        {
            get => this.notificationPositions;
            set => SetProperty<ObservableCollection<NameValue>>(ref this.notificationPositions, value);
        }

        public NameValue SelectedNotificationPosition
        {
            get => this.selectedNotificationPosition;
            set
            {
                SettingsClient.Set<int>("Behaviour", "NotificationPosition", value.Value);
                SetProperty<NameValue>(ref this.selectedNotificationPosition, value);
            }
        }

        public ObservableCollection<int> NotificationSeconds
        {
            get => this.notificationSeconds;
            set => SetProperty<ObservableCollection<int>>(ref this.notificationSeconds, value);
        }

        public int SelectedNotificationSecond
        {
            get => this.selectedNotificationSecond;
            set
            {
                SettingsClient.Set<int>("Behaviour", "NotificationAutoCloseSeconds", value);
                SetProperty<int>(ref this.selectedNotificationSecond, value);
            }
        }

        public ObservableCollection<OutputDevice> OutputDevices
        {
            get => this.outputDevices;
            set => SetProperty<ObservableCollection<OutputDevice>>(ref this.outputDevices, value);
        }

        public OutputDevice SelectedOutputDevice
        {
            get => this.selectedOutputDevice;
            set
            {
                SetProperty<OutputDevice>(ref this.selectedOutputDevice, value);

                // Due to two-way binding, this can be null when the list is being filled.
                if (value != null)
                {
                    SettingsClient.Set<string>("Playback", "AudioDevice", value.Device == null ? string.Empty : value.Device.DeviceID);

                    this.playbackService.SwitchOutputDeviceAsync(value.Device);
                }
            }
        }

        public bool CheckBoxEnableExternalControlChecked
        {
            get => this.checkBoxEnableExternalControlChecked;
            set
            {
                SettingsClient.Set("Playback", "EnableExternalControl", value);
                SetProperty(ref this.checkBoxEnableExternalControlChecked, value);
                if (value == true)
                    this.externalControlService.Start();
                else
                    this.externalControlService.Stop();
            }
        }

        public bool IsWindows10 => Constants.IsWindows10;

        public bool CheckBoxEnableSystemNotificationChecked
        {
            get => this.checkBoxEnableSystemNotificationChecked;
            set
            {
                SetProperty(ref this.checkBoxEnableSystemNotificationChecked, value);
                this.notificationService.SystemNotificationIsEnabled = value;
                RaisePropertyChanged(nameof(this.IsNotificationEnabled));
            }
        }

        public SettingsPlaybackViewModel(IPlaybackService playbackService, ITaskbarService taskbarService, INotificationService notificationService, IDialogService dialogService, IExternalControlService externalControlService)
        {
            this.playbackService = playbackService;
            this.taskbarService = taskbarService;
            this.notificationService = notificationService;
            this.dialogService = dialogService;
            this.externalControlService = externalControlService;

            this.ShowTestNotificationCommand = new DelegateCommand(() => this.notificationService.ShowNotificationAsync());

            this.playbackService.AudioDevicesChanged += (_, __) => Application.Current.Dispatcher.Invoke(() => this.GetOutputDevicesAsync());

            this.GetCheckBoxesAsync();
            this.GetNotificationPositionsAsync();
            this.GetNotificationSecondsAsync();
            this.GetLatenciesAsync();
            this.GetOutputDevicesAsync();
        }

        private async void GetOutputDevicesAsync()
        {
            IList<MMDevice> outputDevices = await this.playbackService.GetAllOutputDevicesAsync();

            this.OutputDevices = new ObservableCollection<OutputDevice>();

            this.OutputDevices.Add(new OutputDevice() { Name = ResourceUtils.GetString("Language_Default_Audio_Device"), Device = null });

            foreach (MMDevice device in outputDevices)
            {
                this.OutputDevices.Add(new OutputDevice() { Name = device.FriendlyName, Device = device });
            }

            MMDevice savedDevice = await this.playbackService.GetSavedAudioDeviceAsync();

            this.selectedOutputDevice = savedDevice == null ? this.OutputDevices.First() : new OutputDevice() { Name = savedDevice.FriendlyName, Device = savedDevice };
            RaisePropertyChanged(nameof(this.SelectedOutputDevice));
        }

        private async void GetLatenciesAsync()
        {
            var localLatencies = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                // Increment by 50
                for (int index = 50; index <= 500; index += 50)
                {
                    if (index == 200)
                    {
                        localLatencies.Add(new NameValue
                        {
                            Name = index + " ms (" + Application.Current.FindResource("Language_Default").ToString().ToLower() + ")",
                            Value = index
                        });
                    }
                    else
                    {
                        localLatencies.Add(new NameValue
                        {
                            Name = index + " ms",
                            Value = index
                        });
                    }
                }
            });

            this.Latencies = localLatencies;

            NameValue localSelectedLatency = null;

            await Task.Run(() => localSelectedLatency = this.Latencies.Where((pa) => pa.Value == SettingsClient.Get<int>("Playback", "AudioLatency")).Select((pa) => pa).First());

            this.SelectedLatency = localSelectedLatency;
        }

        private async void GetNotificationPositionsAsync()
        {
            var localNotificationPositions = new ObservableCollection<NameValue>();

            await Task.Run(() =>
            {
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Bottom_Left"), Value = (int)NotificationPosition.BottomLeft });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Top_Left"), Value = (int)NotificationPosition.TopLeft });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Top_Right"), Value = (int)NotificationPosition.TopRight });
                localNotificationPositions.Add(new NameValue { Name = ResourceUtils.GetString("Language_Bottom_Right"), Value = (int)NotificationPosition.BottomRight });
            });

            this.NotificationPositions = localNotificationPositions;

            NameValue localSelectedNotificationPosition = null;

            await Task.Run(() => localSelectedNotificationPosition = NotificationPositions.Where((np) => np.Value == SettingsClient.Get<int>("Behaviour", "NotificationPosition")).Select((np) => np).First());

            this.SelectedNotificationPosition = localSelectedNotificationPosition;
        }

        private async void GetNotificationSecondsAsync()
        {
            var localNotificationSeconds = new ObservableCollection<int>();

            await Task.Run(() =>
            {
                for (int index = 1; index <= 5; index++)
                {
                    localNotificationSeconds.Add(index);
                }

            });

            this.NotificationSeconds = localNotificationSeconds;

            int localSelectedNotificationSecond = 0;

            await Task.Run(() => localSelectedNotificationSecond = NotificationSeconds.Where((ns) => ns == SettingsClient.Get<int>("Behaviour", "NotificationAutoCloseSeconds")).Select((ns) => ns).First());

            this.SelectedNotificationSecond = localSelectedNotificationSecond;
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxWasapiExclusiveModeChecked = SettingsClient.Get<bool>("Playback", "WasapiExclusiveMode");
                this.checkBoxShowNotificationWhenPlayingChecked = this.notificationService.ShowNotificationWhenPlaying;
                this.checkBoxShowNotificationWhenPausingChecked = this.notificationService.ShowNotificationWhenPausing;
                this.checkBoxShowNotificationWhenResumingChecked = this.notificationService.ShowNotificationWhenResuming;
                this.checkBoxShowNotificationControlsChecked = this.notificationService.ShowNotificationControls;
                this.checkBoxShowProgressInTaskbarChecked = SettingsClient.Get<bool>("Playback", "ShowProgressInTaskbar");
                this.checkBoxShowNotificationOnlyWhenPlayerNotVisibleChecked = SettingsClient.Get<bool>("Behaviour", "ShowNotificationOnlyWhenPlayerNotVisible");
                this.checkBoxEnableExternalControlChecked = SettingsClient.Get<bool>("Playback", "EnableExternalControl");
                this.checkBoxLoopWhenShuffleChecked = SettingsClient.Get<bool>("Playback", "LoopWhenShuffle");
                this.checkBoxEnableSystemNotificationChecked = this.notificationService.SystemNotificationIsEnabled;
            });
        }

        private void ConfirmEnableExclusiveMode()
        {
            if (this.dialogService.ShowConfirmation(
                0xe11b,
                16,
                ResourceUtils.GetString("Language_Exclusive_Mode"),
                ResourceUtils.GetString("Language_Exclusive_Mode_Confirmation"),
                ResourceUtils.GetString("Language_Yes"),
                ResourceUtils.GetString("Language_No")))
            {
                ApplyExclusiveMode(true);
            }
        }

        private void ApplyExclusiveMode(bool isEnabled)
        {
            SettingsClient.Set<bool>("Playback", "WasapiExclusiveMode", isEnabled);
            SetProperty<bool>(ref this.checkBoxWasapiExclusiveModeChecked, isEnabled);
            if (this.playbackService != null) this.playbackService.ExclusiveMode = isEnabled;
        }
    }
}
