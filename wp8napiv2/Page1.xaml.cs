using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace wp8napiv2
{
    public partial class Page1 : PhoneApplicationPage
    {
        bool addSub = false;
        bool tap = false;

        TimeSpan pos = new TimeSpan(0, 0, 0, 0, 0);

        Uri patch_g;

        public Page1()
        {
            InitializeComponent();
        }

        private void setSubs()
        {
            //MessageBox.Show(PivotPage1.napisy);

            //TimelineMarkerCollection marks =  new TimelineMarkerCollection();

            string[] x = new string[1] { "\n" };
            string[] lines = PivotPage1.napisy.Split(x, StringSplitOptions.RemoveEmptyEntries);

            foreach (string i in lines)
            {
                TimelineMarker uno = new TimelineMarker();
                TimelineMarker dos = new TimelineMarker();
                // RegEx

                //MessageBox.Show(i);

                Regex reg = new Regex(@"\[\d*\]");
                MatchCollection matches = reg.Matches(i);
                if (matches.Count == 2)
                {
                    string time_p = matches[0].Value.Substring(1, matches[0].Value.Length - 2);
                    string time_k = matches[1].Value.Substring(1, matches[1].Value.Length - 2);

                    string text = i.Substring(time_p.Length + time_k.Length + 4);

                    //MessageBox.Show("Tekst: " + text + "\n\nP: " + time_p + "\n\nK: " + time_k);

                    uno.Text = text;
                    dos.Text = "";

                    //double time_p_d = Double.Parse(time_p) / 23.976;
                    //double time_k_d = Double.Parse(time_k) / 23.976;
                    float time_p_d = float.Parse(time_p) / 10;
                    float time_k_d = float.Parse(time_k) / 10;
                    int m_p = (int)(time_p_d / 60);
                    time_p_d = time_p_d - 60 * m_p;
                    int m_k = (int)(time_k_d / 60);
                    time_k_d = time_k_d - 60 * m_k;
                    int h_p = m_p / 60;
                    m_p = m_p - 60 * h_p;
                    int h_k = m_k / 60;
                    m_k = m_k - 60 * h_k;

                    string time_p_s = h_p.ToString() + ":" + m_p.ToString() + ":" + time_p_d.ToString();
                    string time_k_s = h_k.ToString() + ":" + m_k.ToString() + ":" + time_k_d.ToString();

                    //string last = lines.Last();

                    //if(i==lines.Last()) { MessageBox.Show("P: " + time_p_s + "\n\nK: " + time_k_s + "\n\nLine: " + i); break;}

                    try
                    {
                        uno.Time = TimeSpan.Parse(time_p_s);
                        dos.Time = TimeSpan.Parse(time_k_s);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message + "\n\nP: " + time_p_s + "\n\nK: " + time_k_s); }
                    //uno.Type = "subs";
                    //dos.Type = "subs";

                    //marks.Add(uno);
                    //marks.Add(dos);
                    try
                    {
                        //MessageBox.Show("Przed dodaniem");

                        myPlayer.Markers.Add(uno);
                        myPlayer.Markers.Add(dos);

                        //MessageBox.Show("Po dodaniu");
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else
                {
                    MessageBox.Show("Błedny format napisów. Liczba: " + matches.Count.ToString());

                    string tmp = "";
                    foreach (Match j in matches) tmp += j.Value + "\n";

                    MessageBox.Show(tmp);

                    break;
                }

            }

            //MessageBox.Show("Po dodaniu wszystkich");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string patch;

            NavigationContext.QueryString.TryGetValue("patch", out patch);

            //MessageBox.Show("Przed filmem");

            MessageBox.Show(patch);

            myPlayer.Source = new Uri(patch, UriKind.RelativeOrAbsolute);

            patch_g = myPlayer.Source;
            //myPlayer.Pause();

            //setSubs();
        }

        private void myPlayer_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
                subs.Text = e.Marker.Text.ToString().Replace("|", "\n");
            //MessageBox.Show(e.Marker.Text);
        }

        private void myPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            setSubs();

            myPlayer.Position = pos;
        }

        private void myPlayer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (myPlayer.CurrentState == MediaElementState.Playing) { myPlayer.Markers.Clear(); myPlayer.Pause(); }
            if (myPlayer.CurrentState == MediaElementState.Paused || myPlayer.CurrentState == MediaElementState.Stopped) { pos = myPlayer.Position; myPlayer.Source = patch_g; }
        }
    }
}