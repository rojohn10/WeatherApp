using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Globalization;
using WeatherApp.Fragments;
using System.Net;
using System.IO;
using Android.Graphics;
using Plugin.Connectivity;

namespace WeatherApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        Button getWeatherButton;
        TextView placeTextView, temperatureTextView, weatherDescriptionTextView;
        EditText cityNameEditText;
        ImageView weatherImageView;

        ProgressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            getWeatherButton = (Button)FindViewById(Resource.Id.getWeatherButton);
            placeTextView = (TextView)FindViewById(Resource.Id.placeText);
            temperatureTextView = (TextView)FindViewById(Resource.Id.temperatureText);
            weatherDescriptionTextView = (TextView)FindViewById(Resource.Id.weatherDescriptionText);
            cityNameEditText = (EditText)FindViewById(Resource.Id.cityNameText);
            weatherImageView = (ImageView)FindViewById(Resource.Id.weatherImage);

            getWeatherButton.Click += GetWeatherButton_Click;
        }

        private void GetWeatherButton_Click(object sender, System.EventArgs e)
        {
            string place = cityNameEditText.Text;
            GetWeather(place);
            cityNameEditText.Text = "";
            
        }

        async void GetWeather(string place)
        {
            string apiKey = "ce30f178c58a0b13af945920fdd79dbc";
            string apiBase = "https://api.openweathermap.org/data/2.5/weather?q=";
            string unit = "metric";

            if (string.IsNullOrEmpty(place))
            {
                Toast.MakeText(this, "Please enter a valid city name", ToastLength.Short).Show();
                return;
            }

            //Check if the device is not connected to the internet
            
            if (!CrossConnectivity.Current.IsConnected)
            {
                Toast.MakeText(this, "No Internet Connection", ToastLength.Short).Show();
                return;
            }

            ShowProgressDialogue("Loading weather...");

            //Asynchronous API call using HttpClient 
            string url = apiBase + place + "&appid=" + apiKey + "&units=" + unit;
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);

            //PARSE JSON FILE TO AN OBJECT
            var resultObject = JObject.Parse(result);
            string weatherDescription = resultObject["weather"][0]["description"].ToString();
            string icon = resultObject["weather"][0]["icon"].ToString();
            string temperature = resultObject["main"]["temp"].ToString();
            string placeName = resultObject["name"].ToString();
            string country = resultObject["sys"]["country"].ToString();

            //to capitalized starting letters of the location
            weatherDescription = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(weatherDescription);

            weatherDescriptionTextView.Text = weatherDescription;
            placeTextView.Text = placeName + ", " + country;
            temperatureTextView.Text = temperature;

            //Download image using WebRequest
            string imageUrl = "http://openweathermap.org/img/wn/" + icon + ".png";
            System.Net.WebRequest request = default(System.Net.WebRequest);
            request = WebRequest.Create(imageUrl);
            request.Timeout = int.MaxValue;
            request.Method = "GET";

            WebResponse response = default(WebResponse);
            response = await request.GetResponseAsync();

            //Memorystream makes us easier to convert the recieved data from API to byte array
            //Byte array makes us easier to decode it to a bitmap, so when we have an access to the bitmap
            //it means we can set it to image view.
            MemoryStream ms = new MemoryStream();
            response.GetResponseStream().CopyTo(ms); 
            byte[] imagedata = ms.ToArray();

            //this will give access to bitmap
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imagedata, 0, imagedata.Length);
            weatherImageView.SetImageBitmap(bitmap);

            CloseProgressDialogue();
        }

        void ShowProgressDialogue(string status)
        {
            progressDialogue = new ProgressDialogueFragment(status);

            //Transaction manager
            var trans = SupportFragmentManager.BeginTransaction();
            progressDialogue.Cancelable = false;
            progressDialogue.Show(trans, "progress");
        }

        void CloseProgressDialogue()
        {
            if (progressDialogue != null)
            {
                progressDialogue.Dismiss();
                progressDialogue = null;
            }
        }
    }
}