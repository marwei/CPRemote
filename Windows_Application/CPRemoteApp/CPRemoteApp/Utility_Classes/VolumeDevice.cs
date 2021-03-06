﻿//CPRemoteApp Namespaces
using CPRemoteApp.ViewController___Remote;
// System Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Windows Namespaces
using Windows.Storage;
using Windows.Storage.Streams;

namespace CPRemoteApp.Utility_Classes
{
    public class VolumeDevice : Device
    {
        public int volume_increments = 1;
        public string volume_up_ir_code = "";
        public string volume_down_ir_code = "";
        public string mute_ir_code = "";
        public VolumeDevice() { }

        public VolumeDevice(string name_, StorageFile input_file) : base(name_, input_file)
        {

        }

        public VolumeDevice(string name_, StorageFile input_file, List<string> device_info) : base(name_, input_file, device_info)
        {
            volume_up_ir_code = device_info[2];
            volume_down_ir_code = device_info[3];
            mute_ir_code = device_info[4];
        }

        // Must return a Task, so that it can be awaited on
        public async Task initialize()
        {
            IList<string> input = await FileIO.ReadLinesAsync(device_info_file);
            int data_lines_read = 0;
            //Read Volume Settings from Input File
            for(int i = 0; i < input.Count; i++)
            {
                if(input[i].ElementAt(0) == '#')
                {
                    continue;
                }
                switch (data_lines_read)
                {
                    case 0 :
                        IR_protocol = input[i];
                        break;
                    case 1 :
                        IR_bits = input[i];
                        break;
                    case 2 :
                        volume_increments = Convert.ToInt32(input[i]);
                        break;
                    case 3 :
                        volume_up_ir_code = input[i];
                        break;
                    case 4 :
                        volume_down_ir_code = input[i];
                        break;
                    case 5 :
                        mute_ir_code = input[i];
                        break;
                }
                data_lines_read++;
            }
            // Create Volume buttons
            createButtons();
        }

        public void createButtons()
        {
            // Create Volume buttons
            string down_name;
            string down_abbv;
            string up_name;
            string up_abbv;
            for (int x = 1; x < 3; x++)
            {
                int reps = x * volume_increments;
                down_name = "Down " + reps.ToString();
                down_abbv = "-" + reps.ToString();
                Uri down_icon_path = new Uri("ms-appx:///img/vol-" + reps.ToString() + ".png");
                RemoteButton down_btn = new RemoteButton(down_name, down_abbv, volume_down_ir_code, reps, down_icon_path);
                up_name = "Up " + reps.ToString();
                up_abbv = "+" + reps.ToString();
                Uri up_icon_path = new Uri("ms-appx:///img/vol+" + reps.ToString() + ".png");
                RemoteButton up_btn = new RemoteButton(up_name, up_abbv, volume_up_ir_code, reps, up_icon_path);
                buttonScanner.add_button(down_btn, false);
                buttonScanner.add_button(up_btn, false);
            }
            string mute_name = "Mute";
            string mute_abbv = "Mute";
            Uri mute_uri = new Uri("ms-appx:///img/volMute.png");
            RemoteButton mute_button = new RemoteButton(mute_name, mute_abbv, mute_ir_code, 1, mute_uri);
            buttonScanner.add_button(mute_button, false);
            is_initialized = true;
            remote_timer.Interval = TimeSpan.FromSeconds(1);
            remote_timer.Tick += setAllowIRTransmission;
        }

        override public async void sendIRInfo()
        {
            string to_send = "-S.";
            to_send += IR_protocol;
            to_send += ".";
            RemoteButton cur_button = buttonScanner.getCurrentButton();
            if (cur_button.abbreviation[0] == '-')
            {
                to_send += volume_down_ir_code;
            }
            else if (cur_button.abbreviation[0] == '+')
            {
                to_send += volume_up_ir_code;
            }
            else
            {
                to_send += mute_ir_code;
            }
            to_send += ".";
            to_send += IR_bits;
            to_send += ".";
            to_send += cur_button.getRepitions();
            to_send += "/";
            App.bm.OperateTVButton_Click(to_send);
            await Task.Delay(TimeSpan.FromSeconds(.1));
        }

        public void setVolumeIncrement(int value)
        {
            volume_increments = value;
            buttonScanner = new RemoteButtonScanner();
            createButtons();
            saveDevice();
        }

        public async void saveDevice()
        {
            List<String> output = new List<string>() { IR_protocol,
                                                        IR_bits,
                                                        volume_increments.ToString(), 
                                                        volume_up_ir_code, 
                                                        volume_down_ir_code,
                                                        mute_ir_code};
            await FileIO.WriteLinesAsync(device_info_file, output);
        }

    }// End of Volume Device Class
}// End of Namespace
