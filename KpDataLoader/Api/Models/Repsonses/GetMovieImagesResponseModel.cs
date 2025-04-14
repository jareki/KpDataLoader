using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KpDataLoader.Api.Models.Repsonses
{
    public class GetMovieImagesResponseModel: BaseResponseModel
    {
        [JsonPropertyName("docs")]
        public Doc Docs { get; set; }
    }

    public class Doc
    {
        [JsonPropertyName("movieId")]
        public int MovieId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
