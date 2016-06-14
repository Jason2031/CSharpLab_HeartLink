using System;
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
using Windows.UI.Popups;
using HeartLink_Lib;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace HeartLink_Client
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AddContacter : Page
    {
        public AddContacter()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, arg) => { arg.Handled = true; };
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> contacterPair = new Dictionary<string, string>();
            contacterPair.Add("OWNERID", StaticObj.user.ID);
            if (student.IsChecked == true)
            {
                contacterPair.Add("ISFORSTUDENT", "1");
            }
            else
            {
                contacterPair.Add("ISFORSTUDENT", "0");
            }
            contacterPair.Add("CONTACTORID", ID.Text.Trim());
            StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SendContacter), JsonParser.SerializeObject(contacterPair)));

            Packet[] incomming = await StaticObj.ReceivePackets();
            if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnContacters))
            {
                if (Convert.ToInt32(DataParser.Packets2Str(incomming)) != 0)
                {
                    await (new MessageDialog("发送成功！")).ShowAsync();
                    Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
                    Frame.Navigate(typeof(Main));
                }
                else
                {
                    await (new MessageDialog("上传失败！")).ShowAsync();
                }
            }
        }

        private void ID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ID.Text.Trim().Length > 0)
                confirm.IsEnabled = true;
            else
                confirm.IsEnabled = false;
        }
    }
}
