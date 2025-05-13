
        $(function() {
            const tipoDato = '@Model.TipoDato';
            const valorInput = $('#Valor');

            valorInput.on('input', function() {
                let isValid = true;
                const valor = $(this).val();

                if (tipoDato === 'int') {
                    isValid = /^\d+$/.test(valor);
                } else if (tipoDato === 'decimal') {
                    isValid = /^\d+(\.\d+)?$/.test(valor);
                }

                if (isValid) {
                    $(this).removeClass('is-invalid').addClass('is-valid');
                } else {
                    $(this).removeClass('is-valid').addClass('is-invalid');
                }
            });
        });
