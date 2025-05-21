import { createRouter, createWebHistory } from 'vue-router';
import { usePermissionStore } from '@/stores/permissionStore';

// Importar vistas
import VentaForm from '@/views/ventas/VentaForm.vue';
import VentasList from '@/views/ventas/VentasList.vue';
import ProductoList from '@/views/productos/ProductoList.vue';
import ProductoForm from '@/views/productos/ProductoForm.vue';
import ClienteList from '@/views/clientes/ClienteList.vue';
import ReportesView from '@/views/reportes/ReportesView.vue';
import NotFound from '@/views/NotFound.vue';

const routes = [
    {
        path: '/',
        redirect: '/Home'
    },
    {
        path: '/Home',
        name: 'Home',
        component: () => import('@/views/Home.vue')
    },
    // Rutas de Ventas
    {
        path: '/Ventas',
        name: 'Ventas',
        component: VentasList,
        meta: { requiresPermission: 'ventas.ver' }
    },
    {
        path: '/Ventas/Create',
        name: 'CrearVenta',
        component: VentaForm,
        meta: { requiresPermission: 'ventas.crear' }
    },
    {
        path: '/Ventas/Edit/:id',
        name: 'EditarVenta',
        component: VentaForm,
        meta: { requiresPermission: 'ventas.editar' }
    },
    // Rutas de Productos
    {
        path: '/Productos',
        name: 'Productos',
        component: ProductoList,
        meta: { requiresPermission: 'productos.ver' }
    },
    {
        path: '/Productos/Create',
        name: 'CrearProducto',
        component: ProductoForm,
        meta: { requiresPermission: 'productos.crear' }
    },
    {
        path: '/Productos/Edit/:id',
        name: 'EditarProducto',
        component: ProductoForm,
        meta: { requiresPermission: 'productos.editar' }
    },
    // Rutas de Clientes
    {
        path: '/Clientes',
        name: 'Clientes',
        component: ClienteList,
        meta: { requiresPermission: 'clientes.ver' }
    },
    // Rutas de Reportes
    {
        path: '/Reportes',
        name: 'Reportes',
        component: ReportesView,
        meta: { requiresPermission: 'reportes.ver' }
    },
    // Ruta de error 404
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: NotFound
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

// Guardia de navegación para verificar permisos
router.beforeEach((to, from, next) => {
    const requiresPermission = to.meta.requiresPermission;

    if (requiresPermission) {
        const permissionStore = usePermissionStore();

        if (!permissionStore.permissionsLoaded) {
            permissionStore.loadPermissions();
        }

        if (permissionStore.hasPermission(requiresPermission)) {
            next();
        } else {
            // Redirigir a página de acceso denegado
            next({ name: 'AccessDenied' });
        }
    } else {
        next();
    }
});

export default router;