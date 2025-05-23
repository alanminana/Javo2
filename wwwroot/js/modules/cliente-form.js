// modules/cliente-form.js
import { loadDropdown } from '../utils/dropdown.js';
import { ajaxPost } from '../utils/app.js';
import { showModal, bindEnter } from '../utils/bind-helpers.js';

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

        const provEl = document.querySelector(provSelector);
        if (provEl) {
            provEl.addEventListener('change', () => {
                const provinciaID = provEl.value;
                loadDropdown('/Clientes/GetCiudades', { provinciaID }, citySelector,
                    { placeholder: 'Seleccione ciudad...' });
            });

            const initialProv = provEl.value;
            if (initialProv) {
                loadDropdown('/Clientes/GetCiudades', { provinciaID: initialProv }, citySelector,
                    { placeholder: 'Seleccione ciudad...' });
            }
        }
    },

    initCreditoFields() {
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
        if (btn) {
            btn.addEventListener('click', () => {
                const idEl = document.querySelector('#ClienteID');
                const id = idEl?.value;
                if (id) window.location.href = `/Clientes/AsignarGarante?clienteID=${id}`;
            });
        }
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

document.addEventListener('DOMContentLoaded', () => clienteForm.init());
export default clienteForm;