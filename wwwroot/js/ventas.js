// Fixed ventas.js - Save to wwwroot/js/ventas.js

$(document).ready(function () {
    console.log("Ventas.js loaded");

    // Helper function to get the anti-forgery token
    function getAntiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').val();
    }

    // Autorizar venta
    $(document).on('click', '.autorizar-venta', function (e) {
        e.preventDefault();
        const ventaId = $(this).data('id');
        const $row = $(this).closest('tr');
        const token = getAntiForgeryToken();

        console.log("Authorize clicked for ID:", ventaId);
        console.log("Token:", token);

        if (!token) {
            console.error("Anti-forgery token not found!");
            alert("Error: Missing security token. Please refresh the page and try again.");
            return;
        }

        if (confirm('¿Está seguro que desea autorizar esta venta?')) {
            $.ajax({
                url: '/Ventas/Autorizar',
                type: 'POST',
                data: { id: ventaId, __RequestVerificationToken: token },
                headers: {
                    'RequestVerificationToken': token
                },
                success: function (response) {
                    console.log("Success response:", response);
                    if (response.success) {
                        $row.fadeOut(400, function () {
                            $(this).remove();
                            if ($('table tbody tr').length === 0) {
                                location.reload();
                            } else {
                                alert("Venta autorizada correctamente");
                            }
                        });
                    } else {
                        alert(response.message || 'Error al autorizar la venta.');
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Error details:", { xhr, status, error });

                    // Try to parse any JSON response
                    let errorMessage = 'Error al procesar la solicitud.';
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response && response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        // If parsing fails, use status text
                        errorMessage = xhr.statusText || errorMessage;
                    }

                    alert('Error: ' + errorMessage);
                }
            });
        }
    });

    // Rechazar venta
    $(document).on('click', '.rechazar-venta', function (e) {
        e.preventDefault();
        const ventaId = $(this).data('id');
        const $row = $(this).closest('tr');
        const token = getAntiForgeryToken();

        console.log("Reject clicked for ID:", ventaId);
        console.log("Token:", token);

        if (!token) {
            console.error("Anti-forgery token not found!");
            alert("Error: Missing security token. Please refresh the page and try again.");
            return;
        }

        if (confirm('¿Está seguro que desea rechazar esta venta?')) {
            $.ajax({
                url: '/Ventas/Rechazar',
                type: 'POST',
                data: { id: ventaId, __RequestVerificationToken: token },
                headers: {
                    'RequestVerificationToken': token
                },
                success: function (response) {
                    console.log("Success response:", response);
                    if (response.success) {
                        $row.fadeOut(400, function () {
                            $(this).remove();
                            if ($('table tbody tr').length === 0) {
                                location.reload();
                            } else {
                                alert("Venta rechazada correctamente");
                            }
                        });
                    } else {
                        alert(response.message || 'Error al rechazar la venta.');
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Error details:", { xhr, status, error });

                    // Try to parse any JSON response
                    let errorMessage = 'Error al procesar la solicitud.';
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response && response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        // If parsing fails, use status text
                        errorMessage = xhr.statusText || errorMessage;
                    }

                    alert('Error: ' + errorMessage);
                }
            });
        }
    });

    // Marcar como entregada
    $('.marcar-entregada').click(function () {
        const ventaId = $(this).data('id');
        $('#confirmarEntregaModal').modal('show');
        $('#confirmarEntrega').data('id', ventaId);
    });

    // Confirmar entrega
    $('#confirmarEntrega').click(function () {
        const ventaId = $(this).data('id');
        const token = getAntiForgeryToken();

        console.log("Confirm delivery clicked for ID:", ventaId);

        if (!token) {
            console.error("Anti-forgery token not found!");
            alert("Error: Missing security token. Please refresh the page and try again.");
            return;
        }

        $('#confirmarEntregaModal').modal('hide');

        $.ajax({
            url: '/Ventas/MarcarEntregada',
            type: 'POST',
            data: { id: ventaId, __RequestVerificationToken: token },
            headers: {
                'RequestVerificationToken': token
            },
            success: function (response) {
                if (response.success) {
                    alert('Venta marcada como entregada exitosamente');
                    // Eliminar la fila de la tabla
                    $(`button[data-id="${ventaId}"]`).closest('tr').fadeOut(400, function () {
                        $(this).remove();
                        // Si no quedan más ventas, recargar la página
                        if ($('.table tbody tr').length === 0) {
                            location.reload();
                        }
                    });
                } else {
                    alert(response.message || 'Error al marcar la venta como entregada');
                }
            },
            error: function (xhr, status, error) {
                console.error("Error details:", { xhr, status, error });
                alert('Error al procesar la solicitud.');
            }
        });
    });
});