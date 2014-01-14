using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Storage;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using Cimbalino.Phone.Toolkit.Extensions;
using System.Text;

namespace wp8napiv2
{
    public partial class PivotPage1 : PhoneApplicationPage
    {
        public ObservableCollection<ExternalStorageFile> files {get;set;}
        public ObservableCollection<ExternalStorageFolder> folders{get;set;}

        public ExternalStorageFolder current_folder;

        public static string postData, htmlData, napisy, file_patch;

        public bool download = true;

        //public bool testing = true; // BOOL FOR TESTS, NEED TO BE DELETED IN FULL APP

        public PivotPage1()
        {
            InitializeComponent();

            files = new ObservableCollection<ExternalStorageFile>();
            folders = new ObservableCollection<ExternalStorageFolder>();

            this.DataContext = this;
        }

        private async void Pivot_Loaded(object sender, RoutedEventArgs e)
        {

            ExternalStorageDevice _sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();

            if (_sdCard != null) loadFolder(_sdCard.RootFolder);
            else MessageBox.Show("WP8Napi nie znalazł karty SD. Sprawdz czy karta została (poprawnie) włożona i spróbuj jeszcze raz.");

        }

        private async void loadFolder(ExternalStorageFolder fol_tmp)
        {
            try
            {
                current_folder = fol_tmp;

                folders.Clear();
                files.Clear();

                IEnumerable<ExternalStorageFile> files_tmp = await fol_tmp.GetFilesAsync();
                IEnumerable<ExternalStorageFolder> folders_tmp = await fol_tmp.GetFoldersAsync();

                foreach (ExternalStorageFile i in files_tmp)
                {
                    files.Add(i);
                }

                foreach (ExternalStorageFolder i in folders_tmp)
                {
                    folders.Add(i);
                }
            }

            catch (Exception w) { MessageBox.Show(w.Message + "\n\n#1"); } // Tu sie cos kiedys napisze


        }

        private void ListBox_SelectionChanged_folders(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            if (lb.SelectedItem != null)
            {
                loadFolder((ExternalStorageFolder)lb.SelectedItem);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string x;

            NavigationContext.QueryString.TryGetValue("dow",out x);

            if (x == "false") download = false;
        }

        private async void ListBox_SelectionChanged_files(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;// <==== FOR TESTING !!!

            if (lb.SelectedItem != null //<==== FOR TESTING !!!
                //testing
                )
            {
                ExternalStorageFile patch = (ExternalStorageFile)lb.SelectedItem; //<==== FOR TESTING !!!

                //FileStream file = File.OpenRead(patch.Path); //<==== FOR TESTING !!!
                //FileStream file = File.OpenRead("Resources/twd402.mp4");

                MessageBox.Show(patch.Path);

                ExternalStorageDevice sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();

                // If the SD card is present, get the route from the SD card.
                if (sdCard != null)
                {
                    try
                    {
                        // Get the route (.GPX file) from the SD card.
                        //ExternalStorageFile filex = await sdCard.GetFileAsync(patch.Path);

                        // Create a stream for the route.
                        Stream file = await patch.OpenForReadAsync();

                        if (download)
                        {
                            byte[] file_byte = new byte[10485760];

                            file.Read(file_byte, 0, 10485760);

                            byte[] data = file_byte.ComputeMD5Hash();

                            StringBuilder sBuilder = new StringBuilder();

                            // Loop through each byte of the hashed data  
                            // and format each one as a hexadecimal string. 
                            for (int i = 0; i < data.Length; i++)
                            {
                                sBuilder.Append(data[i].ToString("x2"));
                            }

                            // Return the hexadecimal string. 

                            //MessageBox.Show(sBuilder.ToString());

                            postData = "mode=1&" +
                            "client=NapiProjektPython&" +
                            "client_ver=0.1&" +
                            "downloaded_subtitles_lang=PL&" +
                            "downloaded_subtitles_txt=1&" +
                            "downloaded_subtitles_id=" + sBuilder.ToString();
                            file_patch = patch.Name;

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://napiprojekt.pl/api/api-napiprojekt3.php");
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.Method = "POST";

                            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
                        }
                        else
                        {
                            try
                            {
                                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    try
                                    {
                                        using (StreamReader sr = new StreamReader(store.OpenFile(file_patch.Substring(file_patch.LastIndexOf("/")), FileMode.Open, FileAccess.Read)))
                                        {
                                            napisy = sr.ReadToEnd();
                                        }
                                    }
                                    catch (IsolatedStorageException ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                            }
                            catch (IsolatedStorageException ex)
                            {
                                MessageBox.Show(ex.Message);

                            }

                            NavigationService.Navigate(new Uri("/Page1.xaml?patch=" + patch.Name, UriKind.Relative));
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        // The route is not present on the SD card.
                        MessageBox.Show("Błąd!");
                    }
                }
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ExternalStorageDevice _sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();

            if (_sdCard != null) loadFolder(current_folder);
            else MessageBox.Show("WP8Napi nie znalazł karty SD. Sprawdz czy karta została (poprawnie) włożona i spróbuj jeszcze raz.");
        }

        public void GetRequestStreamCallback(IAsyncResult asyncResult)
        {
            HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;

            Stream postStream = request.EndGetRequestStream(asyncResult);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());

            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult ar)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {

                HttpWebRequest request = ar.AsyncState as HttpWebRequest;
                WebResponse response = request.EndGetResponse(ar);

                StreamReader txt = new StreamReader(response.GetResponseStream(), System.Text.UTF8Encoding.UTF8);

                string str = txt.ReadToEnd();

                txt.Close();

                htmlData = str;

                //MessageBox.Show(str);

                afterRequestCall();

            });
        }

        private void afterRequestCall()
        {
            //MessageBox.Show("2");

            if (htmlData.Contains("<subtitles>"))
            {

                int startIndex = htmlData.IndexOf("<content><![CDATA[") + 18;
                int endIndex = htmlData.IndexOf("]]></content>", startIndex);
                string todecode = htmlData.Substring(startIndex, endIndex - startIndex);

                byte[] data = Convert.FromBase64String(todecode);
                napisy = UTF8Encoding.UTF8.GetString(data, 0, data.Length);

                try
                {
                    using(var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        //store.CreateDirectory("wp8subs");

                        string f_name;

                        //file_patch.Substring(file_patch.LastIndexOf("/"))

                        if (file_patch.LastIndexOf("/") > 0) f_name = file_patch.Substring(file_patch.LastIndexOf("/"));
                        else f_name = file_patch;

                        store.CreateFile(f_name);

                        try
                        {
                            using( StreamWriter sw = new StreamWriter(store.OpenFile(f_name, FileMode.Open, FileAccess.Write)))
                            {
                                sw.Write(napisy);
                            }
                        }
                        catch (IsolatedStorageException ex) 
                        {
                            MessageBox.Show(ex.Message + "\n\n#2"); 
                        }
                    }
                }
                catch (IsolatedStorageException ex)
                {
                    MessageBox.Show(ex.Message + "\n\n#3");

                }

                //MessageBox.Show(napisy);

                //NavigationService.Navigate(new Uri("/Page2.xaml?patch=" + file_patch, UriKind.Relative));
                NavigationService.Navigate(new Uri("/Page1.xaml?patch=" + file_patch, UriKind.Relative));

            }
            else
            {
                MessageBox.Show("Nie znaleziono napisów.");
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        private void For_tests(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("1");

            ListBox_SelectionChanged_files(null, null);
        }
    }
}