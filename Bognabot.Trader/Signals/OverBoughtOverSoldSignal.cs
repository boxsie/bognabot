﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Trader.Enums;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Trader.Indicators;

namespace Bognabot.Trader.Signals
{
    public interface IStrategy
    {

    }

    public class ADXBackedSMACross : IStrategy
    {

    }
    
    public class OverBoughtOverSoldSignal : ISignal
    {
        public Task<bool> IsPeriodSupportedAsync(TimePeriod period)
        {
            return Task.FromResult(true);
        }

        public Task<SignalStrength> ProcessSignalAsync(TimePeriod timePeriod, CandleDto[] candles)
        {
            var cmo = new CMO().Calculate(candles, 9);
            var cci = new CCI().Calculate(candles, 20);
            var mfi = new MFI().Calculate(candles, 14);

            var cmoUnit = TraderUtils.NormaliseAndClamp(cmo.First(), -50, 50);
            var cciUnit = TraderUtils.NormaliseAndClamp(cci.First(), -100, 100);
            var mfiUnit = TraderUtils.NormaliseAndClamp(mfi.First(), 10, 80);

            var avg = (cmoUnit + cciUnit + mfiUnit) / 3;

            return Task.FromResult(TraderUtils.ToSignalStrength(avg));
        }
    }
}