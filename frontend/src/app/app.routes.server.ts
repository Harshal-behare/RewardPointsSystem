import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Login page can be prerendered (no auth required)
  {
    path: 'login',
    renderMode: RenderMode.Prerender
  },
  // Protected routes should use client-side rendering only
  // This prevents redirect issues on page refresh
  {
    path: 'admin/**',
    renderMode: RenderMode.Client
  },
  {
    path: 'employee/**',
    renderMode: RenderMode.Client
  },
  {
    path: 'dashboard',
    renderMode: RenderMode.Client
  },
  // Default for any other routes
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
