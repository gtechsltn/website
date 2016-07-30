﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/tools</c> resource.
    /// </summary>
    public class ToolsController : Controller
    {
        /// <summary>
        /// Gets the result for the <c>/tools</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/tools</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();
    }
}
