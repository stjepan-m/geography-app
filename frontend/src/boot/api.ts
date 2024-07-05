import axios from 'axios';
import AppSettings from 'src/AppSettings';

const backendBaseURL = AppSettings.ApiUrl;
const backendAPI = axios.create({
  baseURL: backendBaseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const openRouteServiceBaseURL = 'https://api.openrouteservice.org';
const openRouteServiceToken = AppSettings.OpenRouteKey;
const openRouteServiceAPI = axios.create({
  baseURL: openRouteServiceBaseURL,
  headers: {
    'Content-Type': 'application/json',
  },
  params: {
    api_key: openRouteServiceToken,
  },
});

export { axios, backendAPI, backendBaseURL, openRouteServiceAPI };
