using Flex.Smoothlake.FlexLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace FlexApiMeter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Radio radio;
        GUIClient client;

        public class rlhelp : INotifyPropertyChanged
        {
            MainWindow win;
            public rlhelp(MainWindow wnd)
            {
                win = wnd;
            }
            public List<Radio> RadioList 
            { 
                get { return API.RadioList; } 
            }
            public List<GUIClient> ClientList 
            { 
                get { return win.radio == null ? new List<GUIClient>() : win.radio.GuiClients; } 
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged(String name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        
        public rlhelp RLHelp { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            RLHelp = new rlhelp(this);
            btnDisconnect.IsEnabled = false;
            btnBind.IsEnabled = false;
            Meter0.DataContext = minfo0;
            Meter1.DataContext = minfo1;
            Meter2.DataContext = minfo2;
            Meter3.DataContext = minfo3;
            Unit0.DataContext = minfo0;
            Unit1.DataContext = minfo1;
            Unit2.DataContext = minfo2;
            Unit3.DataContext = minfo3;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            disconnect();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.DataContext = RLHelp;


        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            radio = API.RadioList.FirstOrDefault(r => r.Nickname == (string)RadioList.SelectedValue);
            if (radio != null && radio.Connected == false)
            {
                radio.SliceAdded += Radio_SliceAdded;
                radio.SliceRemoved += Radio_SliceRemoved;
                radio.Connect();
                btnConnect.IsEnabled = false;
                btnBind.IsEnabled = true;
                btnDisconnect.IsEnabled = true;
                RLHelp.OnPropertyChanged("ClientList");
            }

        }

        private void btnBind_Click(object sender, RoutedEventArgs e)
        {
            if (radio != null && ClientList.SelectedValue != null)
            {
                client = radio.GuiClients.FirstOrDefault(c => c.Station == (string)ClientList.SelectedValue);
                radio.BindGUIClient(client.ClientID);
            }
        }

        private void Radio_SliceRemoved(Slice slc)
        {
            switch (slc.Index)
            {
                case 0:
                    minfo0.clear();
                    slc.SMeterDataReady -= Slc_SMeterDataReady0;
                    break;
                case 1:
                    minfo1.clear();
                    slc.SMeterDataReady -= Slc_SMeterDataReady1;
                    break;
                case 2:
                    minfo2.clear();
                    slc.SMeterDataReady -= Slc_SMeterDataReady2;
                    break;
                case 3:
                    minfo3.clear();
                    slc.SMeterDataReady -= Slc_SMeterDataReady3;
                    break;
            }
        }

        private void Radio_SliceAdded(Slice slc)
        {
            switch (slc.Index)
            {
                case 0:
                    slc.SMeterDataReady += Slc_SMeterDataReady0;
                    break;
                case 1:
                    slc.SMeterDataReady += Slc_SMeterDataReady1;
                    break;
                case 2:
                    slc.SMeterDataReady += Slc_SMeterDataReady2;
                    break;
                case 3:
                    slc.SMeterDataReady += Slc_SMeterDataReady3;
                    break;
            }
        }

      
       


        void disconnect()
        {
            client = null;
            if (radio != null)
            {
                radio.SliceAdded -= Radio_SliceAdded;
                radio.SliceRemoved -= Radio_SliceRemoved;
                radio.Disconnect();
            }
            radio = null;
            RadioList.SelectedIndex = -1;
            ClientList.SelectedIndex = -1;
            btnConnect.IsEnabled = true;
            btnBind.IsEnabled = false;
            btnDisconnect.IsEnabled = false;
            minfo0.clear();
            minfo1.clear();
            minfo2.clear();
            minfo3.clear();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (radio != null)
            {
                disconnect();

            }
        }
        public class MeterInfo : INotifyPropertyChanged
        {
            private float meterWidth;
            private string sUnit;

            public float MeterWidth { get => meterWidth; set  { meterWidth = value; OnPropertyChanged("MeterWidth");  } }
            public string SUnit { get => sUnit; set { sUnit = value; OnPropertyChanged("SUnit"); } }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(String name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public void clear()
            {
                MeterWidth = 0;
                SUnit = string.Empty;
            }
        }

        public MeterInfo minfo0 = new MeterInfo();
        public MeterInfo minfo1 = new MeterInfo();
        public MeterInfo minfo2 = new MeterInfo();
        public MeterInfo minfo3 = new MeterInfo();

       

        private void Slc_SMeterDataReady0(float data)
        {
            (string sunit, float width) = CalcMeterWidth(data);
            minfo0.MeterWidth = width;
            minfo0.SUnit = sunit;   

        }
        private void Slc_SMeterDataReady1(float data)
        {
            (string sunit, float width) = CalcMeterWidth(data);
            minfo1.MeterWidth = width;
            minfo1.SUnit = sunit;

        }
        private void Slc_SMeterDataReady2(float data)
        {
            (string sunit, float width) = CalcMeterWidth(data);
            minfo2.MeterWidth = width;
            minfo2.SUnit = sunit;

        }
        private void Slc_SMeterDataReady3(float data)
        {
            (string sunit, float width) = CalcMeterWidth(data);
            minfo3.MeterWidth = width;
            minfo3.SUnit = sunit;

        }

        public (string, float) CalcMeterWidth(float mdata)
        {
            if (mdata < -133)
                mdata = -133;
            else if (mdata > 0)
                mdata = 0;
            float meterwidth = 180;
            float S9Mark = 110; 
            // meter is not linear after S9 so different scales below S9 and over S9
            float MeterWidth = (mdata >= -73.0f ?
                S9Mark + (mdata + 73.0f) / 60 * (meterwidth - S9Mark) :
                (mdata + 133.0f) / 60 * S9Mark);
            
            string SUnit = TranslateDbmToSUnit(mdata);
            return (SUnit, MeterWidth);
        }

        private string TranslateDbmToSUnit(float mdata)
        {
            string snu = string.Empty;
            if (mdata < -121f)
            {
                snu = "S0   ";
            }
            else if (mdata <= -115f)
            {
                snu = "S1   ";
            }
            else if (mdata <= -109f)
            {
                snu = "S2   ";
            }
            else if (mdata <= -103f)
            {
                snu = "S3   ";
            }
            else if (mdata <= -97f)
            {
                snu = "S4   ";
            }
            else if (mdata <= -91f)
            {
                snu = "S5   ";
            }
            else if (mdata <= -85f)
            {
                snu = "S6   ";
            }
            else if (mdata <= -79f)
            {
                snu = "S7   ";
            }
            else if (mdata <= -73f)
            {
                snu = "S8   ";
            }
            else if (mdata <= -63f)
            {
                snu = "S9   ";
            }
            else if (mdata <= -53f)
            {
                snu = "S9+10";
            }
            else if (mdata <= -43f)
            {
                snu = "S9+20";
            }
            else if (mdata <= -33f)
            {
                snu = "S9+30";
            }
            else if (mdata <= -23f)
            {
                snu = "S9+40";
            }
            else if (mdata <= -13f)
            {
                snu = "S9+50";
            }
            else if (mdata <= -3f)
            {
                snu = "S9+60";
            }
            else if (mdata <= 7f)
            {
                snu = "S9+70";
            }
            else if (mdata <= 17f)
            {
                snu = "S9+80";
            }
            else
            {
                snu = "S9+90";
            }
            return snu;
        }
    }
}
