using KpDataLoader.ProbabilityFactory;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Универсальная фабрика, которая создаёт экземпляры типа T с заданной вероятностью
/// </summary>
/// <typeparam name="T">Тип создаваемых объектов</typeparam>
public class ProbabilityFactory<T> where T : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ImplementProbabilityType> _types;
    private readonly Random _random;

    public ProbabilityFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this._types = new List<ImplementProbabilityType>();
        this._random = new Random();
    }

    /// <summary>
    /// Регистрирует тип реализации T с указанной вероятностью выбора
    /// </summary>
    /// <param name="name">Уникальное имя реализации</param>
    /// <param name="probability">Вероятность выбора (от 0 до 1)</param>
    /// <param name="implementationType">Тип реализации T</param>
    /// <returns>Текущий экземпляр фабрики для цепочки вызовов</returns>
    public ProbabilityFactory<T> Register(
        string name, 
        double probability, 
        Type implementationType)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (probability is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(probability), "Вероятность должна быть между 0 и 1");

        if (implementationType == null)
            throw new ArgumentNullException(nameof(implementationType));

        if (!typeof(T).IsAssignableFrom(implementationType))
            throw new ArgumentException($"Тип {implementationType.Name} не наследуется от {typeof(T).Name}", nameof(implementationType));

        if (this._types.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Реализация с именем '{name}' уже зарегистрирована", nameof(name));

        this._types.Add(
            new ImplementProbabilityType()
            {
                Name = name,
                Type = implementationType,
                Probability = probability,
                Treshold = 0
            });

        return this;
    }

    /// <summary>
    /// Обобщенная версия метода Register для удобства использования
    /// </summary>
    public ProbabilityFactory<T> Register<TImplementation>(string name, double probability) where TImplementation : class, T
    {
        return this.Register(name, probability, typeof(TImplementation));
    }

    /// <summary>
    /// Регистрирует список реализаций с их вероятностями
    /// </summary>
    /// <param name="registrations">Список кортежей (имя, вероятность, тип реализации)</param>
    /// <returns>Текущий экземпляр фабрики для цепочки вызовов</returns>
    public ProbabilityFactory<T> RegisterMany(IEnumerable<(string Name, double Probability, Type Type)> registrations)
    {
        foreach (var (name, probability, type) in registrations)
        {
            this.Register(name, probability, type);
        }

        return this;
    }

    /// <summary>
    /// Возвращает случайную реализацию T в соответствии с заданными вероятностями
    /// </summary>
    /// <returns>Экземпляр T</returns>
    public T Create()
    {
        if (this._types.Count == 0)
            throw new InvalidOperationException($"Не зарегистрировано ни одной реализации {typeof(T).Name}");

        this.EnsureProbabilityRangesCalculated();

        double randomValue = this._random.NextDouble();

        foreach (var type in this._types)
        {
            if (randomValue <= type.Treshold)
            {
                Type implementationType = type.Type;
                return (T)this._serviceProvider.GetRequiredService(implementationType);
            }
        }

        // Если из-за погрешностей вычислений мы не попали ни в один из диапазонов,
        // возвращаем последний вариант
        var lastType = this._types.Last();
        Type lastImplementationType = lastType.Type;
        return (T)this._serviceProvider.GetRequiredService(lastImplementationType);
    }

    /// <summary>
    /// Проверяет, что сумма вероятностей равна 1
    /// </summary>
    private bool ValidateProbabilitySum()
    {
        double sum = this._types.Sum(t => t.Probability);
        // Используем допуск на погрешность вычислений
        return Math.Abs(sum - 1.0) < 0.0001;
    }

    /// <summary>
    /// Рассчитывает накопительные диапазоны вероятностей
    /// </summary>
    private void EnsureProbabilityRangesCalculated()
    {
        if (this._types.Count == 0)
        {
            return;
        }

        double cumulativeProbability = 0;
        
        int index = 0;
        int count = this._types.Count;
        foreach (var type in this._types)
        {
            cumulativeProbability += type.Probability;
            if (index++ == count - 1)
            {
                cumulativeProbability = 1.0;
            }

            type.Treshold = cumulativeProbability;
        }

        // Проверяем сумму вероятностей
        if (!this.ValidateProbabilitySum())
            throw new InvalidOperationException("Сумма вероятностей не равна 1");
    }
}
