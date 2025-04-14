using KpDataLoader.ProbabilityFactory;
using Microsoft.Extensions.DependencyInjection;

public static class ProbabilityFactoryServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику ProbabilityFactory для заданного типа T
    /// </summary>
    public static IServiceCollection AddProbabilityFactory<T>(
        this IServiceCollection services,
        Action<ProbabilityFactoryBuilder<T>> configure) where T : class
    {
        // Создаем билдер для конфигурации
        var builder = new ProbabilityFactoryBuilder<T>(services);

        // Вызываем конфигурационный метод
        configure(builder);

        // Проверяем сумму вероятностей
        if (!builder.ValidateProbabilitySum())
            throw new InvalidOperationException($"Сумма вероятностей для типа {typeof(T).Name} не равна 1");

        // Регистрируем фабрику как Scoped
        services.AddScoped(provider =>
        {
            var factory = new ProbabilityFactory<T>(provider);

            // Добавляем все реализации из билдера
            foreach (var (name, probability, type) in builder.Implementations)
            {
                factory.Register(name, probability, type);
            }

            return factory;
        });

        return services;
    }
}