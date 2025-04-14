using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;

namespace KpDataLoader.Api.Handlers;

/// <summary>
/// Интерфейс для обработчиков запросов
/// </summary>
/// <typeparam name="TRequest">Тип запроса</typeparam>
/// <typeparam name="TResponse">Тип ответа</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequestModel
    where TResponse : IResponseModel
{
    /// <summary>
    /// Выполняет запрос и возвращает ответ
    /// </summary>
    /// <param name="request">Запрос для выполнения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Ответ на запрос</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}