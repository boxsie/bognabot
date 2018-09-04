using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Config.Core;
using Bognabot.Config.Enums;
using Bognabot.Config.General;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Storage.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Bognabot.Data.Repository
{
    public class RepositoryService
    {
        private readonly ILogger<RepositoryService> _logger;
        private readonly GeneralConfig _generalConfig;
        private readonly List<ExchangeConfig> _exchangeConfigs;

        private List<string> _availableTables;

        public RepositoryService(ILogger<RepositoryService> logger, GeneralConfig generalConfig, IEnumerable<ExchangeConfig> exchangeConfigs)
        {
            _logger = logger;
            _generalConfig = generalConfig;
            _exchangeConfigs = exchangeConfigs.ToList();
            _availableTables = new List<string>();

            EnsureDbCreated();
            RegisterTableNames();
        }

        public async Task<IRepository<Candle>> GetCandleRepository(SupportedExchange exchange, Instrument instrument, TimePeriod period)
        {
            var repo = new Repository<Candle>(_logger);

            var tableName = GetCandleTableName(exchange, instrument, period);

            if (_availableTables.All(x => x != tableName))
                throw new IndexOutOfRangeException();

            await repo.LoadAsync(GetConnectionString(), GetCandleTableName(exchange, instrument, period));
            
            return repo;
        }

        private void EnsureDbCreated()
        {
            _logger.LogInformation($"Looking for database...");

            var dbPath = StorageUtils.PathCombine(Cfg.UserDataPath, _generalConfig.DbFilename);

            _logger.LogDebug($"Looking for database at {dbPath}");

            if (File.Exists(dbPath))
                return;

            _logger.LogInformation($"Database not found, creating...");

            SQLiteConnection.CreateFile(dbPath);

            _logger.LogInformation($"Database created");
        }

        private void RegisterTableNames()
        {
            _availableTables = new List<string>();

            var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>();
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();
            
            foreach (var instrument in instruments)
            {
                foreach (var exchange in _exchangeConfigs)
                {
                    var supportedPeriods = exchange.SupportedTimePeriods;

                    foreach (var period in supportedPeriods)
                    {
                        _availableTables.Add(GetCandleTableName(exchange.Exchange, instrument, period.Key));
                    }
                }
            }
        }

        private string GetConnectionString()
        {
            return $"Data Source={StorageUtils.PathCombine(Cfg.UserDataPath, _generalConfig.DbFilename)};";
        }

        private static string GetCandleTableName(SupportedExchange exchange, Instrument instrument, TimePeriod period)
        {
            return $"{exchange}_{instrument}_{period}_Candles";
        }
    }
}