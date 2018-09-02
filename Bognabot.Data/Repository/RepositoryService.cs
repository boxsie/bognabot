using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Config.Exchange;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Storage.Core;
using Microsoft.AspNetCore.Builder;

namespace Bognabot.Data.Repository
{
    public class RepositoryService
    {
        private List<string> _availableTables;

        public RepositoryService()
        {
            _availableTables = new List<string>();

            EnsureDbCreated();
            RegisterTableNames();
        }

        public async Task<IRepository<Candle>> GetCandleRepository(ExchangeType exchange, InstrumentType instrument, TimePeriod period)
        {
            var repo = new Repository<Candle>();

            var tableName = GetCandleTableName(exchange, instrument, period);

            if (_availableTables.All(x => x != tableName))
                throw new IndexOutOfRangeException();

            await repo.LoadAsync(GetConnectionString(), GetCandleTableName(exchange, instrument, period));
            
            return repo;
        }

        private void EnsureDbCreated()
        {
            var dbPath = StorageUtils.PathCombine(Cfg.AppDataPath, Cfg.General.App.DbFilename);

            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);
        }

        private void RegisterTableNames()
        {
            _availableTables = new List<string>();

            var instruments = Enum.GetValues(typeof(InstrumentType)).Cast<InstrumentType>();

            var exchangeConfigs = typeof(ExchangeAppConfig).GetProperties()
                .Where(x => x.PropertyType == typeof(ExchangeSpecificAppConfig))
                .ToDictionary(x => (ExchangeType)Enum.Parse(typeof(ExchangeType), x.Name),
                    y => (ExchangeSpecificAppConfig)y.GetValue(Cfg.Exchange.App));

            var connectionString = GetConnectionString();

            foreach (var instrument in instruments)
            {
                var supportedExchanges = exchangeConfigs
                    .Where(x => x.Value.SupportedInstruments
                        .Any(y => y.ToLower() == instrument.ToString().ToLower()));

                foreach (var exchange in supportedExchanges)
                    _availableTables.Add(GetCandleTableName(exchange.Key, instrument, TimePeriod.OneMinute));
            }
        }

        private static string GetConnectionString()
        {
            return $"Data Source={StorageUtils.PathCombine(Cfg.AppDataPath, Cfg.General.App.DbFilename)};";
        }

        private static string GetCandleTableName(ExchangeType exchange, InstrumentType instrument, TimePeriod period)
        {
            return $"{exchange}_{instrument}_{period}_Candles";
        }
    }
}