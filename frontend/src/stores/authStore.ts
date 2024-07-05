import { QVueGlobals } from 'quasar';
import { defineStore } from 'pinia';
import { insertData } from 'src/utilities/functions';

const getDefaultState = () => ({
  isAuthenticated: localStorage.sessionToken !== undefined,
  sessionToken: localStorage.sessionToken,
  name: localStorage.user ? JSON.parse(localStorage.user)?.name : undefined,
  email: localStorage.user ? JSON.parse(localStorage.user)?.email : undefined,
  isAdmin: localStorage.sessionToken && localStorage.user ? JSON.parse(localStorage.user)?.userType === 'Administrator' : undefined,
  isTeacher: localStorage.sessionToken && localStorage.user ? JSON.parse(localStorage.user)?.userType === 'Teacher' : undefined,
  isPlayer: localStorage.sessionToken && localStorage.user ? JSON.parse(localStorage.user)?.userType === 'Player' : undefined,
});

export const useAuthStore = defineStore('auth', {
  state: getDefaultState,
  actions: {
    async handleLogin(token: string, errorMessage: string, q: QVueGlobals) {
      const response = await insertData('auth/google/login', { token }, errorMessage, q, undefined);
      if (response.sessionToken) {
        // Store token in local storage
        localStorage.setItem('sessionToken', response.sessionToken);
        localStorage.setItem('user', JSON.stringify(response.user));

        this.isAuthenticated = true;
        this.sessionToken = response.sessionToken;
        (this.name = response.user.name), (this.email = response.user.email), (this.isAdmin = response.user?.userType === 'Administrator');
        this.isTeacher = response.user?.userType === 'Teacher';
        this.isPlayer = response.user?.userType === 'Player';
      }
    },
    handleLogout() {
      // Clear token from local storage
      localStorage.removeItem('sessionToken');
      localStorage.removeItem('user');
      Object.assign(this.$state, getDefaultState());
    },
    // Other actions
  },
});
