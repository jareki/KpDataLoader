using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;
using KpDataLoader.Http;

namespace KpDataLoader.Api.Handlers
{
    public class GetRandomMovieRequestHandler: KpRequestHandler<GetRandomMovieRequestModel,GetRandomMovieResponseModel>
    {
        public GetRandomMovieRequestHandler(
            IHttpClientService httpClientService, 
            string method = "/v1.4/movie/random") : base(httpClientService, method)
        {
        }
    }
}
