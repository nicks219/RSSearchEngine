import js from '@eslint/js'
import globals from 'globals'
import tseslint from '@typescript-eslint/eslint-plugin'
import tsparser from '@typescript-eslint/parser'
import react from 'eslint-plugin-react'

export default [
    { ignores: [
            'dist',
            '**/build/**',
            '**/node_modules/**'
        ] },
    js.configs.recommended,
    {
        files: ['**/*.{ts,tsx}'],
        plugins: {
            '@typescript-eslint': tseslint,
        },

        languageOptions: {
            parser: tsparser,
            globals: {
                ...globals.browser,
            },
        },

        rules: {
            '@typescript-eslint/adjacent-overload-signatures': 'error',
            '@typescript-eslint/no-explicit-any': 'warn',
        },
    },
    {
        ...react.configs.flat.recommended,
        rules: {
            ...react.configs.flat.recommended.rules,
            'react/react-in-jsx-scope': 'off',
        },
        settings: {
            react: {
                version: 'detect'
            }
        },
    },
]
