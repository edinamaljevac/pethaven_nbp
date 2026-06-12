/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#f2fbf7',
          100: '#dff6eb',
          500: '#2f9d72',
          600: '#257f5c',
          700: '#20674d'
        }
      }
    }
  },
  plugins: []
}