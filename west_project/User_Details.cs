using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using west_project.Utilities;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace west_project
{
    public class Location_Data //contains location data for each time instance
    {
        public double Latitude;
        public double Longitude;
        //private DateTime time_Stamp;

        public static Location_Data createLocData(double Lat, double Long)
        {
            Location_Data locdata = new Location_Data();
            locdata.Latitude = Lat;
            locdata.Longitude = Long;
            return locdata;
        }
        
    }
    public class User  : INotifyPropertyChanged
    {
        
        private string name { get; set; }
        private int userId { get; set; }
        private StorageFolder image { get; set; } //The folder of the images.
        private StorageFile audio { get; set; }
        private Geopoint StartPoint { get; set; }
        private List<Location_Data> locationData { get; set; }
        private List<string> tags { get; set; }
        private StorageFile video { get; set; } //If the media is video
        //private List<List<hog_records>> hogRecords { get; set; }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.OnPropertyChanged();
            }
        }
        public int UserId
        {
            get { return this.userId; }
            set
            {
                this.userId = value;
                this.OnPropertyChanged();
            }
        }
        public StorageFolder Image
        {
            get { return this.image; }
            set
            {
                this.image = value;
                this.OnPropertyChanged();
            }
        }
        public StorageFile Audio
        {
            get { return this.audio; }
            set
            {
                this.audio = value;
                this.OnPropertyChanged();
            }
        }
        public Geopoint startPoint
        {
            get { return this.StartPoint; }
            set
            {
                this.StartPoint = value;
                this.OnPropertyChanged();
            }
        }
        public List<Location_Data> LocationData
        {
            get { return this.locationData; }
            set
            {
                this.locationData = value;
                this.OnPropertyChanged();
            }
        }

        public List<string> Tags
        {
            get { return this.tags; }
            set
            {
                this.tags = value;
                this.OnPropertyChanged();

            }
        }
        public StorageFile Video
        {
            get { return this.video; }
            set
            {
                this.video = value;
                this.OnPropertyChanged();
            }
        }

        //public List<List<hog_records>> HogRecords
        //{
        //    get { return this.hogRecords; }
        //    set
        //    {
        //        this.hogRecords = value;
        //        this.OnPropertyChanged();

        //    }
        //}


        public event PropertyChangedEventHandler PropertyChanged = delegate { };


        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }



        public static User CreateUser(string Name, string ImagePath, int UserId, Geopoint point = null, Windows.Storage.StorageFile LocFile = null, Windows.Storage.StorageFile TagFile = null, Windows.Storage.StorageFile HOGFile = null)
        {
            User newUser = new User();
            newUser.Name = Name;
            newUser.UserId = UserId;
            if (LocFile == null)
            {
                newUser.LocationData = null;
                if (point == null)
                {
                    // Specify the initial location of the person  
                    BasicGeoposition InitialLocation = new BasicGeoposition() { Latitude = 12.934621, Longitude = 77.577700 }; //Test Coordinates          
                    Geopoint InitialLocationPoint = new Geopoint(InitialLocation); //A geopoint is diaplayed on the map
                    newUser.startPoint = InitialLocationPoint;
                }
                else
                {
                    newUser.startPoint = point;
                }
            }
            else
            {
                //Always null
            }


            return newUser;
        }

        public static async Task<List<Location_Data>> AddLocationData(Windows.Storage.StorageFile LocFile)
        {
            List<Location_Data> dummyList = new List<Location_Data>();
            string[] data = null;
            //Add the location data for the user once the csv file is loaded
            foreach (string line in await Windows.Storage.FileIO.ReadLinesAsync(LocFile))
            {
                data = (line.Split(',')).ToArray();
                Location_Data DataPoint = Location_Data.createLocData(double.Parse(data[0]), double.Parse(data[1]));
                dummyList.Add(DataPoint);

            }
            return dummyList;

             
        }

        public static async Task<List<string>> AddTagData(Windows.Storage.StorageFile TagFile)
        {
            List<string> dummyList = new List<string>();
            
            //Add the Tag data for the user once the csv file is loaded
            foreach (string line in await Windows.Storage.FileIO.ReadLinesAsync(TagFile))
            {
                if (line != string.Empty)
                {
                    dummyList.Add(line);
                }

            }
            return dummyList;


        }

        //public static List<List<hog_records>> AddHOGData(Windows.Storage.StorageFile HogFile)
        //{
        //    List<hog_records> perimageList = new List<hog_records>();
        //    List<List<hog_records>> overallList = new List<List<hog_records>>();




        //}

        public static async Task<int> ChangeUserId(Windows.Storage.StorageFile LocFile)
        {
            string[] data = null;
            foreach (string line in await Windows.Storage.FileIO.ReadLinesAsync(LocFile))
            {
                data = (line.Split(',')).ToArray();
                return int.Parse(data[2]);
            }
            return int.Parse(data[2]);
        }

        public static Geopoint CreateStartPoint(double lat, double longitude)
        {
            // Specify the initial location of the person  
            BasicGeoposition InitialLocation = new BasicGeoposition() { Latitude = lat, Longitude = longitude };
            Geopoint InitialLocationPoint = new Geopoint(InitialLocation); 
            return InitialLocationPoint;
        }

    }

  
}
