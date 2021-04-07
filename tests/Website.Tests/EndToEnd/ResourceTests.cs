﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.EndToEnd
{
    using System.Net;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class ResourceTests : EndToEndTest
    {
        public ResourceTests(WebsiteFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [SkippableTheory]
        [InlineData("/", "text/html")]
        [InlineData("/apple-app-site-association", "application/json")]
        [InlineData("/assets/css/site.css", "text/css")]
        [InlineData("/assets/css/site.css.map", "text/plain")]
        [InlineData("/assets/css/site.min.css", "text/css")]
        [InlineData("/assets/css/site.min.css.map", "text/plain")]
        [InlineData("/assets/js/site.js", "application/javascript")]
        [InlineData("/assets/js/site.js.map", "text/plain")]
        [InlineData("/assets/js/site.min.js", "application/javascript")]
        [InlineData("/assets/js/site.min.js.map", "text/plain")]
        [InlineData("/assets/js/site.tools.js", "application/javascript")]
        [InlineData("/assets/js/site.tools.js.map", "text/plain")]
        [InlineData("/assets/js/site.tools.min.js", "application/javascript")]
        [InlineData("/assets/js/site.tools.min.js.map", "text/plain")]
        [InlineData("/.well-known/apple-app-site-association", "application/json")]
        [InlineData("/.well-known/assetlinks.json", "application/json")]
        [InlineData("BingSiteAuth.xml", "text/xml")]
        [InlineData("browserconfig.xml", "text/xml")]
        [InlineData("/bad-request.html", "text/html")]
        [InlineData("/error.html", "text/html")]
        [InlineData("/favicon.ico", "image/x-icon")]
        [InlineData("/googled1107923138d0b79.html", "text/html")]
        [InlineData("/home/about", "text/html")]
        [InlineData("/home/about/", "text/html")]
        [InlineData("/HOME/ABOUT", "text/html")]
        [InlineData("/humans.txt", "text/plain")]
        [InlineData("/keybase.txt", "text/plain")]
        [InlineData("/manifest.webmanifest", "application/manifest+json")]
        [InlineData("/not-found.html", "text/html")]
        [InlineData("/projects", "text/html")]
        [InlineData("/robots.txt", "text/plain")]
        [InlineData("/service-worker.js", "application/javascript")]
        [InlineData("/sitemap.xml", "text/xml")]
        [InlineData("/tools", "text/html")]
        public async Task Can_Load_Resource_As_Get(string requestUri, string contentType)
        {
            // Arrange
            using var client = Fixture.CreateClient();

            // Act
            using var response = await client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.ShouldNotBeNull();
            response.Content!.Headers.ContentType?.MediaType?.ShouldBe(contentType);
        }

        [SkippableFact]
        public async Task Response_Headers_Contains_Expected_Headers()
        {
            // Arrange
            string[] expectedHeaders =
            {
                "content-security-policy",
                "content-security-policy-report-only",
                "feature-policy",
                "Referrer-Policy",
                "X-Content-Type-Options",
                "X-Datacenter",
                "X-Download-Options",
                "X-Frame-Options",
                "X-Instance",
                "X-Request-Id",
                "X-Revision",
                "X-XSS-Protection",
            };

            using var client = Fixture.CreateClient();

            // Act
            using var response = await client.GetAsync("/");

            // Assert
            foreach (string expected in expectedHeaders)
            {
                response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
            }
        }
    }
}
