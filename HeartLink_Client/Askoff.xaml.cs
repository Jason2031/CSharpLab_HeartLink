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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using HeartLink_Lib;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace HeartLink_Client
{

    public class AskOffContent : INotifyPropertyChanged    //请假条展示项
    {
        private string time;
        private string date;
        private string id;
        private string reason;

        public string Reason
        {
            get { return reason; }
            set
            {
                if (reason != value)
                    reason = value;
                NotifyPropertyChanged("Reason");
            }
        }

        public string Time
        {
            get { return time; }
            set
            {
                if (time != value)
                    time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public string ID
        {
            get { return id; }
            set
            {
                if (id != value)
                    id = value;
                NotifyPropertyChanged("ID");
            }
        }

        public string Date
        {
            get { return date; }
            set
            {
                if (date != value)
                    date = value;
                NotifyPropertyChanged("Date");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Askoff : Page
    {

        public ContentCollection<AskOffContent> AskOffList { get; set; }

        public Askoff()
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
            if (StaticObj.user is HeartLink_Lib.Student)
            {
                gridForStudent.Visibility = Visibility.Visible;
                gridForSupervisor.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridForSupervisor.Visibility = Visibility.Visible;
                gridForStudent.Visibility = Visibility.Collapsed;
                AskOffList = new ContentCollection<AskOffContent>();
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetAskOff), ""));

                Packet[] incommingAskOff = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incommingAskOff[0]) == Convert.ToInt32(CommandCode.ReturnAskOff))
                {
                    List<Dictionary<string, string>> jsonAskOffs = JsonParser.DeserializeListOfDictionaryObject(DataParser.Packets2Str(incommingAskOff));

                    foreach (Dictionary<string, string> con in jsonAskOffs)
                    {
                        AskOffContent temp = new AskOffContent();
                        temp.Reason = con["REASON"];
                        temp.ID = con["ID"];
                        temp.Date = con["ASKOFFDATE"];
                        temp.Time = con["ASKOFFTIME"];
                        AskOffList.Add(temp);
                    }

                    display.ItemsSource = AskOffList;
                }
            }
        }


        private async void confirm_Click(object sender, RoutedEventArgs e)
        {
            #region Stu
            if (StaticObj.user is Student)
            {
                Dictionary<string, string> askoffPair = new Dictionary<string, string>();
                askoffPair.Add("ID", StaticObj.user.ID);
                askoffPair.Add("ASKOFFDATE", date.Date.ToString().Split(new char[]{' '})[0]);
                askoffPair.Add("ASKOFFTIME", String.Format("{0}~{1}", fromTime.Time.ToString().Substring(0, 5), toTime.Time.ToString().Substring(0, 5)));
                askoffPair.Add("REASON", reasonTxtBox.Text.Trim());
                askoffPair.Add("ISSOLVED", "0");
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SendAskOff), JsonParser.SerializeObject(askoffPair)));

                Packet[] incommingAskOff = await StaticObj.ReceivePackets();
                if (DataParser.GetPacketCommandCode(incommingAskOff[0]) == Convert.ToInt32(CommandCode.ReturnAskOff))
                {
                    if (Convert.ToInt32(DataParser.Packets2Str(incommingAskOff)) != 0)
                    {
                        await (new MessageDialog("已上传")).ShowAsync();
                        Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
                        Frame.Navigate(typeof(Main));
                    }
                    else
                    {
                        await (new MessageDialog("上传失败")).ShowAsync();
                    }
                }
            }
            #endregion
            #region Sup
            else
            {
                AskOffContent selected = (AskOffContent)display.SelectedItem;
                Dictionary<string, string> solvePair = new Dictionary<string, string>();
                solvePair.Add("ID", selected.ID);
                solvePair.Add("ASKOFFDATE", selected.Date);
                solvePair.Add("ASKOFFTIME", selected.Time);
                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SolveAskOff), JsonParser.SerializeObject(solvePair)));

                Packet[] incomming = await StaticObj.ReceivePackets();
                int solveResult = DataParser.GetPacketCommandCode(incomming[0]);
                if (solveResult == Convert.ToInt32(CommandCode.Succeed))
                {
                    await (new MessageDialog("已解决")).ShowAsync();
                    display.Items.RemoveAt(display.SelectedIndex);
                }
                else
                {
                    await (new MessageDialog("错误")).ShowAsync();
                }
            }
            #endregion
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }

        private void Changed(object sender, TextChangedEventArgs e)
        {
            if (reasonTxtBox.Text.Trim().Length > 0)
            {
                confirm.IsEnabled = true;
                m_BtnSolve.IsEnabled = true;
            }
            else
            {
                confirm.IsEnabled = false;
                m_BtnSolve.IsEnabled = false;
            }
        }
    }
}
