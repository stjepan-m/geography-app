import { boot } from 'quasar/wrappers';
import AppSettings from 'src/AppSettings';
import vue3GoogleLogin from 'vue3-google-login';

export default boot(({ app }) => {
  app.use(vue3GoogleLogin, {
    clientId: AppSettings.GoogleClientId,
  });
});
