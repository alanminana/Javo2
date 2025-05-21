import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';

export default defineConfig({
    plugins: [vue()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, 'wwwroot/vue')
        }
    },
    build: {
        outDir: 'wwwroot/vue/dist',
        assetsDir: 'assets',
        // Usar relative para que funcione en subdirectorios
        rollupOptions: {
            output: {
                entryFileNames: 'assets/[name].js',
                chunkFileNames: 'assets/[name].js',
                assetFileNames: 'assets/[name].[ext]'
            }
        }
    },
    server: {
        port: 5173, // Puerto para desarrollo
        strictPort: true,
        proxy: {
            // Proxy para API requests
            '/api': {
                target: 'https://localhost:5001',
                secure: false,
                changeOrigin: true
            }
        }
    }
});