import 'toastr/build/toastr.min.css';
import toastr from 'toastr';

// Configuración base
toastr.options = {
    position: 'top-right',
    timeOut: 5000,
    closeButton: true,
    progressBar: true,
    preventDuplicates: true
};

export default {
    success(message, title) {
        toastr.success(message, title);
    },
    error(message, title) {
        toastr.error(message, title);
    },
    warning(message, title) {
        toastr.warning(message, title);
    },
    info(message, title) {
        toastr.info(message, title);
    }
};