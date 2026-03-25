using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelGuide;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }
    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // quay lại login
    }
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        //tạm thời chỉ quay về login
        await Shell.Current.GoToAsync("..");
    }
}
