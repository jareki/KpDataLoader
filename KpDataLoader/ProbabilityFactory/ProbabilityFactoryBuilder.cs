using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpDataLoader.ProbabilityFactory
{
    /// <summary>
    /// Обобщенный билдер для настройки ProbabilityFactory
    /// </summary>
    /// <typeparam name="T">Тип создаваемых объектов</typeparam>
    public class ProbabilityFactoryBuilder<T> where T : class
    {
        private readonly IServiceCollection _services;
        internal readonly List<(string Name, double Probability, Type Type)> Implementations;

        public ProbabilityFactoryBuilder(IServiceCollection services)
        {
            _services = services;
            Implementations = new List<(string, double, Type)>();
        }

        /// <summary>
        /// Добавляет реализацию с указанной вероятностью
        /// </summary>
        public ProbabilityFactoryBuilder<T> AddImplementation<TImplementation>(string name, double probability)
            where TImplementation : class, T
        {
            // Регистрируем тип в DI
            _services.AddTransient<TImplementation>();

            // Обеспечиваем резолвинг по конкретному типу
            _services.AddTransient(typeof(TImplementation));

            // Добавляем информацию о реализации
            Implementations.Add((name, probability, typeof(TImplementation)));

            return this;
        }

        /// <summary>
        /// Проверяет, что сумма вероятностей равна 1
        /// </summary>
        public bool ValidateProbabilitySum()
        {
            double sum = Implementations.Sum(w => w.Probability);
            return Math.Abs(sum - 1.0) < 0.000001;
        }
    }
}
