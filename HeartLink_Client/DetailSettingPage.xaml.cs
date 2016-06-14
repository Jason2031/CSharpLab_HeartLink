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
    public sealed partial class DetailSettingPage : Page
    {
        public DetailSettingPage()
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
            ID.Text = StaticObj.user.ID;
            UserName.Text = StaticObj.user.NAME;
            if (StaticObj.user.GENDER == "Male")
            {
                Gender.Text = "男";
            }
            else
            {
                Gender.Text = "女";
            }
            PhoneNum.Text = StaticObj.user.CELLPHONENUMBER;
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> logoutPair = new Dictionary<string, string>();
            if (StaticObj.user is Student)
            {
                logoutPair.Add("Type", "Stu");
            }
            else
            {
                logoutPair.Add("Type", "Sup");
            }
            logoutPair.Add("ID", StaticObj.user.ID);
            StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.Logout), JsonParser.SerializeObject(logoutPair)));

            Packet[] incomming = await StaticObj.ReceivePackets();
            if (Convert.ToInt32(DataParser.GetPacketCommandCode(incomming[0])) == Convert.ToInt32(CommandCode.Logout))
            {
                if (Convert.ToInt32(DataParser.Packets2Str(incomming)) != 0)
                {
                    await (new MessageDialog("已注销")).ShowAsync();
                    Frame.BackStack.Clear();
                    Frame.Navigate(typeof(Login));
                }
                else
                {
                    await (new MessageDialog("注销失败")).ShowAsync();
                }
            }

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
