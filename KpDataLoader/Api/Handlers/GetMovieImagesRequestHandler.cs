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
    public class GetMovieImagesRequestHandler: KpRequestHandler<GetMovieImagesRequestModel,GetMovieImagesResponseModel>
    {
        public GetMovieImagesRequestHandler(
            IHttpClientService httpClientService, 
            string method = "/v1.4/image") : base(httpClientService, method)
        {
        }
    }
}
