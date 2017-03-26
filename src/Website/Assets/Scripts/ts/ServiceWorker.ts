﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(() => {
    if ("serviceWorker" in navigator) {
        (navigator as any).serviceWorker
            .register("/service-worker.js")
            .then(() => {
            })
            .catch((e: any) => {
                console.error("Failed to register Service Worker: ", e);
            });
    }
})();
