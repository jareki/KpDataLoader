using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Универсальная фабрика, которая создаёт экземпляры типа T с заданной вероятностью
/// </summary>
/// <typeparam name="T">Тип создаваемых объектов</typeparam>
public class ProbabilityFactory<T> where T : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _implementationTypes;
    private readonly List<(string Name, double ProbabilityThreshold)> _probabilityRanges;
    private readonly Random _random;

    public ProbabilityFactory(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this._implementationTypes = new Dictionary<string, Type>();
        this._probabilityRanges = new List<(string, double)>();
        this._random = new Random();
    }

    /// <summary>
    /// Регистрирует тип реализации T с указанной вероятностью выбора
    /// </summary>
    /// <param name="name">Уникальное имя реализации</param>
    /// <param name="probability">Вероятность выбора (от 0 до 1)</param>
    /// <param name="implementationType">Тип реализации T</param>
    /// <returns>Текущий экземпляр фабрики для цепочки вызовов</returns>
    public ProbabilityFactory<T> Register(string name, double probability, Type implementationType)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (probability < 0 || probability > 1)
            throw new ArgumentOutOfRangeException(nameof(probability), "Вероятность должна быть между 0 и 1");

        if (implementationType == null)
            throw new ArgumentNullException(nameof(implementationType));

        if (!typeof(T).IsAssignableFrom(implementationType))
            throw new ArgumentException($"Тип {implementationType.Name} не наследуется от {typeof(T).Name}", nameof(implementationType));

        if (this._implementationTypes.ContainsKey(name))
            throw new ArgumentException($"Реализация с именем '{name}' уже зарегистрирована", nameof(name));

        this._implementationTypes.Add(name, implementationType);
        this._probabilityRanges.Clear(); // Сбрасываем кэш диапазонов вероятностей

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
        if (this._implementationTypes.Count == 0)
            throw new InvalidOperationException($"Не зарегистрировано ни одной реализации {typeof(T).Name}");

        this.EnsureProbabilityRangesCalculated();

        double randomValue = this._random.NextDouble();

        foreach (var (name, threshold) in this._probabilityRanges)
        {
            if (randomValue <= threshold)
            {
                Type implementationType = this._implementationTypes[name];
                return (T)this._serviceProvider.GetRequiredService(implementationType);
            }
        }

        // Если из-за погрешностей вычислений мы не попали ни в один из диапазонов,
        // возвращаем последний вариант
        var lastName = this._probabilityRanges.Last().Name;
        Type lastImplementationType = this._implementationTypes[lastName];
        return (T)this._serviceProvider.GetRequiredService(lastImplementationType);
    }

    /// <summary>
    /// Проверяет, что сумма вероятностей равна 1
    /// </summary>
    public bool ValidateProbabilitySum()
    {
        double sum = this._implementationTypes.Keys.Sum(name => this.GetProbabilityForImplementation(name));
        // Используем допуск на погрешность вычислений
        return Math.Abs(sum - 1.0) < 0.000001;
    }

    /// <summary>
    /// Возвращает вероятность для указанной реализации
    /// </summary>
    /// <param name="name">Имя реализации</param>
    /// <returns>Вероятность выбора</returns>
    private double GetProbabilityForImplementation(string name)
    {
        int index = this._probabilityRanges.FindIndex(r => r.Name == name);
        if (index == 0)
            return this._probabilityRanges[0].ProbabilityThreshold;

        return this._probabilityRanges[index].ProbabilityThreshold - this._probabilityRanges[index - 1].ProbabilityThreshold;
    }

    /// <summary>
    /// Рассчитывает накопительные диапазоны вероятностей
    /// </summary>
    private void EnsureProbabilityRangesCalculated()
    {
        if (this._probabilityRanges.Count > 0)
            return;

        double cumulativeProbability = 0;

        // Собираем пары (имя, вероятность)
        var implementationProbabilities = new Dictionary<string, double>();
        foreach (var name in this._implementationTypes.Keys)
        {
            implementationProbabilities[name] = 0; // Инициализируем все нулями
        }

        // Заполняем вероятности из зарегистрированных реализаций
        foreach (var name in this._implementationTypes.Keys)
        {
            // Пока что берем равные вероятности, но потом будем заполнять правильно
            implementationProbabilities[name] = 1.0 / this._implementationTypes.Count;
        }

        // Сортируем по возрастанию вероятности
        var sortedImplementations = implementationProbabilities
            .OrderBy(pair => pair.Value)
            .ToList();

        foreach (var pair in sortedImplementations)
        {
            cumulativeProbability += pair.Value;
            this._probabilityRanges.Add((pair.Key, cumulativeProbability));
        }

        // Проверяем сумму вероятностей
        if (!this.ValidateProbabilitySum())
            throw new InvalidOperationException("Сумма вероятностей не равна 1");
    }
}

// Расширения для регистрации в DI