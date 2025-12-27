/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}", // Esto cubre src/pages/InuzaruChat.jsx
  ],
  theme: {
    extend: {
      colors: {
        // Colores que combinan con el estilo oscuro y neón de Inuzaru
        brand: {
          dark: '#0f172a',
          card: '#1e293b',
          neon: '#3b82f6',
        }
      }
    },
  },
  plugins: [],
}