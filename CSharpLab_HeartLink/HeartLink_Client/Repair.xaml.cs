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
    public sealed partial class Repair : Page
    {
        public Repair()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, arg) => { arg.Handled = true; };
            if (StaticObj.user is Student)
            {
                forStudent.Visibility = Visibility.Visible;
                forSupervisor.Visibility = Visibility.Collapsed;
            }
            else
            {
                forSupervisor.Visibility = Visibility.Visible;
                forStudent.Visibility = Visibility.Collapsed;
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetRepair),""));

                Packet[] incomming = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnRepair))
                {
                    List<Dictionary<string, string>> repairList = JsonParser.DeserializeListOfDictionaryObject(DataParser.Packets2Str(incomming));
                    foreach (Dictionary<string, string> temp in repairList)
                    {
                        display.Items.Add(temp["CONTENT"]);
                    }
                }
                else
                {
                    await (new MessageDialog("获得失败")).ShowAsync();
                }
            }
        }


        private void RequestTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RequestTextBox.Text.Trim().Length > 0)
                ConfirmBtn.IsEnabled = true;
            else
                ConfirmBtn.IsEnabled = false;
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            #region Stu
            if (StaticObj.user is Student)
            {
                Dictionary<string, string> repairPair = new Dictionary<string, string>();
                repairPair.Add("students_ID", StaticObj.user.ID);
                repairPair.Add("CONTENT", RequestTextBox.Text.Trim());
                repairPair.Add("ISSOLVED","0");
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SendRepair), JsonParser.SerializeObject(repairPair)));

                Packet[] incomming = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnRepair))
                {
                    if (Convert.ToInt32(DataParser.Packets2Str(incomming)) != 0)
                    {
                        await (new MessageDialog("发送成功")).ShowAsync();
                        Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
                        Frame.Navigate(typeof(Main));
                    }
                    else
                    {
                        await (new MessageDialog("操作失败")).ShowAsync();
                    }
                }
            }
            #endregion
            #region Sup
            else
            {
                string selected = (string)display.SelectedItem;
                Dictionary<string, string> repairPair = new Dictionary<string, string>();
                repairPair.Add("students_ID", StaticObj.user.ID);
                repairPair.Add("CONTENT", selected);
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SolveRepair), JsonParser.SerializeObject(repairPair)));

                Packet[] incomming = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.Succeed))
                {
                    await (new MessageDialog("已完成")).ShowAsync();
                    display.Items.Remove(selected);
                }
                else
                {
                    await (new MessageDialog("操作失败")).ShowAsync();
                }
            }
            #endregion
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }
    }
}
