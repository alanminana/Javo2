
        let selectedId = null;
        function reload() { location.reload(); }
        document.querySelectorAll('.mark-delivered').forEach(btn => {
            btn.addEventListener('click', () => {
                selectedId = btn.dataset.id;
                var modal = new bootstrap.Modal(document.getElementById('confirmDeliveryModal'));
                modal.show();
            });
        });
        document.getElementById('confirmDeliveryBtn').addEventListener('click', () => {
            fetch('@Url.Action("MarcarEntregada", "Ventas")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ id: selectedId })
            }).then(res => res.json())
              .then(resp => {
                  if (resp.success) {
                      location.reload();
                  } else {
                      alert(resp.message || 'Error al procesar entrega');
                  }
              }).catch(() => alert('Error de red'));
        });
