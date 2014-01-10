using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace wp8napiv2
{
    public partial class Page2 : PhoneApplicationPage
    {
        string patch;
        public Page2()
        {
            InitializeComponent();
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (patch != null) NavigationService.Navigate(new Uri("/Page1.xaml?patch=" + patch, UriKind.Relative));
            else MessageBox.Show("Błędny adres pliku, spróbuj jeszczec raz. Jeśli błąd będzie nadal występować należy wykonać wszystkie czynności od poczatku.");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigationContext.QueryString.TryGetValue("patch", out patch);
        }
    }
}