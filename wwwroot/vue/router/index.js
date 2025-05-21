// Importar vistas principales
import Home from '@/views/Home.vue';
import NotFound from '@/views/NotFound.vue';

// Importar vistas de módulos
import ClientesIndex from '@/views/Clientes/Index.vue';
import ClientesCreate from '@/views/Clientes/Create.vue';
import ClientesEdit from '@/views/Clientes/Edit.vue';
import ClientesDetails from '@/views/Clientes/Details.vue';

import VentasIndex from '@/views/Ventas/Index.vue';
import VentasCreate from '@/views/Ventas/Create.vue';
import VentasEdit from '@/views/Ventas/Edit.vue';
import VentasDetails from '@/views/Ventas/Details.vue';
import VentasAutorizaciones from '@/views/Ventas/Autorizaciones.vue';
import VentasEntregaProductos from '@/views/Ventas/EntregaProductos.vue';

import CotizacionesIndex from '@/views/Cotizaciones/Index.vue';
import CotizacionesCreate from '@/views/Cotizaciones/Create.vue';
import CotizacionesDetails from '@/views/Cotizaciones/Details.vue';

import ProductosIndex from '@/views/Productos/Index.vue';
import ProductosCreate from '@/views/Productos/Create.vue';
import ProductosEdit from '@/views/Productos/Edit.vue';
import ProductosDetails from '@/views/Productos/Details.vue';

import ProveedoresIndex from '@/views/Proveedores/Index.vue';
import ProveedoresCompras from '@/views/Proveedores/Compras.vue';
import ProveedoresCreate from '@/views/Proveedores/Create.vue';
import ProveedoresEdit from '@/views/Proveedores/Edit.vue';
import ProveedoresDetails from '@/views/Proveedores/Details.vue';

import CatalogoProductosIndex from '@/views/CatalogoProductos/Index.vue';

import PromocionesIndex from '@/views/Promociones/Index.vue';
import PromocionesCreate from '@/views/Promociones/Create.vue';
import PromocionesEdit from '@/views/Promociones/Edit.vue';

import ReportesIndex from '@/views/Reportes/Index.vue';
import ReportesVentas from '@/views/Reportes/Ventas.vue';
import ReportesStock from '@/views/Reportes/Stock.vue';

import ConfiguracionIndex from '@/views/Configuracion/Index.vue';
import CreditoConfiguracion from '@/views/Credito/Configuracion.vue';
import CreditoCriterios from '@/views/Credito/Criterios.vue';

// Rutas de la aplicación
const routes = [
    // Ruta principal
    {
        path: '/',
        component: Home,
        name: 'home'
    },

    // Rutas de Clientes
    {
        path: '/clientes',
        component: ClientesIndex,
        name: 'clientes.index'
    },
    {
        path: '/clientes/create',
        component: ClientesCreate,
        name: 'clientes.create'
    },
    {
        path: '/clientes/:id/edit',
        component: ClientesEdit,
        name: 'clientes.edit'
    },
    {
        path: '/clientes/:id',
        component: ClientesDetails,
        name: 'clientes.details'
    },

    // Rutas de Ventas
    {
        path: '/ventas',
        component: VentasIndex,
        name: 'ventas.index'
    },
    {
        path: '/ventas/create',
        component: VentasCreate,
        name: 'ventas.create'
    },
    {
        path: '/ventas/:id/edit',
        component: VentasEdit,
        name: 'ventas.edit'
    },
    {
        path: '/ventas/:id',
        component: VentasDetails,
        name: 'ventas.details'
    },
    {
        path: '/ventas/autorizaciones',
        component: VentasAutorizaciones,
        name: 'ventas.autorizaciones'
    },
    {
        path: '/ventas/entrega-productos',
        component: VentasEntregaProductos,
        name: 'ventas.entrega-productos'
    },

    // Rutas de Cotizaciones
    {
        path: '/cotizaciones',
        component: CotizacionesIndex,
        name: 'cotizaciones.index'
    },
    {
        path: '/cotizaciones/create',
        component: CotizacionesCreate,
        name: 'cotizaciones.create'
    },
    {
        path: '/cotizaciones/:id',
        component: CotizacionesDetails,
        name: 'cotizaciones.details'
    },

    // Rutas de Productos
    {
        path: '/productos',
        component: ProductosIndex,
        name: 'productos.index'
    },
    {
        path: '/productos/create',
        component: ProductosCreate,
        name: 'productos.create'
    },
    {
        path: '/productos/:id/edit',
        component: ProductosEdit,
        name: 'productos.edit'
    },
    {
        path: '/productos/:id',
        component: ProductosDetails,
        name: 'productos.details'
    },

    // Rutas de Proveedores
    {
        path: '/proveedores',
        component: ProveedoresIndex,
        name: 'proveedores.index'
    },
    {
        path: '/proveedores/compras',
        component: ProveedoresCompras,
        name: 'proveedores.compras'
    },
    {
        path: '/proveedores/create',
        component: ProveedoresCreate,
        name: 'proveedores.create'
    },
    {
        path: '/proveedores/:id/edit',
        component: ProveedoresEdit,
        name: 'proveedores.edit'
    },
    {
        path: '/proveedores/:id',
        component: ProveedoresDetails,
        name: 'proveedores.details'
    },

    // Rutas de CatalogoProductos
    {
        path: '/catalogo-productos',
        component: CatalogoProductosIndex,
        name: 'catalogo-productos.index'
    },

    // Rutas de Promociones
    {
        path: '/promociones',
        component: PromocionesIndex,
        name: 'promociones.index'
    },
    {
        path: '/promociones/create',
        component: PromocionesCreate,
        name: 'promociones.create'
    },
    {
        path: '/promociones/:id/edit',
        component: PromocionesEdit,
        name: 'promociones.edit'
    },

    // Rutas de Reportes
    {
        path: '/reportes',
        component: ReportesIndex,
        name: 'reportes.index'
    },
    {
        path: '/reportes/ventas',
        component: ReportesVentas,
        name: 'reportes.ventas'
    },
    {
        path: '/reportes/stock',
        component: ReportesStock,
        name: 'reportes.stock'
    },

    // Rutas de Configuración
    {
        path: '/configuracion',
        component: ConfiguracionIndex,
        name: 'configuracion.index'
    },

    // Rutas de Crédito
    {
        path: '/credito/configuracion',
        component: CreditoConfiguracion,
        name: 'credito.configuracion'
    },
    {
        path: '/credito/criterios',
        component: CreditoCriterios,
        name: 'credito.criterios'
    },

    // Ruta para páginas no encontradas - debe ser la última
    {
        path: '/:pathMatch(.*)*',
        component: NotFound,
        name: 'not-found'
    }
];

export default routes;