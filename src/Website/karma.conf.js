// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

const puppeteer = require("puppeteer");
process.env.CHROME_BIN = puppeteer.executablePath();

module.exports = function (config) {
    config.set({

        autoWatch: false,
        concurrency: Infinity,

        browsers: ["ChromeHeadless"],
        frameworks: ["jasmine", "karma-typescript"],

        files: [
            "https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js",
            "https://cdnjs.cloudflare.com/ajax/libs/clipboard.js/1.6.1/clipboard.min.js",
            "Assets/Scripts/**/*.ts"
        ],

        preprocessors: {
            "**/*.ts": ["karma-typescript"]
        },

        karmaTypescriptConfig: {
            tsconfig: "tsconfig.json"
        },

        htmlDetailed: {
            splitResults: false
        },

        plugins: [
            "karma-appveyor-reporter",
            "karma-chrome-launcher",
            "karma-html-detailed-reporter",
            "karma-jasmine",
            "karma-typescript"
        ],

        reporters: [
            "progress",
            "appveyor",
            "karma-typescript"
        ]
    });
};
