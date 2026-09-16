using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EarTrumpet.UI.ViewModels
{
    /// <summary>
    /// Keeps the flyout's single microphone control in sync with the current
    /// Windows default recording endpoint.  It deliberately does not expose
    /// recording application sessions: Windows has one shared input level.
    /// </summary>
    public sealed class MicrophoneViewModel : BindableBase
    {
        private readonly DeviceCollectionViewModel _recordingDevices;
        private readonly IAudioDeviceManager _recordingDeviceManager;

        public ObservableCollection<DeviceViewModel> Devices => _recordingDevices.AllDevices;
        public DeviceViewModel SelectedDevice
        {
            get => _recordingDevices.Default;
            set
            {
                if (value == null || value.Id == _recordingDevices.Default?.Id)
                {
                    return;
                }

                var device = _recordingDeviceManager.Devices.FirstOrDefault(x => x.Id == value.Id);
                if (device == null)
                {
                    return;
                }

                // Keep normal applications and communications applications on
                // the same microphone, matching the behavior users expect from
                // the Windows default input selector.
                _recordingDeviceManager.Default = device;
                ((IAudioDeviceManagerWindowsAudio)_recordingDeviceManager).SetDefaultDevice(device, ERole.eCommunications);
            }
        }

        public bool HasDevice => SelectedDevice != null;

        public MicrophoneViewModel(DeviceCollectionViewModel recordingDevices, IAudioDeviceManager recordingDeviceManager)
        {
            _recordingDevices = recordingDevices;
            _recordingDeviceManager = recordingDeviceManager;

            _recordingDevices.DefaultChanged += OnDefaultChanged;
            _recordingDevices.AllDevices.CollectionChanged += OnDevicesChanged;
        }

        private void OnDefaultChanged(object sender, DeviceViewModel e)
        {
            RaiseSelectionChanged();
        }

        private void OnDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Devices));
            RaisePropertyChanged(nameof(HasDevice));
        }

        private void RaiseSelectionChanged()
        {
            RaisePropertyChanged(nameof(SelectedDevice));
            RaisePropertyChanged(nameof(HasDevice));
        }
    }
}
