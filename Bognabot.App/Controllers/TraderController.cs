using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.App.ViewModels;
using Bognabot.Services.Exchange.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Bognabot.App.Controllers
{
    public class TraderController : Controller
    {
        private readonly IEnumerable<IExchangeService> _exchangeServices;

        public TraderController(IEnumerable<IExchangeService> exchangeServices)
        {
            _exchangeServices = exchangeServices;
        }

        public IActionResult Index()
        {
            var vm = new TraderIndexViewModel
            {
                ExchangeNames = _exchangeServices.Select(x => x.ExchangeConfig.ExchangeName).ToArray()
            };

            return View(vm);
        }
    }
}