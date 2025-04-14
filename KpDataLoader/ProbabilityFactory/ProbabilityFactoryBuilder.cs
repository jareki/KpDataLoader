using Microsoft.Extensions.DependencyInjection;

namespace KpDataLoader.ProbabilityFactory
{
    /// <summary>
    /// Обобщенный билдер для настройки ProbabilityFactory
    /// </summary>
    /// <typeparam name="T">Тип создаваемых объектов</typeparam>
    public class ProbabilityFactoryBuilder<T> where T : class
    {
        private readonly IServiceCollection _services;
        internal readonly List<ImplementProbabilityType> Implementations;

        public ProbabilityFactoryBuilder(IServiceCollection services)
        {
            this._services = services;
            this.Implementations = new List<ImplementProbabilityType>();
        }

        /// <summary>
        /// Добавляет реализацию с указанной вероятностью
        /// </summary>
        public ProbabilityFactoryBuilder<T> AddImplementation<TImplementation>(string name, double probability)
            where TImplementation : class, T
        {
            // Регистрируем тип в DI
            this._services.AddTransient<TImplementation>();

            // Обеспечиваем резолвинг по конкретному типу
            this._services.AddTransient(typeof(TImplementation));

            // Добавляем информацию о реализации
            this.Implementations.Add(
                new ImplementProbabilityType()
                    {
                        Name = name,
                        Probability = probability,
                    Type = typeof(TImplementation),
                    Treshold = 0
                });

            return this;
        }

        /// <summary>
        /// Проверяет, что сумма вероятностей равна 1
        /// </summary>
        public bool ValidateProbabilitySum()
        {
            double sum = this.Implementations.Sum(t => t.Probability);
            return Math.Abs(sum - 1.0) < 0.0001;
        }
    }
}
