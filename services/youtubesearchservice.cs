using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Services;
using Youtube.Models;

using Google.Apis.YouTube.v3;
namespace TwitterBot.Services
{
    public class YoutubeService 
    {
        private readonly string ApiKey;
        private string newestVidDate;
        public YoutubeService(string ApiKey)
        {
            this.ApiKey = ApiKey;
            this.newestVidDate=  DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

         public async Task<YoutubeVideo> Search()
    {
      var youtubeService = new YouTubeService(new BaseClientService.Initializer()
      {
        ApiKey = this.ApiKey,
        ApplicationName = this.GetType().ToString()
      });

      var searchListRequest = youtubeService.Search.List("snippet");
   
        searchListRequest.ChannelId = "UC-tIwhZwjGMTCEBUVuAG5fA";
	searchListRequest.PublishedAfter=newestVidDate;

searchListRequest.Order=SearchResource.ListRequest.OrderEnum.Date;




      // Call the search.list method to retrieve results matching the specified query term.
      var searchListResponse = await searchListRequest.ExecuteAsync();
      
   

      // Add each result to the appropriate list, and then display the lists of
    
      foreach (var searchResult in searchListResponse.Items)
      {
     
        
        
       if(searchResult.Id.Kind=="youtube#video")
          {
              YoutubeVideo NewVID= new YoutubeVideo(){ 
                  
                        VidID= searchResult.Id.VideoId,
                           Title =searchResult.Snippet.Title,
              };
              this.newestVidDate=searchResult.Snippet.PublishedAt;
                  
                  return NewVID;


          }
       
        
      }

return null;
  
    }
    }
}