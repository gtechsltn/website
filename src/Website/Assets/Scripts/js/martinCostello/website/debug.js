﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/**
 * Defines the namespace for debugging.
 */
martinCostello.website.debug = {
    branch: $("meta[name='x-site-branch']").attr("content"),
    revision: $("meta[name='x-site-revision']").attr("content")
};

/**
 * Logs a message.
 * @param {String} message - The message to log.
 * @param {Object} [optionalParams] - The optional parameters to log.
 */
martinCostello.website.debug.log = function (message, optionalParams) {
    console.log(message, optionalParams);
};
