﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CPRemoteApp.ViewController___Settings
{
  public sealed partial class ChannelList : UserControl
  {
    public ChannelList()
    {
      this.InitializeComponent();
      _channel_name.Text = "name";
    }

    public ChannelList(string name)
    {
      this.InitializeComponent();

      _channel_name.Text = name;

    }

    private void pointerMoved(object sender, object e)
    {
      _channel_canvas.Background = new SolidColorBrush(Windows.UI.Colors.OrangeRed);

    }
    private void pointerExited(object sender, object e)
    {
      _channel_canvas.Background = new SolidColorBrush(Windows.UI.Colors.Black);
    }

  }
}
