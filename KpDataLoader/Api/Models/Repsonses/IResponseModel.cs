namespace KpDataLoader.Api.Models.Repsonses;

public interface IResponseModel
{
    /// <summary>
    /// HTTP статус код ответа
    /// </summary>
    int StatusCode { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    string Error { get; set; }

    string Message { get; set; }
}