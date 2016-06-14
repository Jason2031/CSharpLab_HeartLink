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
    public sealed partial class Delivery : Page
    {
        public Delivery()
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
            #region Stu
            if (StaticObj.user is HeartLink_Lib.Student)
            {
                gridForSupervisor.Visibility = Visibility.Collapsed;
                gridForStudent.Visibility = Visibility.Visible;
                Dictionary<string, string> deliveryPair = new Dictionary<string, string>();
                deliveryPair.Add("ID", StaticObj.user.ID);
                deliveryPair.Add("ISSOLVED", "0");
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetDelivery), JsonParser.SerializeObject(deliveryPair)));

                Packet[] incomming = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnDelivery))
                {
                    List<Dictionary<string, string>> deliveryList = JsonParser.DeserializeListOfDictionaryObject(DataParser.Packets2Str(incomming));
                    foreach (Dictionary<string, string> temp in deliveryList)
                    {
                        display.Items.Add(temp["CONTENT"]);
                    }
                }
                else
                {
                    await (new MessageDialog("获得失败")).ShowAsync();
                }
            }
            #endregion
            #region Sup
            else
            {
                gridForStudent.Visibility = Visibility.Collapsed;
                gridForSupervisor.Visibility = Visibility.Visible;
            }
            #endregion
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            #region Stu
            if (StaticObj.user is Student)
            {
                string selected = (string)display.SelectedItem;
                Dictionary<string, string> deliveryPair = new Dictionary<string, string>();
                deliveryPair.Add("ID", StaticObj.user.ID);
                deliveryPair.Add("CONTENT", selected);
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SolveDelivery),JsonParser.SerializeObject(deliveryPair)));

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
            #region Sup
            else
            {
                Dictionary<string, string> deliveryPair = new Dictionary<string, string>();
                deliveryPair.Add("ID", ID.Text.Trim());
                deliveryPair.Add("CONTENT", content.Text.Trim());
                deliveryPair.Add("ISSOLVED", "0");
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SendDelivery), JsonParser.SerializeObject(deliveryPair)));

                Packet[] incomming = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnDelivery))
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
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }
    }
}
