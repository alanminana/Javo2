<template>
    <div class="bg-dark text-white d-flex flex-column min-vh-100">
        <!-- Header con menú móvil -->
        <nav class="navbar navbar-dark bg-dark d-md-none">
            <div class="container-fluid">
                <button class="btn btn-outline-light" type="button" data-bs-toggle="offcanvas" data-bs-target="#mobileMenu" aria-label="Abrir Menú">
                    <i class="bi bi-list"></i>
                </button>
                <router-link class="navbar-brand ms-2" to="/">The BuryCode.</router-link>

                <!-- User Info en mobile -->
                <div v-if="isAuthenticated" class="dropdown">
                    <button class="btn btn-link dropdown-toggle text-light text-decoration-none" type="button" id="userDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="bi bi-person-circle"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end dropdown-menu-dark" aria-labelledby="userDropdown">
                        <li><span class="dropdown-item-text">{{ userName }}</span></li>
                        <li><hr class="dropdown-divider"></li>
                        <li><button class="dropdown-item" @click="logout">Cerrar Sesión</button></li>
                    </ul>
                </div>
            </div>
        </nav>

        <!-- Menú móvil -->
        <div class="offcanvas offcanvas-start bg-dark text-white" tabindex="-1" id="mobileMenu" aria-labelledby="mobileMenuLabel">
            <div class="offcanvas-header">
                <h5 id="mobileMenuLabel" class="offcanvas-title">Menú</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="offcanvas" aria-label="Cerrar"></button>
            </div>
            <div class="offcanvas-body">
                <ul class="nav nav-pills flex-column mb-4">
                    <li class="nav-item" v-for="item in menuItems" :key="item.path">
                        <router-link :to="item.path"
                                     class="nav-link text-white"
                                     :class="{ 'active': isActive(item.path) }"
                                     v-if="!item.permission || hasPermission(item.permission)"
                                     @click="closeMobileMenu">
                            <i :class="`bi bi-${item.icon} me-2`"></i>{{ item.text }}
                        </router-link>
                    </li>
                </ul>
                <hr />
                <div v-if="isAuthenticated">
                    <form @submit.prevent="logout" class="d-flex">
                        <button type="submit" class="btn btn-outline-light w-100 text-start">
                            <i class="bi bi-box-arrow-right me-2"></i><strong>Cerrar Sesión</strong>
                        </button>
                    </form>
                </div>
                <div v-else>
                    <router-link class="btn btn-primary w-100" to="/Auth/Login">
                        <i class="bi bi-box-arrow-in-right me-2"></i><strong>Iniciar Sesión</strong>
                    </router-link>
                </div>
            </div>
        </div>

        <!-- Contenedor principal -->
        <div class="container-fluid flex-grow-1">
            <div class="row">
                <!-- Sidebar Desktop -->
                <aside class="col-12 col-md-3 col-xl-2 d-none d-md-block sidebar bg-dark text-white pt-3">
                    <router-link class="d-flex align-items-center mb-3 text-white text-decoration-none fs-5" to="/">
                        The Bury<span class="text-primary">Code.</span>
                    </router-link>
                    <hr class="border-secondary" />
                    <ul class="nav nav-pills flex-column mb-auto">
                        <li class="nav-item" v-for="item in menuItems" :key="item.path">
                            <router-link :to="item.path"
                                         class="nav-link text-white"
                                         :class="{ 'active': isActive(item.path) }"
                                         v-if="!item.permission || hasPermission(item.permission)">
                                <i :class="`bi bi-${item.icon} me-2`"></i>{{ item.text }}
                            </router-link>
                        </li>
                    </ul>
                    <hr class="border-secondary" />

                    <div v-if="isAuthenticated" class="d-flex align-items-center justify-content-between mb-3">
                        <div class="d-flex align-items-center">
                            <div class="rounded-circle bg-secondary text-light d-flex align-items-center justify-content-center me-2" style="width: 32px; height: 32px;">
                                <i class="bi bi-person-fill"></i>
                            </div>
                            <span class="text-light">{{ userName }}</span>
                        </div>
                        <button @click="logout" class="btn btn-link text-light text-decoration-none">
                            <i class="bi bi-box-arrow-right me-1"></i>Cerrar Sesión
                        </button>
                    </div>
                    <div v-else>
                        <router-link class="btn btn-primary w-100" to="/Auth/Login">
                            <i class="bi bi-box-arrow-in-right me-2"></i>Iniciar Sesión
                        </router-link>
                    </div>
                </aside>

                <!-- Main Content -->
                <main class="col-12 col-md-9 col-xl-10 py-3">
                    <router-view v-slot="{ Component }">
                        <transition name="fade" mode="out-in">
                            <component :is="Component" />
                        </transition>
                    </router-view>
                </main>
            </div>
        </div>

        <!-- Footer -->
        <footer class="mt-auto py-3 bg-dark text-muted">
            <div class="container-fluid">
                &copy; {{ new Date().getFullYear() }} - Javo2 - <router-link to="/Home/Privacy" class="text-white text-decoration-none">Privacidad</router-link>
            </div>
        </footer>

        <!-- Toasts container -->
        <div class="toast-container position-fixed top-0 end-0 p-3"></div>
    </div>
</template>

<script>
    import { ref, computed } from 'vue';
    import { useRouter, useRoute } from 'vue-router';
    import { usePermissionStore } from '@/stores/permissionStore';
    import apiService from '@/services/apiService';

    export default {
        name: 'App',
        setup() {
            const router = useRouter();
            const route = useRoute();
            const permissionStore = usePermissionStore();

            // Estado de autenticación
            const isAuthenticated = ref(true); // En producción, obtener de API o cookie
            const userName = ref('Usuario'); // En producción, obtener de API o cookie

            // Elementos del menú
            const menuItems = [
                { path: '/', icon: 'house-fill', text: 'Inicio', permission: null },
                { path: '/Clientes', icon: 'people-fill', text: 'Clientes', permission: 'clientes.ver' },
                { path: '/Ventas', icon: 'cart-fill', text: 'Ventas', permission: 'ventas.ver' },
                { path: '/Proveedores', icon: 'truck-flatbed', text: 'Proveedores', permission: 'productos.ver' },
                { path: '/Promociones', icon: 'tag-fill', text: 'Promociones', permission: 'productos.ver' },
                { path: '/Reportes', icon: 'bar-chart-fill', text: 'Reportes', permission: 'reportes.ver' },
                { path: '/Configuracion', icon: 'gear-fill', text: 'Configuración', permission: 'configuracion.ver' },
                { path: '/CatalogoProductos', icon: 'box-seam-fill', text: 'Catálogo de Productos', permission: 'productos.ver' },
                { path: '/Credito/Configuracion', icon: 'calculator', text: 'Créditos', permission: 'configuracion.editar' }
            ];

            // Verificar si una ruta está activa
            function isActive(path) {
                return route.path === path || route.path.startsWith(path + '/');
            }

            // Verificar permisos
            function hasPermission(permission) {
                return permissionStore.hasPermission(permission);
            }

            // Cerrar sesión
            async function logout() {
                try {
                    await apiService.post('/Auth/Logout', {});
                    window.location.href = '/Auth/Login'; // Redireccionar a login
                } catch (error) {
                    console.error('Error al cerrar sesión:', error);
                }
            }

            // Cerrar menú móvil
            function closeMobileMenu() {
                const offcanvas = document.getElementById('mobileMenu');
                if (offcanvas) {
                    const bsOffcanvas = bootstrap.Offcanvas.getInstance(offcanvas);
                    if (bsOffcanvas) {
                        bsOffcanvas.hide();
                    }
                }
            }

            return {
                isAuthenticated,
                userName,
                menuItems,
                isActive,
                hasPermission,
                logout,
                closeMobileMenu
            };
        }
    };
</script>

<style>
    /* Animaciones para las transiciones entre rutas */
    .fade-enter-active,
    .fade-leave-active {
        transition: opacity 0.2s ease;
    }

    .fade-enter-from,
    .fade-leave-to {
        opacity: 0;
    }
</style>