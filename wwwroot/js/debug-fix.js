// debug-fix.js - Solución para el problema de i.debug
(function (window) {
    'use strict';

    // Asegurar que App existe
    window.App = window.App || {};

    // Guardar cualquier definición previa de debug
    var originalDebug = window.App.debug;

    // Crear una función de debug que mantenga compatibilidad
    function debugFunction(message, data) {
        if (window.App.config && window.App.config.debug) {
            console.log(message, data || '');
        }
    }

    // Agregar propiedades del objeto debug a la función
    debugFunction.init = function () { };
    debugFunction.log = debugFunction;
    debugFunction.error = function (msg, data) { console.error(msg, data || ''); };

    // Asignar la función a App.debug
    window.App.debug = debugFunction;

    // Preservar cualquier propiedad previa
    if (originalDebug && typeof originalDebug === 'object') {
        for (var prop in originalDebug) {
            if (originalDebug.hasOwnProperty(prop) && !window.App.debug[prop]) {
                window.App.debug[prop] = originalDebug[prop];
            }
        }
    }
})(window);