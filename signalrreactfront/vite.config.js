import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 65513,
        proxy: {
            '/api': {
                target: 'http://localhost:5253',
                secure: false,
                changeOrigin: true
            },
            '/uploads': {
                target: 'http://localhost:5253',
                secure: false,
                changeOrigin: true
            },
            '/chatHub': {
                target: 'http://localhost:5253',
                secure: false,
                ws: true
            }
        }
    }
})