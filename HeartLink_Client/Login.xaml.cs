using System;
using System.Text;
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
using Windows.Storage;
using HeartLink_Lib;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace HeartLink_Client
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Login : Page
    {
        private bool flag1 = false, flag2 = false;
        private DateTime dtBackTimeFirst;
        private DateTime dtBackTimeSecond;
        public Login()
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
            //从本地json文件中获取以往登录的账户名和加密后的密码（若有）
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, arg) => {
                dtBackTimeSecond = System.DateTime.Now;
                TimeSpan ts = dtBackTimeSecond - dtBackTimeFirst;
                if (ts >= new TimeSpan(0, 0, 2)) { 
                    //UIService.Instance.ShowToastPrompt("", "再按一次返回键退出程序 8-)");
                    arg.Handled = true;
                    dtBackTimeFirst = dtBackTimeSecond;
                }
                else
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
            };
            try
            {
                var fold = Windows.Storage.ApplicationData.Current.LocalFolder; //打开文件夹
                StorageFile file = await fold.GetFileAsync("Info.json");    //打开文件
                string result = await FileIO.ReadTextAsync(file); //读取内容
                Dictionary<string, string> remember = JsonParser.DeserializeObject(result);
                idTextBox.Text = remember["ID"];
                //pswPasswordBox.Password = Encryption.Decrypt(remember["PASSWORD"]);
                pswPasswordBox.Password = remember["PASSWORD"];
                if (remember["ISSTUDENT"].Equals("1"))
                {
                    studentRadioButton.IsChecked = true;
                    supervisorRadioButton.IsChecked = false;
                }
                else
                {
                    supervisorRadioButton.IsChecked = true;
                    studentRadioButton.IsChecked = false;
                }
            }
            catch { }
            if (idTextBox.Text.Trim().Length != 0)
            {
                flag1 = true;
                submitButton.IsEnabled = flag1 && flag2;
            }
            if (pswPasswordBox.Password.Length != 0)
            {
                flag2 = true;
                submitButton.IsEnabled = flag1 && flag2;
            }
        }

        private void idTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag1 = (idTextBox.Text.Trim().Length != 0);
            submitButton.IsEnabled = flag1 && flag2;
        }

        private void pswPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            flag2 = (pswPasswordBox.Password.Length != 0);
            submitButton.IsEnabled = flag1 && flag2;
        }

        private async void submitButton_Click(object sender, RoutedEventArgs e)
        {
            //判断以及登录
            string id = idTextBox.Text.Trim();
            string psw = pswPasswordBox.Password;

            if (psw.Length < 4 || psw.Length > 15)
            {
                await (new MessageDialog(String.Format("请确保密码长度在{0}~{1}位之间", Consts.PasswordLength_MIN, Consts.PasswordLength_MAX))).ShowAsync();
            }

            Dictionary<string, string> loginPair = new Dictionary<string, string>();
            loginPair.Add("ID", id);
            loginPair.Add("Password", psw);
            if (studentRadioButton.IsChecked == true)
            {
                loginPair.Add("Type", "Stu");
            }
            else
            {
                loginPair.Add("Type", "Sup");
            }
            Packet[] packets = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Login), JsonParser.SerializeObject(loginPair));
            StaticObj.SendPackets(packets);

            Packet[] incomming = await StaticObj.ReceivePackets();
            int loginResult = DataParser.GetPacketCommandCode(incomming[0]);
            if (loginResult == Convert.ToInt32(CommandCode.Succeed))  //登录成功
            {
                Dictionary<string, string> resultPair = JsonParser.DeserializeObject(DataParser.Packets2Str(incomming));
                if (studentRadioButton.IsChecked == true)
                {
                    StaticObj.user = new Student();
                }
                else
                {
                    StaticObj.user = new Supervisor();
                }
                StaticObj.user.ID = id;
                StaticObj.user.PASSWORD = psw;
                StaticObj.user.NAME = resultPair["NAME"];
                StaticObj.user.GENDER = resultPair["GENDER"];
                if (studentRadioButton.IsChecked == true)
                {
                    (StaticObj.user as Student).DORMITORYADDRESS = resultPair["DORMITORY"];
                    (StaticObj.user as Student).CLASSNUMBER = resultPair["CLASSNUMBER"];
                    (StaticObj.user as Student).DIRECTORNAME = resultPair["DIRECTORNAME"];
                    (StaticObj.user as Student).DIRECTORCELLPHONENUMBER = resultPair["DIRECTORCELLPHONE"];
                }
                else
                {
                    (StaticObj.user as Supervisor).DORMITORYBUILDINGS = resultPair["BUILDING"];
                }

                Dictionary<string, string> remember = new Dictionary<string, string>();
                remember.Add("ID", idTextBox.Text.Trim());
                remember.Add("PASSWORD", pswPasswordBox.Password);
                if (studentRadioButton.IsChecked == true)
                {
                    remember.Add("ISSTUDENT", "1");
                }
                else
                {
                    remember.Add("ISSTUDENT", "0");
                }
                var fold = Windows.Storage.ApplicationData.Current.LocalFolder; //打开文件夹
                StorageFile file = await fold.CreateFileAsync("Info.json", CreationCollisionOption.ReplaceExisting);    //创建文件
                await FileIO.WriteTextAsync(file, JsonParser.SerializeObject(remember));    //写入

                Frame.BackStack.Clear();
                Frame.Navigate(typeof(Main));
            }
            else if (loginResult == Convert.ToInt32(CommandCode.WrongPsw))
            {
                await (new MessageDialog("用户名或密码错误")).ShowAsync();
            }
            else if (loginResult == Convert.ToInt32(CommandCode.AlreadyLogin))
            {
                await (new MessageDialog("该用户已登录，若非本人操作请修改密码")).ShowAsync();
            }
            else
            {
                await (new MessageDialog("该用户名未注册")).ShowAsync();
            }
        }

        private void newUserHyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Register), e.OriginalSource))
            {
                throw new Exception("Failed to create Register page");
            }
        }
    }
}
