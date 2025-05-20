// utils/event-handler.js
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.events = {
        // Registrar evento con manejo centralizado
        on: function (selector, eventType, handler) {
            $(document).on(eventType, selector, handler);
        },

        // Registrar evento una sola vez
        once: function (selector, eventType, handler) {
            $(document).one(eventType, selector, handler);
        },

        // Eliminar eventos
        off: function (selector, eventType) {
            $(document).off(eventType, selector);
        },

        // Disparar evento
        trigger: function (selector, eventType, data) {
            $(selector).trigger(eventType, data);
        },

        // Delegar eventos con opciones avanzadas
        delegate: function (options) {
            const defaults = {
                selector: '',
                event: 'click',
                prevent: true,
                stop: false,
                data: null,
                handler: function () { }
            };

            const settings = $.extend({}, defaults, options);

            $(document).on(settings.event, settings.selector, function (e) {
                if (settings.prevent) e.preventDefault();
                if (settings.stop) e.stopPropagation();

                settings.handler.call(this, e, settings.data);
            });
        }
    };

})(window, jQuery);