namespace KpDataLoader.Api.Models.Repsonses
{
    public abstract class BaseResponseModel: IResponseModel
    {
        public int StatusCode { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }
}
