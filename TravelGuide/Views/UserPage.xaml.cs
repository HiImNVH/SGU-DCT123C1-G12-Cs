using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelGuide.Views
{
    public partial class UserPage : ContentPage
    {
        public UserPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UsernameLabel.Text = UserSession.Username;
            EmailLabel.Text = UserSession.Email;
        }
        private async void HomePageTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        private async void OnPlaceTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PlaceListPage());
        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.Username = null;
            UserSession.Email = null;

            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
