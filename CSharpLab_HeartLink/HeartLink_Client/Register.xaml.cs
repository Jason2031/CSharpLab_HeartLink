using HeartLink_Client.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
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

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace HeartLink_Client
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Register : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private bool flag1 = false, flag2 = false, flag3 = false, flag4 = false, flag5 = false;
        private bool flag11 = false, flag12 = false, flag13 = false, flag14 = false;
        private bool flag21 = false;

        public Register()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 获取此 <see cref="Page"/> 的视图模型。
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。 首次访问页面时，该状态将为 null。</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/></param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, arg) => { arg.Handled = true; };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private bool JudgeIfCanProceed()
        {
            bool flag = flag1 && flag2;
            if (flag3)
                return flag && flag11 && flag12 && flag13 && flag14;
            else
                return flag && flag21;
        }

        private void idTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag1 = (idTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void pswPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            flag2 = (pswPasswordBox.Password.Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        #endregion

        private async void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            //做判断

            Dictionary<string, string> regPair = new Dictionary<string, string>();
            regPair.Add("ID", idTextBox.Text.Trim());
            //TODO:Encryption
            //string encrypted=Encryption.Encrypt(pswPasswordBox.Password);
            string encrypted = pswPasswordBox.Password;
            regPair.Add("PASSWORD",encrypted);
            regPair.Add("NAME", nameTextBox.Text.Trim());
            regPair.Add("STATUS", "OFFLINE");
            if (maleRadioButton.IsChecked == true)
            {
                regPair.Add("GENDER", "Male");
            }
            else
            {
                regPair.Add("GENDER", "Female");
            }
            regPair.Add("CELLPHONE", phonenumTextBox.Text.Trim());
            if (studentRadioButton.IsChecked == true)
            {
                regPair.Add("Type", "Stu");
                regPair.Add("DORMITORY", dormnumTextBox.Text.Trim());
                regPair.Add("CLASSNUMBER", classnumTextBox.Text.Trim());
                regPair.Add("DIRECTORNAME", directorNameTextBox.Text.Trim());
                regPair.Add("DIRECTORCELLPHONE", directorPhonenumTextBox.Text.Trim());
            }
            else
            {
                regPair.Add("Type", "Sup");
                regPair.Add("BUILDING", buildingTextBox.Text.Trim());
            }
            Packet[] packets = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Register), JsonParser.SerializeObject(regPair));
            StaticObj.SendPackets(packets);

            Packet[] incomming = await StaticObj.ReceivePackets();
            int regResult = DataParser.GetPacketCommandCode(incomming[0]);
            if (regResult == Convert.ToInt32(CommandCode.Succeed))
            {
                await (new MessageDialog("注册成功")).ShowAsync();
                //将登录名及加密后的密码写入文件中，以便以后可一键登录
                Dictionary<string, string> remember = new Dictionary<string, string>();
                remember.Add("ID", idTextBox.Text.Trim());
                remember.Add("PASSWORD", encrypted);
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
                Frame.Navigate(typeof(Login));
            }
            else if (regResult==Convert.ToInt32(CommandCode.RegisterFailed))
            {
                await (new MessageDialog("注册失败")).ShowAsync();
            }
            else
            {
                await (new MessageDialog("该用户名已被注册")).ShowAsync();
            }
        }

        private void goBackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }

        private void studentRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            flag3 = true;
            studentInfoPanel.Visibility = Visibility.Visible;
            supervisorInfoPanel.Visibility = Visibility.Collapsed;
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void supervisorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            flag3 = false;
            studentInfoPanel.Visibility = Visibility.Collapsed;
            supervisorInfoPanel.Visibility = Visibility.Visible;
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag4 = (nameTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void phonenumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag5 = (phonenumTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void dormnumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag11 = (dormnumTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void classnumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag12 = (classnumTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void directorNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag13 = (directorNameTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void directorPhonenumTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag14 = (directorPhonenumTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }

        private void buildingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            flag21 = (buildingTextBox.Text.Trim().Length != 0);
            nextStepButton.IsEnabled = JudgeIfCanProceed();
        }
    }
}
