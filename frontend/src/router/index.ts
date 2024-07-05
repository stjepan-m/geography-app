import { route } from 'quasar/wrappers';
import { createMemoryHistory, createRouter, createWebHashHistory, createWebHistory } from 'vue-router';

import routes from './routes';
import { useAuthStore } from 'stores/authStore';

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */

export default route(function (/* { store, ssrContext } */) {
  const createHistory = process.env.SERVER ? createMemoryHistory : process.env.VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory;

  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,

    // Leave this as is and make changes in quasar.conf.js instead!
    // quasar.conf.js -> build -> vueRouterMode
    // quasar.conf.js -> build -> publicPath
    history: createHistory(process.env.VUE_ROUTER_BASE),
  });

  const authStore = useAuthStore();

  Router.beforeEach((to, from, next) => {
    if (to.matched.some((record) => record.meta.requiresAuth) && !authStore.isAuthenticated) {
      // If not authenticated, redirect to login page
      next({ name: 'Login', query: { redirect: to.fullPath } });
    } else if (to.matched.some((record) => record.meta.requiresTeacherOrAdmin) && !authStore.isAdmin && !authStore.isTeacher) {
      //If not authorized, redirect to access denied page
      next({ name: 'Access Denied' });
    } else if (to.matched.some((record) => record.meta.requiresAdmin) && !authStore.isAdmin) {
      //If not authorized, redirect to access denied page
      next({ name: 'Access Denied' });
    } else {
      next(); // Continue to the route
    }
  });

  return Router;
});
