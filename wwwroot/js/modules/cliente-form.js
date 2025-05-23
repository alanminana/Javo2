import { loadDropdown } from '../utils/dropdown.js';
import { ajaxPost, showModal, bindEnter } from '../utils/utils.js';

const clienteForm = {
    init() {
        this.initProvinciasCiudades();
        this.initCreditoFields();
        this.initGarante();
        this.bindEnterSubmit();
    },

    initProvinciasCiudades() {
        const provSelector = '#provincias';
        const citySelector = '#ciudades';
        // Cargar provincias al iniciar (asumiendo provincias está precargado en HTML)
        document.querySelector(provSelector)
            .addEventListener('change', () => {
                const provinciaID = document.querySelector(provSelector).value;
                loadDropdown('/Clientes/GetCiudades', { provinciaID }, citySelector, { placeholder: 'Seleccione ciudad...' });
            });

        // Trigger inicial si hay valor por defecto
        const initialProv = document.querySelector(provSelector).value;
        if (initialProv) {
            loadDropdown('/Clientes/GetCiudades', { provinciaID: initialProv }, citySelector, { placeholder: 'Seleccione ciudad...' });
        }
    },

    initCreditoFields() {
        // Ejemplo: mostrar/ocultar campos de crédito según checkbox
        const chkCredito = document.querySelector('#tieneCredito');
        const creditoFields = document.querySelector('#creditoFields');
        if (!chkCredito || !creditoFields) return;

        const toggle = () => {
            creditoFields.style.display = chkCredito.checked ? 'block' : 'none';
        };
        chkCredito.addEventListener('change', toggle);
        toggle();
    },

    initGarante() {
        const btn = document.querySelector('#cambiarGarante');
        btn?.addEventListener('click', () => {
            const id = document.querySelector('#ClienteID')?.value;
            if (id) window.location.href = `/Clientes/AsignarGarante?clienteID=${id}`;
        });
    },

    bindEnterSubmit() {
        bindEnter('#ClienteName', () => this.submitForm());
        bindEnter('#DniCliente', () => this.submitForm());
    },

    async submitForm() {
        const form = document.querySelector('#clienteForm');
        if (!form) return;
        const data = {};
        new FormData(form).forEach((v, k) => data[k] = v);
        try {
            await ajaxPost(form.action, data);
            showModal('#successModal', 'Cliente guardado correctamente');
        } catch (e) {
            showModal('#errorModal', 'Error al guardar cliente');
        }
    }
};

// Auto-inicialización
document.addEventListener('DOMContentLoaded', () => clienteForm.init());

export default clienteForm;
