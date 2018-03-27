using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace west_project
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Multiple_Person : Page
    {

        ObservableCollection<User> Users = new ObservableCollection<User>();
        Random rand = new Random();
        public Multiple_Person()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Test code only
            //int numberUsers = 5;
            //populateUserList(numberUsers); //5 = number of users to create

            Users = (ObservableCollection<User>)e.Parameter;
            int numberUsers = Users.Count;


            //Non Test Code
            SetupMap(numberUsers);  //Setup the map initially
            
            UsersList.ItemsSource = Users;
        }

        

        private void SetupMap(int numberUsers)
        {
            
            for(int i =0; i<numberUsers; i++)
            {
                //Specify the initial location of the person : Creating a map icon
                MapIcon InitialLocationIcon = new MapIcon();
                InitialLocationIcon.Location = Users[i].startPoint;
                InitialLocationIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                InitialLocationIcon.Title = "User " + i; //The name of the person
                InitialLocationIcon.ZIndex = 0;

                //Specify the initial location of the person : Adding the Initial Location to the map
                MultiplePeopleMap.MapElements.Add(InitialLocationIcon);
               // Specify the initial location of the person : Centre the map over the initial location
                MultiplePeopleMap.Center = InitialLocationIcon.Location;  //Center the map here
                MultiplePeopleMap.ZoomLevel = 14; //Mention the zoom level of the map
                List<BasicGeoposition> PathPointList = CreateListGeopositions(Users[i]);
                //Specify the route of a person
                MapPolyline OverallRoute = new MapPolyline();
                OverallRoute.Path = new Geopath(PathPointList);


                //Specify the route of a person : Display on the map
                if(i%3 == 0)
                {
                    OverallRoute.StrokeColor = Colors.Blue;
                }
                if (i % 3 == 1)
                {
                    OverallRoute.StrokeColor = Colors.LavenderBlush;
                }
                if (i % 3 == 2)
                {
                    OverallRoute.StrokeColor = Colors.Red;
                }

                    OverallRoute.StrokeThickness = 8;
                OverallRoute.StrokeDashed = false;
                MultiplePeopleMap.MapElements.Add(OverallRoute);
            }

        }
        private List<BasicGeoposition> CreateListGeopositions(User user)
        {
            //Create a list of geopositions from given location data
            List<BasicGeoposition> PositionList = new List<BasicGeoposition>();
            foreach (Location_Data item in user.LocationData)
            {
                PositionList.Add(new BasicGeoposition() { Latitude = item.Latitude, Longitude = item.Longitude });
            }
            return PositionList;
        }






        private void UsersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(personal_focus),(User)e.ClickedItem);
        }
    }
}
