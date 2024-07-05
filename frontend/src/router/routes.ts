import { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', name: 'Login', component: () => import('pages/LoginPage.vue') }],
  },
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/IndexPage.vue') }],
  },
  {
    path: '/create-game',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/CreateGame.vue') }],
    meta: { requiresAuth: true, requiresTeacherOrAdmin: true },
  },
  {
    path: '/play',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/PlayGame.vue') }],
  },
  {
    path: '/games',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/GamesManagement.vue') }],
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  {
    path: '/my-games',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/MyGamesManagement.vue') }],
    meta: { requiresAuth: true, requiresTeacherOrAdmin: true },
  },
  {
    path: '/game',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/GameInfo.vue') }],
    meta: { requiresAuth: true, requiresTeacherOrAdmin: true },
  },
  {
    path: '/locations',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', component: () => import('pages/LocationsManagement.vue') }],
    meta: { requiresAuth: true, requiresTeacherOrAdmin: true },
  },
  {
    path: '/about',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', name: 'About Page', component: () => import('pages/AboutPage.vue') }],
  },
  {
    path: '/access-denied',
    component: () => import('layouts/MainLayout.vue'),
    children: [{ path: '', name: 'Access Denied', component: () => import('pages/AccessDenied.vue') }],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
