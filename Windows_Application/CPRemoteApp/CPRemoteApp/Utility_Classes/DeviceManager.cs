﻿// Systems Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Windows Namespaces
using Windows.Storage;

namespace CPRemoteApp.Utility_Classes
{
    public class DeviceManager
    {
        StorageFolder devices_folder;
        StorageFile devices_info_file;
        List<ChannelDevice> channel_devices;
        List<VolumeDevice> volume_devices;
        public VolumeDevice volumeController { private set; get; }
        public ChannelDevice channelController { private set; get; }
        private bool is_initialized = false;
        public DeviceManager()
        {
            volumeController = new VolumeDevice();
            channelController = new ChannelDevice();
            channel_devices = new List<ChannelDevice>();
            volume_devices = new List<VolumeDevice>();
        }

        public bool isInitialized()
        {
            return is_initialized;
        }

        //Initialize "devices" list and Channel and volumeDevices
        public async Task initialize(StorageFolder devices_folder_)
        {
            is_initialized = true;
            // Device Manager Metadata Initialization 
            devices_folder = devices_folder_;
            devices_info_file = (StorageFile) await devices_folder.TryGetItemAsync("devices_info.txt");
            if(devices_info_file == null)
            {
                System.Diagnostics.Debug.WriteLine("No Devices info file!");
                return;
            }
            IList<string> input = await FileIO.ReadLinesAsync(devices_info_file);
            int num_channel_devices = Convert.ToInt32(input[0]);
            int cur_index = 1;
            String device_name;
            StorageFile cur_device_input_file;

            // Channel Device Initialization

            for(int i = 0; i < num_channel_devices; ++i)
            {
                device_name = input[cur_index++];
                try
                {
                    cur_device_input_file = (StorageFile)await get_input_file_from_name(device_name, 'c');
                    ChannelDevice c_device = new ChannelDevice(device_name, cur_device_input_file);
                    channel_devices.Add(c_device);
                }
                catch(Exception except)
                {
                    System.Diagnostics.Debug.WriteLine(except.Message);
                }
            }
            // Get Current Channel Device
            int chan_index = 0;
            bool found = false;
            string cur_chan_device_name = input[cur_index++];
            foreach(ChannelDevice cur in channel_devices)
            {
                if(cur.get_name().Equals(cur_chan_device_name))
                {
                    found = true;
                    break;
                }
                chan_index++;
            }
            if (!found)
            {
                // Throw Exception about channel device not found
            }
            else
            {
                channelController = channel_devices[chan_index];
                await channelController.initialize();
            }

            // Volume Device Initialization

            int num_vol_devices = Convert.ToInt32(input[cur_index++]);
            for(int i = 0; i < num_vol_devices; ++i)
            {
                device_name = input[cur_index++];
                cur_device_input_file = await get_input_file_from_name(device_name, 'v');
                VolumeDevice v_device = new VolumeDevice(device_name, cur_device_input_file);
                volume_devices.Add(v_device);
            }
            int vol_index = 0;
            string cur_vol_device_name = input[cur_index++];
            found = false;
            foreach(VolumeDevice cur_v in volume_devices)
            {
                if(cur_v.get_name().Equals(cur_vol_device_name))
                {
                    found = true;
                    break;
                }
                vol_index++;
            }
            if(!found)
            {
                // TODO: Throw Error
            }
            else
            {
                volumeController = volume_devices[vol_index];
                await volumeController.initialize();
            }
      
        }

        public async void saveDeviceList()
        {
            // # of channel devices, name of each channel device, cur channel device, 
            StorageFile info_file = await devices_folder.CreateFileAsync("devices_info.txt", CreationCollisionOption.ReplaceExisting);
            List<string> save_output = new List<string>();
            save_output.Add(channel_devices.Count.ToString());
            foreach(ChannelDevice cur_dev in channel_devices)
            {
                save_output.Add(cur_dev.get_name());
            }
            save_output.Add(channelController.get_name());
            //# of vol devices, name of vol devices, cur vol device
            save_output.Add(volume_devices.Count.ToString());
            foreach(VolumeDevice cur_dev in volume_devices)
            {
                save_output.Add(cur_dev.get_name());
            }
            save_output.Add(volumeController.get_name());
            await FileIO.WriteLinesAsync(info_file, save_output);
            devices_info_file = info_file;
        }

        public async void addChannelDevice(string name, List<string> IR_info)
        {
            string i_file_name = get_input_file_name(name, 'c');
            StorageFile chan_file = await devices_folder.CreateFileAsync(i_file_name, CreationCollisionOption.ReplaceExisting);
            ChannelDevice channel_dev = new ChannelDevice(name, chan_file, IR_info);
            channel_devices.Add(channel_dev);
            channel_dev.saveDevice();
            channelController = channel_dev;
            saveDeviceList();
        }

        public async void addVolumeDevice(string name, List<string> IR_info)
        {
            string i_file_name = get_input_file_name(name, 'v');
            StorageFile vol_file = await devices_folder.CreateFileAsync(i_file_name, CreationCollisionOption.ReplaceExisting);
            VolumeDevice vol_dev = new VolumeDevice(name, vol_file, IR_info);
            volume_devices.Add(vol_dev);
            vol_dev.saveDevice();
            volumeController = vol_dev;
            volumeController.createButtons();
            saveDeviceList();
        }

        public void removeChannelDevice(string name)
        {
            // TODO: Delete the File from the devices folder
            if(name == channelController.get_name())
            {
                //throw new Exception("Cannot Delete the currently selected device");
                return;
            }
            int index = getChannelDeviceIndex(ref name);
            if(index == -1)
            {
                return;
            }
            channel_devices.RemoveAt(index);
            saveDeviceList();
        }

        public void removeVolumeDevice(string name)
        {
            if(name == volumeController.get_name())
            {
                //throw new Exception("Cannot Delete the currently selected device");
                return;
            }
            int index = getVolumeDeviceIndex(ref name);
            if(index == -1)
            {
                return;
            }
            volume_devices.RemoveAt(index);
            saveDeviceList();
        }

        public async Task<bool> selectChannelDevice(string name)
        {
            int index = getChannelDeviceIndex(ref name);
            if(index == -1)
            {
                return false;
            }
            channelController = channel_devices[index];
            if (!channelController.is_initialized)
            {
                await channelController.initialize();
            }
            saveDeviceList();
            return true;
        }

        public async Task selectVolumeDevice(string name)
        {
            int index = getVolumeDeviceIndex(ref name);
            if (index == -1)
            {
                return;
            }
            volumeController = volume_devices[index];
            await volumeController.initialize();
            saveDeviceList();
        }

        private int getVolumeDeviceIndex(ref string name)
        {
            int vol_index = 0;
            foreach (VolumeDevice cur in volume_devices)
            {
                if (cur.get_name().Equals(name))
                {
                    return vol_index;
                }
                vol_index++;
            }
            return -1;
        }

        private int getChannelDeviceIndex(ref string name)
        {
            int chan_index = 0;
            foreach (ChannelDevice cur in channel_devices)
            {
                if (cur.get_name().Equals(name))
                {
                    return chan_index;
                }
                chan_index++;
            }
            return -1;
        }

        public List<VolumeDevice> getVolumeDevices()
        {
            return volume_devices;
        }

        public List<ChannelDevice> getChannelDevices()
        {
            return channel_devices;
        }

        private string get_input_file_name(string name, char postfix)
        {
            string f_name = name.Replace(" ", "_");
            f_name += "_" + postfix + ".txt";
            return f_name;
        }

        public async Task<bool> device_input_file_exists(string dev_name, bool channel_or_volume)
        {
            char postfix;
            if(channel_or_volume)
            {
                postfix = 'v';
            }
            else
            {
                postfix = 'c';
            }
            string file_name = get_input_file_name(dev_name, postfix);
            StorageFile input_file = (StorageFile) await devices_folder.TryGetItemAsync(file_name);
            bool r_val;
            if (input_file.IsEqual(null))
            {
                r_val = false;
            }
            else
            {
                r_val = true;
            }
            return r_val;
        }

        private async Task<StorageFile> get_input_file_from_name(string name, char postfix)
        {
            name = get_input_file_name(name, postfix);
            StorageFile input_file = (StorageFile) await devices_folder.TryGetItemAsync(name);
            return input_file;
        }

    }
}
