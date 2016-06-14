using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
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
    public class DailyReminderDisplay
    {
        public string ReminderTime { get; set; }
        public string ReminderContent { get; set; }
    }
    public class MessageBoxContent : INotifyPropertyChanged    //留言板展示项
    {
        private string time;
        private string type;
        private string id;
        private string content;

        public string Content
        {
            get { return content; }
            set
            {
                if (content != value)
                    content = value;
                NotifyPropertyChanged("Content");
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

        public string Type
        {
            get { return type; }
            set
            {
                if (type != value)
                    type = value;
                NotifyPropertyChanged("Type");
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

    public class ContactorContent : INotifyPropertyChanged    //联系人展示项
    {
        private string name;
        private string phonenum;

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                    name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string PhoneNum
        {
            get { return phonenum; }
            set
            {
                if (phonenum != value)
                    phonenum = value;
                NotifyPropertyChanged("PhoneNum");
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

    public class ContentCollection<T> : ObservableCollection<T>
    {
        public ContentCollection() : base() { }
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Main : Page
    {
        public ContentCollection<ContactorContent> Contacters { get; set; }
        public ContentCollection<MessageBoxContent> MessageBox { get; set; }
        public string ReminderTime { get; set; }
        public string ReminderContent { get; set; }

        private ListView MessageBoardList = null;
        private ListView ContacterList = null;
        public Main()
        {
            this.InitializeComponent();

            ReminderTime = DateTime.Now.ToString();
            ReminderContent = "Default";
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, arg) => { arg.Handled = true; };
            Contacters = new ContentCollection<ContactorContent>();
            MessageBox = new ContentCollection<MessageBoxContent>();

            #region GetContacters
            Dictionary<string, string> commandPair = new Dictionary<string, string>();
            commandPair.Add("ID", StaticObj.user.ID);
            Packet[] packets = DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetContacters), JsonParser.SerializeObject(commandPair));
            StaticObj.SendPackets(packets);

            Packet[] incommingContacters = await StaticObj.ReceivePackets();

            if (DataParser.GetPacketCommandCode(incommingContacters[0]) == Convert.ToInt32(CommandCode.ReturnContacters))
            {
                List<Dictionary<string, string>> jsonContacters = JsonParser.DeserializeListOfDictionaryObject(DataParser.Packets2Str(incommingContacters));

                foreach (Dictionary<string, string> con in jsonContacters)
                {
                    ContactorContent listContent = new ContactorContent();
                    listContent.Name = con["NAME"];
                    listContent.PhoneNum = con["CELLPHONE"];
                    Contacters.Add(listContent);
                }
            }
            #endregion

            #region GetMessageBox
            packets = DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetMessageBox), "");
            StaticObj.SendPackets(packets);

            Packet[] incommingMessageBox = await StaticObj.ReceivePackets();

            if (DataParser.GetPacketCommandCode(incommingMessageBox[0]) == Convert.ToInt32(CommandCode.ReturnMessageBox))
            {
                List<Dictionary<string, string>> jsonMessageBox = JsonParser.DeserializeListOfDictionaryObject(DataParser.Packets2Str(incommingMessageBox));

                foreach (Dictionary<string, string> msg in jsonMessageBox)
                {
                    MessageBoxContent listContent = new MessageBoxContent();
                    listContent.ID = msg["ID"];
                    if (msg["ISFROMSTUDENT"].Equals("1"))
                    {
                        listContent.Type = "Student";
                    }
                    else
                    {
                        listContent.Type = "Supervisor";
                    }
                    //listContent.Type = msg["TYPE"];
                    listContent.Time = msg["SENDTIME"];
                    listContent.Content = msg["CONTENT"];
                    MessageBox.Add(listContent);
                }
            }
            #endregion

            if (StaticObj.user is HeartLink_Lib.Student)
            {
                WorkServiceForStudent.Visibility = Visibility.Visible;
                WorkServiceForSupervisor.Visibility = Visibility.Collapsed;

                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetDailyReminder), ""));

                Packet[] incommingDailyReminder = await StaticObj.ReceivePackets();
                DailyReminderDisplay dis = new DailyReminderDisplay();
                if (DataParser.GetPacketCommandCode(incommingDailyReminder[0]) == Convert.ToInt32(CommandCode.ReturnDailyReminder))
                {
                    Dictionary<string, string> dailyReminder = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingDailyReminder));
                    dis.ReminderTime = dailyReminder["REMINDTIME"];
                    dis.ReminderContent = dailyReminder["CONTENT"];
                }
                WorkServiceForStudent.DataContext = dis;
            }
            else
            {
                WorkServiceForSupervisor.Visibility = Visibility.Visible;
                WorkServiceForStudent.Visibility = Visibility.Collapsed;
            }
        }


        private void ContacterClick(object sender, ItemClickEventArgs e)
        {
            //Button btn = FindChildControl<Button>(ContactersHub, "CallBtn") as Button;
            //btn.IsEnabled = true;
            //ListView l = FindChildControl<ListView>(ContactersHub, "ContactersDisplay") as ListView;
            //ContactorContent item = (ContactorContent)l.SelectedItem;

            //Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI(item.PhoneNum, item.Name);
        }

        private async void MessageBoard_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageBoxContent item=(MessageBoxContent)e.ClickedItem;
            await (new MessageDialog(String.Format("{0} {1}\n{2}\n{3}",item.Type,item.ID,item.Time,item.Content))).ShowAsync();
        }

        private void DailyReminder(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(DailyReminder), e.OriginalSource))
            {
                throw new Exception("Failed to create DailyReminder page");
            }
        }

        private void Inspection(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Inspection), e.OriginalSource))
            {
                throw new Exception("Failed to create Inspection page");
            }
        }

        private void LifeServices_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch (((Grid)e.ClickedItem).Name)
            {
                case "RepairBtn":
                    if (!Frame.Navigate(typeof(Repair), e.OriginalSource))
                    {
                        throw new Exception("Failed to create Repair page");
                    }
                    break;
                case "AskoffBtn":
                    if (!Frame.Navigate(typeof(Askoff), e.OriginalSource))
                    {
                        throw new Exception("Failed to create Askoff page");
                    }
                    break;
                case "DeliveryBtn":
                    if (!Frame.Navigate(typeof(Delivery), e.OriginalSource))
                    {
                        throw new Exception("Failed to create Delivery page");
                    }
                    break;
                case "WaterBtn":
                    if (!Frame.Navigate(typeof(Water), e.OriginalSource))
                    {
                        throw new Exception("Failed to create Water page");
                    }
                    break;
            }
        }

        private void SetupBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(DetailSettingPage), e.OriginalSource))
            {
                throw new Exception("Failed to create DetailSettingPage page");
            }
        }

        private async void UploadLocation(object sender, RoutedEventArgs e)
        {
            Geolocator geolocator = new Geolocator();
            // 期望的精度级别（PositionAccuracy.Default 或 PositionAccuracy.High）
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            // 期望的数据精度（米）
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync();
                Dictionary<string, string> locationPair = new Dictionary<string, string>();
                locationPair.Add("ID", StaticObj.user.ID);
                locationPair.Add("LONGITUDE", geoposition.Coordinate.Point.Position.Longitude.ToString("0.000"));
                locationPair.Add("LATITUDE", geoposition.Coordinate.Point.Position.Latitude.ToString("0.000"));
                locationPair.Add("ALTITUDE", geoposition.Coordinate.Point.Position.Altitude.ToString("0.000"));
                locationPair.Add("TIME", DateTime.Now.ToString());
                locationPair.Add("POSITIONSOURCE", geoposition.Coordinate.PositionSource.ToString());

                StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.SendLocation), JsonParser.SerializeObject(locationPair)));
                Packet[] incomming = await StaticObj.ReceivePackets();

                if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnLocation))
                {
                    if (Convert.ToInt32(DataParser.Packets2Str(incomming)) != 0)
                    {
                        await (new MessageDialog(String.Format("发送成功！\n时间：{0}\n经度：{1}\n纬度：{2}海拔：{3}\n位置信息来源：{4}", locationPair["TIME"], locationPair["LONGITUDE"], locationPair["LATITUDE"], locationPair["ALTITUDE"], locationPair["POSITIONSOURCE"]))).ShowAsync();
                    }
                }
                else
                {
                    await (new MessageDialog("上传失败！")).ShowAsync();
                }
            }
            catch { }
        }

        private void AddContacter(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(AddContacter), e.OriginalSource))
            {
                throw new Exception("Failed to create AddContacter page");
            }
        }

        private void WriteMessage(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(WriteMessage), e.OriginalSource))
            {
                throw new Exception("Failed to create WriteMessage page");
            }
        }

        private DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }
                else
                {
                    // Not found it - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return null;
        }

        private void ContactersList_LostFocus(object sender, RoutedEventArgs e)
        {
            //Button btn = FindChildControl<Button>(ContactersHub, "CallBtn") as Button;
            //btn.IsEnabled = false;
        }

        private void Exit(object sender, RoutedEventArgs e)
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
            StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.Exit), JsonParser.SerializeObject(logoutPair)));

            Application.Current.Exit();
        }

        private void Call(object sender, RoutedEventArgs e)
        {
            ListView l = FindChildControl<ListView>(ContactersHub, "ContactersDisplay") as ListView;
            ContactorContent item = (ContactorContent)l.SelectedItem;

            Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI(item.PhoneNum, item.Name);
        }

        private void MessageBoardDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBoardList = FindChildControl<ListView>(MessageBoard, "MessageBoardDisplay") as ListView;
            try
            {
                MessageBoardList.ItemsSource = MessageBox;
            }
            catch { }
        }

        private void ContactersDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            ContacterList = FindChildControl<ListView>(ContactersHub, "ContactersDisplay") as ListView;
            try
            {
                ContacterList.ItemsSource = Contacters;
            }
            catch { }
        }

    }
}
