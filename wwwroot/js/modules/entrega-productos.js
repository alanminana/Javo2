// entrega-productos.js - Módulo para entrega de productos
(function (window, $) {
    'use strict';

    var App = window.App = window.App || {};

    App.entregaProductos = {
        selectedId: null,

        init: function () {
            this.setupEventHandlers();
        },

        // Configurar manejadores de eventos
        setupEventHandlers: function () {
            const self = this;

            // Botones de marcar entregado
            document.querySelectorAll('.mark-delivered').forEach(btn => {
                btn.addEventListener('click', () => {
                    self.selectedId = btn.dataset.id;
                    var modal = new bootstrap.Modal(document.getElementById('confirmDeliveryModal'));
                    modal.show();
                });
            });

            // Confirmar entrega
            document.getElementById('confirmDeliveryBtn').addEventListener('click', () => {
                self.marcarEntregada();
            });
        },

        // Marcar venta como entregada
        marcarEntregada: function () {
            if (!this.selectedId) return;

            fetch('/Ventas/MarcarEntregada', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ id: this.selectedId })
            })
                .then(res => res.json())
                .then(resp => {
                    if (resp.success) {
                        location.reload();
                    } else {
                        App.notify.error(resp.message || 'Error al procesar entrega');
                    }
                })
                .catch(() => {
                    App.notify.error('Error de red al procesar la solicitud');
                });
        }
    };

})(window, jQuery);