// Add this script to your Ventas views to help debug the AJAX issues
// Save as ~/wwwroot/js/debug-helper.js

(function () {
    // Log all AJAX requests and responses for debugging
    $(document).ajaxSend(function (event, jqXHR, settings) {
        console.log('AJAX Request:', {
            url: settings.url,
            type: settings.type,
            data: settings.data,
            headers: settings.headers
        });
    });

    $(document).ajaxComplete(function (event, jqXHR, settings) {
        console.log('AJAX Response:', {
            status: jqXHR.status,
            statusText: jqXHR.statusText,
            responseText: jqXHR.responseText,
            url: settings.url
        });
    });

    // Check if anti-forgery token is present and log its value
    $(document).ready(function () {
        const token = $('input[name="__RequestVerificationToken"]').val();
        console.log('Anti-forgery token found:', token ? 'Yes' : 'No', token);
    });
})();