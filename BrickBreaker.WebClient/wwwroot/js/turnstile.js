(function () {
    const widgetMap = new Map();
    let scriptPromise;

    function ensureScript() {
        if (window.turnstile) {
            return Promise.resolve();
        }

        if (!scriptPromise) {
            scriptPromise = new Promise((resolve, reject) => {
                const existing = document.querySelector('script[data-turnstile-script]');
                if (existing) {
                    existing.addEventListener('load', () => resolve());
                    existing.addEventListener('error', reject);
                    return;
                }

                const script = document.createElement('script');
                script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
                script.async = true;
                script.defer = true;
                script.dataset.turnstileScript = 'true';
                script.onload = () => resolve();
                script.onerror = reject;
                document.head.appendChild(script);
            });
        }

        return scriptPromise;
    }

    async function render(elementId, siteKey, dotNetRef, callbackMethod, action) {
        await ensureScript();
        const host = document.getElementById(elementId);
        if (!host) {
            throw new Error(`Element '${elementId}' not found for Turnstile widget.`);
        }

        const options = {
            sitekey: siteKey,
            callback: (token) => dotNetRef.invokeMethodAsync(callbackMethod, elementId, token ?? null),
            'expired-callback': () => dotNetRef.invokeMethodAsync(callbackMethod, elementId, null),
            'error-callback': () => dotNetRef.invokeMethodAsync(callbackMethod, elementId, null)
        };

        if (action) {
            options.action = action;
        }

        const widgetId = window.turnstile.render(host, options);
        widgetMap.set(elementId, widgetId);
    }

    function reset(elementId) {
        if (!window.turnstile) {
            return;
        }

        const widgetId = widgetMap.get(elementId);
        if (widgetId !== undefined) {
            window.turnstile.reset(widgetId);
        }
    }

    window.turnstileInterop = {
        render,
        reset
    };
})();
