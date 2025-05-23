export async function loadDropdown(url, params, selector, { placeholder = 'Seleccione...' } = {}) {
    const selectEl = document.querySelector(selector);
    if (!selectEl) return;

    // Vaciar y agregar opción placeholder
    selectEl.innerHTML = '';
    const placeholderOption = document.createElement('option');
    placeholderOption.value = '';
    placeholderOption.textContent = placeholder;
    selectEl.appendChild(placeholderOption);

    // Construir query string
    const query = new URLSearchParams(params).toString();
    try {
        const response = await fetch(`${url}?${query}`);
        if (!response.ok) throw new Error(response.statusText);
        const items = await response.json();

        items.forEach(item => {
            const opt = document.createElement('option');
            opt.value = item.id;
            opt.textContent = item.nombre || item.text || item.value;
            selectEl.appendChild(opt);
        });
    } catch (err) {
        console.error('Error cargando dropdown:', err);
    }
}