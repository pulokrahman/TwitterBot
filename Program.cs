using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Text.RegularExpressions;
using System.Text;

using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text.Json;
using Twitter.Models;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Twitter
{
    public class Tweets
    {
        public statuses[] statuses { get; set;}

     }
    public class statuses
{
       public Int64 id { get; set; }
        public string created_at { get; set; }
        public string id_str { get; set; }
        public string text { get; set; }
     
    }

    

    public class Program
    {
  
          static string oauth_consumer_key;
       static string CONSUMER_TOKEN;
        static string oauth_token;
      static string ACCESS_TOKEN_SECRET;
       static Dictionary<String, String> foo;
        static string signingkey; 
      static byte[] key;

    static HMACSHA1 myhmacsha1;
       
        static string searchkey;
        static string MaxKey;
        static Boolean Max = false;
        static Boolean Since = false;
        static Boolean Retweets = true;
       static int l = 7;
       public static async Task Search(string what,Boolean Recent)
        {

            string httpmethod = "GET";
            IntializeDictionary();
            foo.Add("q", what);
            if (Recent == true) {
                foo.Add("result_type", "recent");
            }
            if (Since == true && searchkey !=null)
            {
                foo.Add("since_id",searchkey);
            }
              
            string param = ConstructParameterstring(foo);

            string url = "https://api.twitter.com/1.1/search/tweets.json";
            string ParamString = string.Format(
             "{0}&{1}&{2}",
             httpmethod,
             Uri.EscapeDataString(url),
             Uri.EscapeDataString(param.ToString())
         );
            ConstructOauthsign(ParamString);
            String Header = ConstructHeader();
            await GETURLENCODED(Header, url);
        }
        public static async Task Retweet(String search)
        {
            string httpmethod = "POST";
            IntializeDictionary();
            string param = ConstructParameterstring(foo);
            String url = "https://api.twitter.com/1.1/statuses/retweet/" + search + ".json";
            string ParamString = string.Format(
             "{0}&{1}&{2}",
             httpmethod,
             Uri.EscapeDataString(url),
             Uri.EscapeDataString(param.ToString())
         );
            ConstructOauthsign(ParamString);
            String Header = ConstructHeader();
            Dictionary<String, String> j = new Dictionary<string, string>
            {
                {"id",search }
        };
            var json = JsonConvert.SerializeObject(j, Formatting.Indented);
            var httpContent = new StringContent(json);
            await POSTJSON(Header, url, httpContent);
        }
        public static async Task POSTJSON(string Auth, string URL, StringContent JSON)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", Auth);
           
           
            var response = await client.PostAsync(URL, JSON);
           var responseString = await response.Content.ReadAsStringAsync();
         //   Console.WriteLine(responseString); 

        }
        public static async Task GETURLENCODED(string Auth, string url)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Auth);
            string urlencoded = "?"+ string.Join(
        "&",
       foo
            .Where(kvp => !kvp.Key.StartsWith("oauth_"))
            .Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))
            .OrderBy(s => s));
            //Console.WriteLine(urlencoded);
            var response = await client.GetAsync("https://api.twitter.com/1.1/search/tweets.json"+urlencoded);
            var responseString = await response.Content.ReadAsStringAsync();
          // Console.WriteLine(responseString);
           
            Tweets json = System.Text.Json.JsonSerializer.Deserialize<Tweets>(responseString);
            if (json.statuses.Length >0 && Since==false) {
                var k = json.statuses[0];
             //   Console.WriteLine(k.text);
             //   l--;
                searchkey = k.id_str;
                if (Retweets)
                {
                    await Retweet(searchkey);
                }
            }
          else  if (json.statuses.Length > 0 && Since == true)
            {
                var k = json.statuses[json.statuses.Length-1];
                Console.WriteLine(k.text);
                //   l--;
                searchkey = k.id_str;
                if (Retweets)
                {
                    await Retweet(searchkey);
                }
            }

        }
        public static async Task Tweet1(String status)
        {
            string httpmethod = "POST";
            IntializeDictionary();
            foo.Add("status", status);
            string param = ConstructParameterstring(foo);
            string url = "https://api.twitter.com/1.1/statuses/update.json";
            string ParamString =string.Format(
             "{0}&{1}&{2}",
             httpmethod,
             Uri.EscapeDataString(url),
             Uri.EscapeDataString(param.ToString())
         );
            ConstructOauthsign(ParamString);
            String Header = ConstructHeader();
          await  HTTPREQUESTPOSTURLENCODED(Header, url);
        }
        public static async Task HTTPREQUESTPOSTURLENCODED(string Auth, string url)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Auth);
            var formdata = new FormUrlEncodedContent(foo.Where(kvp => !kvp.Key.StartsWith("oauth_")));
            var response = await client.PostAsync(url, formdata);
          var  responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);

        }
        public static string ConstructHeader()
        {
            string header = "OAuth " + string.Join(
        ", ",
       foo
            .Where(kvp => kvp.Key.StartsWith("oauth_"))
            .Select(kvp => string.Format("{0}=\"{1}\"", kvp.Key, kvp.Value))
            .OrderBy(s => s)
    );
            return header;

        }
        public static void ConstructOauthsign(string ParamString)
        {
           



          
        

            byte[] byteArray = myhmacsha1.ComputeHash(Encoding.ASCII.GetBytes(ParamString));
            string oauth_sign = Convert.ToBase64String(byteArray);
            foo.Add("oauth_signature", Uri.EscapeDataString(oauth_sign));
        }
        public static void IntializeDictionary()
        {
            Random rnd = new Random();
            Byte[] b = new Byte[32];
            rnd.NextBytes(b);
            string base64 = Convert.ToBase64String(b);
            string oauth_nonce = Regex.Replace(base64, "[^a-zA-Z0-9]", String.Empty);

            DateTime PreviousDateTime = new DateTime(1970, 1, 1);
            string oauth_timestamp = ((int)(DateTime.UtcNow - PreviousDateTime).TotalSeconds).ToString();
            foo = new Dictionary<String, String>();
               foo.Add("oauth_consumer_key", oauth_consumer_key);
            foo.Add("oauth_signature_method", "HMAC-SHA1");
            foo.Add("oauth_timestamp", oauth_timestamp);
            foo.Add("oauth_nonce", oauth_nonce);
            foo.Add("oauth_token", oauth_token);
            foo.Add("oauth_version", "1.0");
        }

        public static string ConstructParameterstring(Dictionary<String, String> foo)
        {
            string str = string.Join(
          "&",
          foo
              .Union(foo)
              .Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))
              .OrderBy(s => s)
      );
            return str;

        }
public static void Intialize() {

    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();
         OAuth settings_oauth = new OAuth();
    config.GetSection("Oauth").Bind(settings_oauth);
    
 
           oauth_consumer_key=settings_oauth.oauth_consumer_key;
       CONSUMER_TOKEN=settings_oauth.CONSUMER_TOKEN;
        oauth_token=settings_oauth.oauth_token;
   ACCESS_TOKEN_SECRET= settings_oauth.ACCESS_TOKEN_SECRET;  
   signingkey=CONSUMER_TOKEN + "&" + ACCESS_TOKEN_SECRET;
  
        key = Encoding.ASCII.GetBytes(signingkey);
      myhmacsha1 = new HMACSHA1(key);

}
        public static async Task Main(string[] args)
        {
         
          Intialize();
            await Search(Uri.EscapeDataString("from:Thunderblunder7 AND -filter:retweets AND -filter:replies"),true); //need to urlencode it
       
            while (true)
            {

                Thread.Sleep(60*1000);
                Since = true;
                await Search(Uri.EscapeDataString("from:Thunderblunder7 AND -filter:retweets AND -filter:replies"), true); //need to urlencode it
            } 
            //await Retweet(searchkey);
           
 
         
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
