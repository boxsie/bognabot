﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Bognabot.App.Controllers
{
    public class IndicatorsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}