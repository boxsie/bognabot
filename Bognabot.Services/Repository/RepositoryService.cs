using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Data;
using Bognabot.Data.Config;
using Bognabot.Data.Config.Contracts;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange;
using Bognabot.Storage.Core;
using NLog;
using IExchangeService = Bognabot.Services.Exchange.IExchangeService;

namespace Bognabot.Services.Repository
{
    public class RepositoryService
    {
        private readonly ILogger _logger;
        private readonly GeneralConfig _generalConfig;

        private List<string> _availableTables;

        public RepositoryService(ILogger logger, GeneralConfig generalConfig, IEnumerable<IExchangeService> exchanges)
        {
            _logger = logger;
            _generalConfig = generalConfig;
            _availableTables = new List<string>();

            EnsureDbCreated();
            RegisterTableNames(exchanges.Select(x => x.ExchangeConfig).ToList());
        }

        public async Task<Repository<Candle>> GetCandleRepositoryAsync(string exchangeName, Instrument instrument, TimePeriod period)
        {
            var repo = new Repository<Candle>(_logger);

            var tableName = ExchangeUtils.GetCandleDataKey(exchangeName, instrument, period);

            if (_availableTables.All(x => x != tableName))
                throw new IndexOutOfRangeException();

            await repo.CreateTable(GetConnectionString(), ExchangeUtils.GetCandleDataKey(exchangeName, instrument, period));
            
            return repo;
        }

        private void EnsureDbCreated()
        {
            _logger.Log(LogLevel.Info, $"Looking for database...");

            var dbPath = StorageUtils.PathCombine(Cfg.UserDataPath, _generalConfig.DbFilename);

            _logger.Log(LogLevel.Info, $"Looking for database at {dbPath}");

            if (File.Exists(dbPath))
                return;

            _logger.Log(LogLevel.Info, $"Database not found, creating...");

            SQLiteConnection.CreateFile(dbPath);

            _logger.Log(LogLevel.Info, $"Database created");
        }

        private void RegisterTableNames(IReadOnlyCollection<ExchangeConfig> exchangeConfigs)
        {
            _availableTables = new List<string>();

            var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>();
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();
            
            foreach (var instrument in instruments)
            {
                foreach (var exchange in exchangeConfigs)
                {
                    var supportedPeriods = exchange.SupportedTimePeriods;

                    foreach (var period in supportedPeriods)
                    {
                        _availableTables.Add(ExchangeUtils.GetCandleDataKey(exchange.ExchangeName, instrument, period.Key));
                    }
                }
            }
        }

        private string GetConnectionString()
        {
            return $"Data Source={StorageUtils.PathCombine(Cfg.UserDataPath, _generalConfig.DbFilename)};";
        }
    }
}