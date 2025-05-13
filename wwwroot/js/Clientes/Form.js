        $(function() {
            // Manejo de Provincias y Ciudades
            $("#provincias").change(function() {
                var provinciaId = $(this).val();
                if (provinciaId) {
                    $.ajax({
                        url: "@Url.Action("GetCiudades")",
                        type: "GET",
                        data: { provinciaID: provinciaId },
                        success: function(data) {
                            var ciudades = $("#ciudades");
                            ciudades.empty();
                            ciudades.append('<option value="">-- Seleccione una ciudad --</option>');
                            $.each(data, function(i, item) {
                                ciudades.append(`<option value="${item.value}">${item.text}</option>`);
                            });
                        },
                        error: function(xhr, status, error) {
                            console.error("Error al cargar ciudades:", error);
                            alert("Error al cargar las ciudades. Por favor, intente nuevamente.");
                        }
                    });
                } else {
                    $("#ciudades").empty().append('<option value="">-- Seleccione una ciudad --</option>');
                }
            });

            // Si ya hay una provincia seleccionada, cargar las ciudades
            if ($("#provincias").val()) {
                $("#provincias").trigger("change");
            }

            // Manejo de Crédito y Garante
            const toggleCreditoFields = function() {
                const aptoCredito = $("#aptoCredito").is(":checked");
                $(".credito-field").toggleClass("d-none", !aptoCredito);

                const requiereGarante = $("#requiereGarante").is(":checked");
                $("#garanteInfo").toggleClass("d-none", !(aptoCredito && requiereGarante));
            };

            $("#aptoCredito, #requiereGarante").change(toggleCreditoFields);

            // Inicializar campos de crédito
            toggleCreditoFields();
        });
