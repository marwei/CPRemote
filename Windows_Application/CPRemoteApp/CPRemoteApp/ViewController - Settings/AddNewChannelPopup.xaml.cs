﻿using CPRemoteApp.Utility_Classes;
using CPRemoteApp.ViewController___Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CPRemoteApp.ViewController___Settings
{
  public sealed partial class AddNewChannelPopup : UserControl
  {
    private WeakReference<Popup> popup_ref;
    public event ChangedEventHander savePressed;

    public AddNewChannelPopup()
    {
      this.InitializeComponent();
    }

    public AddNewChannelPopup(string name, string channel_num, Uri img_uri)
    {
      this.InitializeComponent();
      _ch_name.Text = name;
      _ch_num.Text = channel_num;
      ImageSource imgSource = new BitmapImage(img_uri);
      _img.Source = imgSource; 
       
    }

    public void setParentPopup(ref Popup p)
    {
        popup_ref = new WeakReference<Popup>(p);
    }

    public void closePopup(object sender, RoutedEventArgs e)
    {
        Popup pop;
        popup_ref.TryGetTarget(out pop);
        pop.IsOpen = false;
    }

    private void saveClicked(object sender, object e)
    {
      //
      // CHECK FOR CHANNEL NAME DUPLICATES.
      //
      ChannelDevice c = ((App)(CPRemoteApp.App.Current)).deviceController.channelController;
      foreach(RemoteButton b in c.buttonScanner.getButtons())
      {
        if(b.getName().ToLower() == _ch_name.Text.ToLower())
        {
          MessageDialog msgDialog = new MessageDialog("There is already a channel saved with that name! Please enter a unique name for the channel!", "Whoops!");
          UICommand okBtn = new UICommand("OK");
          okBtn.Invoked += delegate { };
          msgDialog.Commands.Add(okBtn);
          msgDialog.ShowAsync();
          return;
        }
      }

      //
      // CHECK THAT NUMBER IS A NUMBER
      //
      foreach(char a in _ch_num.Text)
      {
        string s = "1234567890";
        if(!s.Contains(a))
        {
          MessageDialog msgDialog = new MessageDialog("The number for the channel can only contain numbers! Please enter a positive integer for the channel number!", "Whoops!");
          UICommand okBtn = new UICommand("OK");
          okBtn.Invoked += delegate { };
          msgDialog.Commands.Add(okBtn);
          msgDialog.ShowAsync();
          return;
        }
      }


      if (_ch_name.Text != "" && _ch_num.Text != "")
      {
        this._save_button.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        if (savePressed != null) savePressed.Invoke(this, EventArgs.Empty);

        BitmapImage bi = _img.Source as BitmapImage;
        Uri uri; 
        if (bi == null)
        {
            uri = new Uri("ms-appx:///img/unset.png"); 
        }
        else
        {
            uri = bi.UriSource;

        }


        RemoteButton b = new RemoteButton(_ch_name.Text, _ch_name.Text, _ch_num.Text, 1, uri);
        ((App)CPRemoteApp.App.Current).deviceController.channelController.add_channel(b);
        closePopup(null, null);
      }
    }

    async private void uploadClicked(object sender, RoutedEventArgs e)
    {
        FileOpenPicker openPicker = new FileOpenPicker();
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        openPicker.FileTypeFilter.Add(".jpg");
        openPicker.FileTypeFilter.Add(".jpeg");
        openPicker.FileTypeFilter.Add(".png");

        StorageFile file = await openPicker.PickSingleFileAsync();
        Popup pop;


        if (file != null)
        {
            StorageFolder img_folder = await  App.appData.LocalFolder.CreateFolderAsync("images_folder", CreationCollisionOption.OpenIfExists);
            if (await IfStorageItemExist(img_folder, file.Name))
            {
                MessageDialog msgDialog = new MessageDialog("Image with same name already exists. Please rename the image and try again!", "Whoops!");
                UICommand okBtn = new UICommand("OK");
                msgDialog.Commands.Add(okBtn);
                await msgDialog.ShowAsync();
                popup_ref.TryGetTarget(out pop);
                pop.IsOpen = true; 
      
            }
            else
            {
                // file/folder does not exist. 
               await file.CopyAsync(img_folder);
               popup_ref.TryGetTarget(out pop);
               Uri uri = new Uri(App.appData.LocalFolder.Path + "\\" + "images_folder" + "\\" + file.Name);
            

               ImageSource imgSource = new BitmapImage(uri);
                _img.Source = imgSource; 
               pop.IsOpen = true; 

            } 
 
        }
 

    }

    public async Task<bool> IfStorageItemExist(StorageFolder folder, string itemName)
    {
        try
        {
            IStorageItem item = await folder.TryGetItemAsync(itemName);
            return (item != null);
        }
        catch (Exception ex)
        {
            // Should never get here 
            return false;
        }
    } 


  }
}
