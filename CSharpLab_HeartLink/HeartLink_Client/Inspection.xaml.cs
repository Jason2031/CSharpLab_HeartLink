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
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;
using System.ComponentModel;
using HeartLink_Lib;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace HeartLink_Client
{

    public class LocationContent : INotifyPropertyChanged
    {
        private string id;
        private string time;
        private string positionsource;
        private string distance;
        private string azimuth;

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

        public string PositionSource
        {
            get { return positionsource; }
            set
            {
                if (positionsource != value)
                    positionsource = value;
                NotifyPropertyChanged("PositionSource");
            }
        }

        public string Distance
        {
            get { return distance; }
            set
            {
                if (distance != value)
                    distance = value;
                NotifyPropertyChanged("Distance");
            }
        }

        public string Azimuth
        {
            get { return azimuth; }
            set
            {
                if (azimuth != value)
                    azimuth = value;
                NotifyPropertyChanged("Azimuth");
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
    public sealed partial class Inspection : Page
    {
        ContentCollection<LocationContent> locationBox;
        private const double EARTH_RADIUS = 6378.137; //地球半径
        public Inspection()
        {
            this.InitializeComponent();
            ProgressBar.Height = MapControl.ActualHeight;
            ProgressBar.Width = MapControl.ActualWidth;
            locationBox = new ContentCollection<LocationContent>();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (sender, arg) => { arg.Handled = true; };

            locationBox.Clear();

            Geolocator geolocator = new Geolocator();
            Geoposition geoposition = null;
            BasicGeoposition supervisorPosition = new BasicGeoposition();
            try
            {
                geoposition = await geolocator.GetGeopositionAsync();
                supervisorPosition.Longitude = geoposition.Coordinate.Point.Position.Longitude;
                supervisorPosition.Latitude = geoposition.Coordinate.Point.Position.Latitude;
                supervisorPosition.Altitude = geoposition.Coordinate.Point.Position.Altitude;

                MapIcon supervisorIcon = new MapIcon();
                supervisorIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/PinkPushPin.png"));
                supervisorIcon.NormalizedAnchorPoint = new Point(0.25, 0.9);
                supervisorIcon.Location = new Geopoint(supervisorPosition);

                MapControl.Center = geoposition.Coordinate.Point;
            }
            catch { }

            StaticObj.SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.GetLocation), ""));

            Packet[] incomming = await StaticObj.ReceivePackets();
            if (DataParser.GetPacketCommandCode(incomming[0]) == Convert.ToInt32(CommandCode.ReturnLocation))
            {
                List<Dictionary<string, string>> locationDict = JsonParser.DeserializeListOfDictionaryObject(DataParser.Packets2Str(incomming));
                foreach (Dictionary<string, string> dic in locationDict)
                {
                    BasicGeoposition position = new BasicGeoposition();
                    position.Longitude = Convert.ToDouble(dic["LONGITUDE"]);
                    position.Latitude = Convert.ToDouble(dic["LATITUDE"]);
                    position.Altitude = Convert.ToDouble(dic["ALTITUDE"]);
                    
                    MapIcon studentIcon = new MapIcon();
                    studentIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/PinkPushPin.png"));
                    studentIcon.NormalizedAnchorPoint = new Point(0.25, 0.9);
                    studentIcon.Location = new Geopoint(position);

                    MapControl.MapElements.Add(studentIcon);

                    LocationContent locationContent = new LocationContent();
                    locationContent.ID = dic["ID"];
                    locationContent.Time = dic["TIME"];
                    locationContent.PositionSource = dic["POSITIONSOURCE"];
                    double distance = GetDistance(geoposition.Coordinate.Point.Position.Latitude, geoposition.Coordinate.Point.Position.Longitude,
                        Convert.ToDouble(dic["LATITUDE"]), Convert.ToDouble(dic["LONGITUDE"]));
                    if (distance < 1000)
                        locationContent.Distance = "距离：" + distance.ToString("0.0") + "米";
                    else
                        locationContent.Distance = "距离：" + (distance / 1000).ToString("0.00") + "千米";
                    double azimuth = GetAzimuth(geoposition.Coordinate.Point.Position.Latitude, geoposition.Coordinate.Point.Position.Longitude,
                        Convert.ToDouble(dic["LATITUDE"]), Convert.ToDouble(dic["LONGITUDE"]));
                    if (azimuth == 0)
                        locationContent.Azimuth = "北";
                    else if (azimuth > 0 && azimuth < 90)
                        locationContent.Azimuth = "北偏东\t" + azimuth.ToString("0.000") + "°";
                    else if (azimuth == 90)
                        locationContent.Azimuth = "东";
                    else if (azimuth > 90 && azimuth < 180)
                        locationContent.Azimuth = "南偏东\t" + (180 - azimuth).ToString("0.000") + "°";
                    else if (azimuth == 180)
                        locationContent.Azimuth = "南";
                    else if (azimuth > 180 && azimuth < 270)
                        locationContent.Azimuth = "南偏西\t" + (azimuth - 270).ToString("0.000") + "°";
                    else if (azimuth == 270)
                        locationContent.Azimuth = "西";
                    else if (azimuth > 270 && azimuth < 360)
                        locationContent.Azimuth = "北偏西\t" + (360 - azimuth).ToString("0.000") + "°";

                    locationBox.Add(locationContent);
                }
                InspectionList.ItemsSource = locationBox;
            }
            
            ProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }


        private static double AngToRad(double d)    //度数转弧度
        {
            return d * Math.PI / 180.0;
        }

        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = AngToRad(lat1);
            double radLat2 = AngToRad(lat2);
            double a = radLat1 - radLat2;
            double b = AngToRad(lng1) - AngToRad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        public static double GetAzimuth(double lat1, double lng1, double lat2, double lng2)
        {
            double lat1Rad = AngToRad(lat1);
            double lng1Rad = AngToRad(lng1);
            double lat2Rad = AngToRad(lat2);
            double lng2Rad = AngToRad(lng2);
            double azimuth = 0;

            azimuth = Math.Sin(lat1Rad) * Math.Sin(lat2Rad) + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(lng2Rad - lng1Rad);
            azimuth = Math.Sqrt(1 - azimuth * azimuth);

            azimuth = Math.Cos(lat2Rad) * Math.Sin(lng2Rad - lng1Rad) / azimuth;

            azimuth = Math.Asin(azimuth) * 180 / Math.PI;
            return azimuth;
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
            Frame.Navigate(typeof(Main));
        }

    }
}
