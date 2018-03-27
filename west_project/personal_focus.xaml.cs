using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace west_project
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class personal_focus : Page
    {
        
        public personal_focus()
        {
            this.InitializeComponent();
        }

     

       
        List<StorageFile> Image_list = new List<StorageFile>();  //List of names of the images
        static List<Location_Data> Location_list = new List<Location_Data>();  //List of locations
        static List<string> Tag_list = new List<string>(); //List of Tags (3 per image)
        Image MovementGif = new Image();
        string Audio_track_name = null;
        static int Image_count = 0; //To maintain the count of the image being displayed
        static int Location_count = 0; //To maintain count of location being displayed
        static bool Timer_status = false;
        List<BitmapImage> BitmapImagesList = new List<BitmapImage>();
        double TimePerImage = 0.6;
        int NumberofImagesPerLoc = 10;
        User user = null;
       

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            


            //Get the single person object from the previous page and then set various variables
            user = (User)e.Parameter;
            //Setup the list of locations with the initial location in the beginning using Location_Data structure in Location_list
            Location_list = user.LocationData;
            
            //Setup the Image names with the first image being the initial image in Image_List
            IReadOnlyList<StorageFile> fileList = await user.Image.GetFilesAsync();  //Get all file details as a readonly list
            Debug.WriteLine(fileList.Count);
            foreach (StorageFile file in fileList)
            {
                BasicProperties BasicProp = await file.GetBasicPropertiesAsync();
                if(BasicProp.Size != 0)                          //Do not add 0 byte files.
                    Image_list.Add(file);
            }   //Creates a list of files

            //Setup the Tags in the same order as images
            Tag_list = user.Tags;

            //Debug.WriteLine(Image_list);

            //Test Image list
            //StorageFolder AppInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;    //Gets Current installed location
            //StorageFolder Assets = await AppInstalledFolder.GetFolderAsync("Assets");          //Gets current assets folder
            //StorageFolder TestImages = await Assets.GetFolderAsync("TestPhotos");





            //Setup the audio track  in Audio_track_name
            Audio_track_name = user.Audio.Name;


            MovementGif.Height = 50;
            MovementGif.Width = 30;
            MovementGif.Source = new BitmapImage(new Uri("ms-appx:///Assets/walker.gif", UriKind.Absolute));
            MovementGif.Visibility = Visibility.Collapsed;
            //Initial UI setup
            SetupMap(); //Setup the initial Location and the overall route
            SetupAudio();  //Setup the audio player
            SetupImage(); //Setup the Initial Image

            SetupImageRate();
            //SetupMovementGif

            //MovementGif.Source = new BitmapImage(new Uri("ms-appx:///Assets/runner.gif", UriKind.Absolute));
            

            // await CreateBMapList();



        }
        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            //Kill Audio when navigated away
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Audio_player?.MediaPlayer.Dispose();
            });
        }

        private async void SetupImageRate()
        {
            //This method is involved in calculating the average time per image and number of images per location
            MusicProperties Audio_prop = await user.Audio.Properties.GetMusicPropertiesAsync();
            TimeSpan Audio_span = Audio_prop.Duration;
            int NumberofImages = Image_list.Count;
            TimePerImage = Audio_span.TotalSeconds / NumberofImages;
            NumberofImagesPerLoc = Image_list.Count / Location_list.Count;

        }




        private Geopoint CreateGeopoint()
        {
            //Returns an appropriate geopoint according to the location count   . This method
            //is to be used only when the location cont should determine the location on the map.
            if(Location_count >= Location_list.Count)
            {
                Location_count = Location_list.Count - 1;
            }

            BasicGeoposition TempGeoposition = new BasicGeoposition() { Latitude = Location_list[Location_count].Latitude,
            Longitude = Location_list[Location_count].Longitude };
            Geopoint RequiredGeopoint = new Geopoint(TempGeoposition);
            return RequiredGeopoint;
        }

      
        private static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            IRandomAccessStream stream = (IRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

            Debug.WriteLine(stream);
            await bitmapImage.SetSourceAsync(stream);

            return bitmapImage;

        }

        private async void SetupImage()
        {
            //Display the initial image
            Image_spot.Source = await LoadImage(Image_list[Image_count]);
            BigPhotoView.Source = await LoadImage(Image_list[Image_count]);
            if (Tag_list != null)
            {
                TagBox.Text = Tag_list[Image_count];
            }
        }

        private void SetupAudio()
        {
            //This function sets up the audio player before any user interaction

            //Add audio source to the mediaplayer
            Audio_player.Source = MediaSource.CreateFromStorageFile(user.Audio);    //Test Audio

            //Binding State changed function to audio player so as to sync other elements on screen
            Audio_player.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            Audio_player.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            Audio_player.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            
            //Timestamp Matching
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {

                    Image_count = (int)((((sender)).Position.TotalSeconds / TimePerImage));
                    if (Image_count >= Image_list.Count - 1)
                    {
                        Image_count = Image_list.Count - 1;
                    }
                    Image_spot.Source = await LoadImage(Image_list[Image_count]);
                    BigPhotoView.Source = await LoadImage(Image_list[Image_count]);
                    if (Tag_list != null)
                    {
                        TagBox.Text = Tag_list[Image_count];
                    }

                    int PreviousLocCount = Location_count;
                    Location_count = (int)(((sender)).Position.TotalSeconds / (TimePerImage * NumberofImagesPerLoc));
                    if (PreviousLocCount != Location_count)
                    {
                        
                        UpdateMap();
                    }
                   




                });
        }




        private void UpdateMap()
        {
            MapIcon Location_Icon = Location_map.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;
            Image GifTemp = Location_map.Children.FirstOrDefault(x => x is Image) as Image;
            if(GifTemp != null)
            {
                Location_map.Children.Remove(GifTemp);
            }
            
            Location_map.MapElements.Remove(Location_Icon);
            Geopoint DisplayPoint = CreateGeopoint(); //The new point on the map at which it has to be displayed
            Point MapPoint; //Dummy point
            Location_Icon.Location = DisplayPoint; //Set the geographical coordinates of the geopoint

            DistanceCalculator();
            Location_map.Children.Add(MovementGif);
            MapControl.SetLocation(MovementGif, DisplayPoint);
            MapControl.SetNormalizedAnchorPoint(MovementGif, new Point(0.5, 0.5));
            Location_map.MapElements.Add(Location_Icon);
            //Specify the location of the person : Centre the map over the location
            Location_map.Center = DisplayPoint;  //Center the map here
            Location_map.ZoomLevel = 19;         //Mention the zoom level of the map
            Location_map.GetOffsetFromLocation(DisplayPoint, out MapPoint); //Converts Latitude and longitude to x and Y coordinates.
            double X = MapPoint.X;
            double Y = MapPoint.Y;  
            
            //WalkingImage.Margin = new Thickness(X, Y, 0, 0);
            //WalkingImage.Visibility = Visibility.Visible;
            Image_spot.Margin = new Thickness(X + 30, Y, 0, 0);
            Close_Button.Margin = new Thickness(X + 242, Y+55, 0, 0);
        }
        private void DistanceCalculator()
        {
            double distance = 0;
            if (Location_count != 0)
            {
                 distance = DistanceTo(Location_list[Location_count].Latitude, Location_list[Location_count].Longitude,
                 Location_list[Location_count - 1].Latitude, Location_list[Location_count - 1].Longitude);
            }
            
            if (distance >= 3)
            {
                //WalkingImage.Visibility = Visibility.Collapsed;
                //RunningImage.Visibility = Visibility.Visible;
                MovementGif.Source = new BitmapImage(new Uri("ms-appx:///Assets/runner.gif", UriKind.Absolute));

            }
            else
            {
                //WalkingImage.Visibility = Visibility.Visible;
                //RunningImage.Visibility = Visibility.Collapsed;
                MovementGif.Source = new BitmapImage(new Uri("ms-appx:///Assets/walker.gif", UriKind.Absolute));
            }

            MovementGif.Visibility = Visibility.Visible;

        }
        private double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'm')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;
            switch (unit)
            {
                case 'm': //meters -> default
                    return dist * 1609.344;
                case 'K': //Kilometers 
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
                
            }
            return dist;
        }

        private void SetupMap()
        {
            //This function sets up the map for first view before any user interaction


            Geopoint InitialLocationPoint = user.startPoint; //A geopoint is diaplayed on the map

            //Specify the initial location of the person : Creating a map icon
            MapIcon InitialLocationIcon = new MapIcon();
            InitialLocationIcon.Location = InitialLocationPoint;
            InitialLocationIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            InitialLocationIcon.Title = user.Name; //The name of the person
            InitialLocationIcon.ZIndex = 0;
            

            //Specify the initial location of the person : Adding the Initial Location to the map
            Location_map.MapElements.Add(InitialLocationIcon);

            //Specify the initial location of the person : Centre the map over the initial location
            Location_map.Center = InitialLocationPoint;  //Center the map here
            Location_map.ZoomLevel = 18;                 //Mention the zoom level of the map

            MovementGif.Visibility = Visibility.Visible;
            Location_map.Children.Add(MovementGif);
            MapControl.SetLocation(MovementGif, InitialLocationPoint);
            MapControl.SetNormalizedAnchorPoint(MovementGif, new Point(0.5, 0.5));

            List<BasicGeoposition> PathPointList = CreateListGeopositions();
            //Specify the route of a person
            MapPolyline OverallRoute = new MapPolyline();
            OverallRoute.Path = new Geopath(PathPointList);
            

            //Specify the route of a person : Display on the map
            OverallRoute.StrokeColor = Colors.Blue;
            OverallRoute.StrokeThickness = 8;
            OverallRoute.StrokeDashed = false;
            Location_map.MapElements.Add(OverallRoute);
        }

        private List<BasicGeoposition> CreateListGeopositions()
        {
            //Create a list of geopositions from given location data
            List<BasicGeoposition> PositionList = new List<BasicGeoposition>();
            foreach (Location_Data item in user.LocationData)
            {
                PositionList.Add(new BasicGeoposition() { Latitude = item.Latitude, Longitude = item.Longitude });
            }
            return PositionList;
        }

        private async void PlaybackSession_PlaybackStateChanged(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            //Starting slideshow when audio starts
            MediaPlaybackSession AudioplaybackSession = sender as MediaPlaybackSession;
            
            if (AudioplaybackSession != null)
            {
                if(AudioplaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    //Start image slideshow
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                     () => {
                         //Image_Control_Timer.Start();
                         //Location_Control_Timer.Start();
                         Timer_status = true;

                     });

                    



                }
                if(AudioplaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                   
                    //Pause Image slideshow
                    if (Timer_status)
                    {   
                        if (AudioplaybackSession.Position.CompareTo(new TimeSpan(0,0,0)) != 0)
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                                    () =>
                                                    {
                                                        //Image_Control_Timer.Stop();
                                                        //Location_Control_Timer.Stop();
                                                        Timer_status = false;
                                                    });
                        }
                        else
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                                         async () =>
                                                         {
                                                             //Image_Control_Timer.Stop();
                                                             //Location_Control_Timer.Stop();
                                                             Image_count = 0;
                                                             Location_count = 0;
                                                             Geopoint DisplayPoint = CreateGeopoint();
                                                             MapIcon Location_Icon = Location_map.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;
                                                             Location_Icon.Location = DisplayPoint; //Set the geographical coordinates of the geopoint
                                                             Location_map.Center = DisplayPoint;  //Center the map here
                                                             Location_map.ZoomLevel = 14;
                                                             Timer_status = false;
                                                             Image_spot.Source = await LoadImage(Image_list[Image_count]);
                                                             BigPhotoView.Source = await LoadImage(Image_list[Image_count]);
                                                             if(Tag_list != null)
                                                             {
                                                                 TagBox.Text = Tag_list[Image_count];
                                                             }
                                                                

                                                         });
                        }
                        
                    }
                    

                    //Location Slideshow
                }

                

            }

            //Time matching of audio, map and images

        }

        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if (Timer_status)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                   
                    Image_count = 0;
                    Timer_status = false;
                    Image_spot.Source = await LoadImage(Image_list[Image_count]);
                    BigPhotoView.Source = await LoadImage(Image_list[Image_count]);
                    if (Tag_list != null)
                    {
                        TagBox.Text = Tag_list[Image_count];
                    }
                });
            }
        }

        private async void Image_spot_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (Image_count >= 0)
            {
                //Setup Esc Key
                Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
                //OPen the flipview to view all the images.
                if (Audio_player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {



                    //Audio_player.MediaPlayer.Pause(); //Pause the audio
                    BigPhotoView.Height = PersonalGrid.ActualHeight - 80;
                    BigPhotoView.Width = PersonalGrid.ActualWidth - 80;
                    //BigPhotoView.Source = await LoadImage(Image_list[Image_count]);
                    BigPhotoView.Visibility = Visibility.Visible;
                    Image_spot.Visibility = Visibility.Collapsed;
                    Close_Button.Visibility = Visibility.Collapsed;
                    //WalkingImage.Visibility = Visibility.Collapsed;
                    //RunningImage.Visibility = Visibility.Collapsed;


                }
                //Open the flipview to view all the images.
                if (Audio_player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {


                    BigPhotoView.Height = PersonalGrid.ActualHeight - 80;
                    BigPhotoView.Width = PersonalGrid.ActualWidth - 80;
                    BigPhotoView.Source = await LoadImage(Image_list[Image_count]);
                    BigPhotoView.Visibility = Visibility.Visible;
                    Image_spot.Visibility = Visibility.Collapsed;
                    Close_Button.Visibility = Visibility.Collapsed;
                   // WalkingImage.Visibility = Visibility.Collapsed;
                    //RunningImage.Visibility = Visibility.Collapsed;

                }
            }

        }

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            if (args.KeyCode == 27) //ESC
            {
                //Do somthing
                BigPhotoView.Visibility = Visibility.Collapsed;
                Image_spot.Visibility = Visibility.Visible;
                Close_Button.Visibility = Visibility.Visible;
                //WalkingImage.Visibility = Visibility.Visible;
                
            }
        }
      
        bool pointerEnterFlag = false;     //Differentiate between enter and click
        private void Location_map_MapElementPointerEntered(MapControl sender, MapElementPointerEnteredEventArgs args)
        {
            //Handles the event when the pointer is placed on a map icon
            
            Type InFocus = args.MapElement.GetType();
            if ( InFocus == typeof(MapIcon))
            {
                //Pause the music and slideshow
                if (Audio_player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                    Audio_player.MediaPlayer.Pause(); //Pauses the slidehow as well due to the handlers setup previously
                                      
                }
                //Show the image temporarily
                double X = args.Position.X;   //Get the X cordinate of Pointer
                double Y = args.Position.Y;   //Get the Y coordinate of the pointer
                Image_spot.Margin = new Thickness(X + 30, Y, 0, 0);
                Close_Button.Margin = new Thickness(X + 240, Y+60, 0, 0);
                Image_spot.Visibility = Visibility.Visible;
                Close_Button.Visibility = Visibility.Collapsed;

            }
            pointerEnterFlag = true;
        }

        private void Location_map_MapElementPointerExited(MapControl sender, MapElementPointerExitedEventArgs args)
        {
            //Handles the event when the map icon loses focus
            Type InFocus = args.MapElement.GetType();
            if (InFocus == typeof(MapIcon))
            {
                //Pause the music and slideshow
                if (Audio_player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                   
                        Audio_player.MediaPlayer.Play(); //Plays the slidehow in the background due to handlers setup previously
                    
                    if(pointerEnterFlag)
                    {
                        Image_spot.Visibility = Visibility.Collapsed;
                    }
                    



                }

            }
            pointerEnterFlag = false;
        }

        private void Location_map_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            MapIcon ClickedIcon = args.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;   //Gets the clicked item
            if (ClickedIcon != null)
            {
                //Pause the music and slideshow
                if (Audio_player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                {
                    Audio_player.MediaPlayer.Play(); //Plays the slidehow in the background due to handlers setup previously
                   
                    //Tag mapicon with image
                    Point MapPoint;
                    Location_map.GetOffsetFromLocation(ClickedIcon.Location, out MapPoint); //Converts Latitude and longitude to x and Y coordinates.
                    double X = MapPoint.X;
                    double Y = MapPoint.Y;
                    Image_spot.Margin = new Thickness(X + 30, Y, 0, 0);
                    Close_Button.Margin = new Thickness(X + 242, Y+55, 0, 0);
                    Image_spot.Visibility = Visibility.Visible; //Change the state of image spot
                    Close_Button.Visibility = Visibility.Visible;



                }
                if (Audio_player.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                {
                   
                    //Tag mapicon with image
                    Point MapPoint;
                    Location_map.GetOffsetFromLocation(ClickedIcon.Location, out MapPoint); //Converts Latitude and longitude to x and Y coordinates.
                    double X = MapPoint.X;
                    double Y = MapPoint.Y;
                    Image_spot.Margin = new Thickness(X + 30, Y, 0, 0);
                    Close_Button.Margin = new Thickness(X + 242, Y+55, 0, 0);
                    Image_spot.Visibility = Visibility.Visible; //Change the state of image spot
                    Close_Button.Visibility = Visibility.Visible;



                }

            }
            pointerEnterFlag = false;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            //Close the image spot
            Image_spot.Visibility = Visibility.Collapsed; //Change the state of image spot
            Close_Button.Visibility = Visibility.Collapsed;
           
        }

       
    }
}
