import {fileURLToPath, URL} from 'node:url';
import {defineConfig} from 'vite';
import plugin from '@vitejs/plugin-react';
import react from '@vitejs/plugin-react'
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';

const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
        ? `${process.env.APPDATA}/ASP.NET/https`
        : `${process.env.HOME}/.aspnet/https`;

//const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
//const certificateName = certificateArg ? certificateArg.groups?.value : "rsse.client";
const certificateArg = process.argv.map(arg => arg.match(/--name=(.+)/i)).find(match => match !== null);
const certificateName = certificateArg ? certificateArg[1] : "rsse.client";

if (!certificateName) {
    console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
    process.exit(-1);
}

const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

// создать сертификат только при разработке:
if (process.env.NODE_ENV === "development" && (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath))) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

// https://vitejs.dev/config/
// https://vitejs.dev/config/#conditional-config
export default defineConfig(({ command, mode }) => {
    if (command === 'serve') {
        // конфигурация для запуска тестов:
        if (mode === 'test') return {
            // vitest testing:
            plugins: [react()],
            test: {
                global: true,
                environment: 'jsdom',
                setupFiles: ['setupTest.ts']
            },
        };
        // конфигурация для разработки:
        return {
            plugins: [plugin()],
            resolve: {
                alias: {
                    '@': fileURLToPath(new URL('./src/index.tsx', import.meta.url))
                }
            },
            server: {
                proxy: {
                    '/system': {
                        target: 'http://localhost:5000',
                        secure: false
                    }
                },
                host: 'localhost',
                port: 5173,
                https: {
                    key: fs.readFileSync(keyFilePath),
                    cert: fs.readFileSync(certFilePath),
                }
            },
            // root: 'src',
            build: {
                outDir: 'build',
                // sourcemap: true
            }
        }
    }
    else {
        // конфигурация для билда:
        return {
            build: {
                outDir: 'build',
            }
        }
    }
})
