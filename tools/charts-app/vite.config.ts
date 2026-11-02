// Builds the charts page to ONE self-contained file, ../../docs/charts.html: JS, CSS and the benchmark
// data are inlined, so it needs no other files, works when opened from disk, and a rebuild changes
// exactly one file in git.
//
// The code is written against the React API (TanStack Router and Table are React libraries), and the
// bundle is Preact: `react` is aliased to `preact/compat`. The real `react` packages are devDependencies
// only because TanStack declares them as peers and TypeScript needs their types; they are never bundled.
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import { viteSingleFile } from 'vite-plugin-singlefile'

export default defineConfig({
  plugins: [tailwindcss(), viteSingleFile()],
  oxc: { jsx: { runtime: 'automatic', importSource: 'preact' } },
  resolve: {
    alias: {
      react: 'preact/compat',
      'react-dom/test-utils': 'preact/test-utils',
      'react-dom': 'preact/compat',
      'react/jsx-runtime': 'preact/jsx-runtime',
    },
  },
  server: { open: '/charts.html' },
  build: {
    outDir: '../../docs',
    // docs/ holds the rest of the site, so it must never be emptied. Only charts.html is written.
    emptyOutDir: false,
    rollupOptions: {
      input: 'charts.html',
      // TanStack ships "use client" directives for React Server Components, which are meaningless in a
      // static page. Drop that one warning so real warnings stay visible in CI logs.
      onwarn(warning, defaultHandler) {
        if (warning.code === 'MODULE_LEVEL_DIRECTIVE') return
        defaultHandler(warning)
      },
    },
  },
})
