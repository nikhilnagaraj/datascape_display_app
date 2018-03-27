using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace west_project
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class File_Selection : Page  
    {
        double NumberUsers = 0;
        private ObservableCollection<User> Users = new ObservableCollection<User>() ;

        int UserTag = 0;
      




        public File_Selection()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UsersList.ItemsSource = Users;
            UserNumber.IsOpen = true;    //Open the popup asking you to enter the number of users
        }
        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            //Is run when the done button is clicked on the number of user selection box.
            //It creates a set of dummy users .
            NumberUsers = UserNumberComboBox.SelectedIndex + 1; //Set the number of users whose data shall be loaded simultaneously
            UserNumber.IsOpen = false;
            populateUserList();            
            UsersList.ItemsSource = Users;
            
            
        }
        private void populateUserList()
        {
            //This code is used to populate the user list with a list of dummy users.
            int i = 0;
            while (i < NumberUsers)
            {
                string UserName = "User " + i.ToString();
                int userId = i + 1000;
                string ImagePath = "http://hopezookingston.com/wp-content/uploads/2013/06/donkey.jpg";

                User newUser = User.CreateUser(UserName, ImagePath, userId);
                
                Users.Add(newUser);
                i += 1;

            }
        }

        private void FilePicker_Click(object sender, RoutedEventArgs e)
        {
            //This is run when the load files button of every user is clicked.
            Button ClickedButton = sender as Button;
            MainGrid.IsHitTestVisible = false;
            UserTag = Int32.Parse(ClickedButton.Tag.ToString()); //The tag of the user whose files are to be loaded.
            FileSelection.IsOpen = true;
            
        }

        private async void BrowseJrnyButton_Click(object sender, RoutedEventArgs e)
        {
            //Placeholders for loaded data
            Windows.Storage.StorageFile LocFile = null;
            Windows.Storage.StorageFile TagFile = null;
            Windows.Storage.StorageFile AudFile = null;
            Windows.Storage.StorageFolder ImageFolder = null;
            Windows.Storage.StorageFolder JourneyFolder = null;

            //Boolean to check if required data is available
            bool tagBool = false;

            //Load Journey Folder
            var FolderPicker = new Windows.Storage.Pickers.FolderPicker();
            FolderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            FolderPicker.FileTypeFilter.Add("*");

            JourneyFolder = await FolderPicker.PickSingleFolderAsync();
            if (JourneyFolder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("JourneyFolder", JourneyFolder);
                JourneyFS.Text = JourneyFolder.Path;
                

                ImageFolder = await JourneyFolder.GetFolderAsync("Images");
                LocFile = await JourneyFolder.GetFileAsync("Location_Data.csv");

                //Try getting Tag Data
                try
                {
                    TagFile = await JourneyFolder.GetFileAsync("Tag_Data.csv");
                    tagBool = true;
                }
                catch
                {
                    tagBool = false;
                }
                try
                {
                    AudFile = await JourneyFolder.GetFileAsync("Audio.m4a");
                }
                catch(FileNotFoundException)
                {
                    AudFile = await JourneyFolder.GetFileAsync("Audio_Data.mp3");
                }
                
                int indexUser = Users.IndexOf(Users.Last(x => x.UserId == UserTag)); //Get the index of the user whose data is being loaded

                //Setup User's Location data
                Users[indexUser].LocationData = await User.AddLocationData(LocFile);       //Add location data to the user
                Users[indexUser].startPoint = User.CreateStartPoint(Users[indexUser].LocationData[0].Latitude, Users[indexUser].LocationData[0].Longitude);     //Add start point to the file
                //Users[indexUser].UserId = await User.ChangeUserId(LocFile);
                UserTag = Users[indexUser].UserId;

                Users[indexUser].Image = ImageFolder;  //Add the user's image folder
                Users[indexUser].Audio = AudFile;      //Add the user's audio file

                if (tagBool)
                {
                    Users[indexUser].Tags = await User.AddTagData(TagFile);       //Add Tag data to the user 
                }
               


            }
            else
            {
                JourneyFS.Text = "Cancelled";
            }




        }
  

        private void DoneFSButton_Click(object sender, RoutedEventArgs e)
        {
            //After all the files have been loaded
            MainGrid.IsHitTestVisible = true;
            FileSelection.IsOpen = false;
            DoneData.IsOpen = true;
        }

        private void DoneDataButton_Click(object sender, RoutedEventArgs e)
        {
            DoneData.IsOpen = false;
            if(Users.Count > 1)
            {
                Frame.Navigate(typeof(Multiple_Person), Users);
            }
            else
           {
                Frame.Navigate(typeof(personal_focus), Users[0]);
           }
            
           
        }

        private void CancelDataButton_Click(object sender, RoutedEventArgs e)
        {
            DoneData.IsOpen = false;
        }

        
    }
}
