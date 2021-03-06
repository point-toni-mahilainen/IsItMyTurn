﻿using IsItMyTurn.Models;
using IsItMyTurn.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IsItMyTurn.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPage : ContentPage
    {
        public CompletedShift shiftObject;
        public EditPage(CompletedShift obj)
        {
            InitializeComponent();
            BindingContext = new PickerViewModel();

            shiftObject = obj;
        }

        protected override void OnAppearing()
        {
            // Data for the shift to be edited to apartment and date pickers
            string[] splittedDateString = shiftObject.DateStr.Split(new char[] { '.' });

            int day = int.Parse(splittedDateString[0]);
            int month = int.Parse(splittedDateString[1]);
            int year = int.Parse(splittedDateString[2]);

            ApartmentPicker.SelectedIndex = shiftObject.ApartmentId - 1;
            DatePicker.Date = new DateTime(year, month, day);
        }

        private async void UpdateBtn_Clicked(object sender, EventArgs e)
        {
            // Updated data to database
            Apartment item = (Apartment)ApartmentPicker.SelectedItem;

            NewShift newShift = new NewShift()
            {
                ApartmentId = item.ApartmentId,
                Date = DatePicker.Date
            };

            string json = JsonConvert.SerializeObject(newShift);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PutAsync("https://isitmyturnapi.azurewebsites.net/api/completedshift/" + shiftObject.ShiftId.ToString(), content);
            int status = (int)response.StatusCode;

            // Status codes:
            // 200 - Everything OK
            // 201 - A shift has updated successfully. Some problems with notifications
            if (status == 200)
            {
                await DisplayAlert("Is It My Turn", "Kirjaus päivitetty onnistuneesti!", "OK");
                var vUpdatedPage = new SeekAndDestroy();
                Navigation.InsertPageBefore(vUpdatedPage, this);
                NavigationPage.SetHasNavigationBar(vUpdatedPage, false);
                await Navigation.PopAsync();
            }
            else if (status == 201)
            {
                await DisplayAlert("Is It My Turn",
                    "Kirjauksen päivitys onnistui, mutta ilmoitusten lähettämisessä käyttäjille ilmeni ongelmia.\r\n\r\n" +
                    "Käytä WhatsApp-ryhmää vuoron vaihdon ilmoittamiseen ja ota yhteyttä sovelluksen ylläpitäjään.", "OK");
                var vUpdatedPage = new SeekAndDestroy();
                Navigation.InsertPageBefore(vUpdatedPage, this);
                NavigationPage.SetHasNavigationBar(vUpdatedPage, false);
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Is It My Turn", "Kirjauksen päivitys epäonnistui! Ole hyvä ja yritä uudelleen.\r\nJos ongelma ei poistu, ota yhteyttä sovelluksen ylläpitäjään.", "OK");
            }
        }

        private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void ToMainPageBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PopToRootAsync();
        }
    }
}