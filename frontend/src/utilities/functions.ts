import * as APP_CONSTANTS from 'src/utilities/constants';
import { QVueGlobals } from 'quasar';
import { Router } from 'vue-router';
import { AxiosError } from 'axios';
import { backendAPI, openRouteServiceAPI } from 'src/boot/api';
import { MessageType } from 'src/models/models';
import { useAuthStore } from 'src/stores/authStore';

/**
 * Executes a GET request to the backend API
 * @param request - Request URL, relative to the Base URL
 * @param errorMessage - Error message to be sent if the request fails
 * @param q - QVueGlobals instance for notification messages
 * @returns - Data retrieved from the API
 */
export async function retrieveData(request: string, errorMessage: string, q: QVueGlobals, router: Router) {
  try {
    const response = await backendAPI.get(request, { headers: { Authorization: useAuthStore().sessionToken } });
    return response.data;
  } catch (error) {
    console.log('Error: ', error);
    if ((error as AxiosError).response?.status === 401) {
      useAuthStore().handleLogout();
      router.push({ name: 'Login', query: { redirect: router.currentRoute.value.fullPath } });
    } else {
      showNotification(errorMessage, MessageType.Error, q);
    }
    return undefined;
  }
}

/**
 * Executes a POST request to the backend API
 * @param request - Request URL, relative to the Base URL
 * @param data - Data to be sent in the body of the request
 * @param errorMessage - Error message to be sent if the request fails
 * @param q - QVueGlobals instance for notification messages
 * @returns - Data from the response
 */
export async function insertData<T>(request: string, data: T, errorMessage: string, q: QVueGlobals, router: Router | undefined) {
  try {
    const response = await backendAPI.post(request, data, { headers: { Authorization: useAuthStore().sessionToken } });
    return response.data;
  } catch (error) {
    console.log('Error: ', error);
    if ((error as AxiosError).response?.status === 401) {
      if (router) {
        useAuthStore().handleLogout();
        router.push({ name: 'Login', query: { redirect: router.currentRoute.value.fullPath } });
      }
    } else {
      showNotification(errorMessage, MessageType.Error, q);
    }
    return undefined;
  }
}

/**
 * Executes a PUT request to the backend API
 * @param request - Request URL, relative to the Base URL
 * @param data - Data to be sent in the body of the request
 * @param errorMessage - Error message to be sent if the request fails
 * @param q - QVueGlobals instance for notification messages
 * @returns - Data from the response
 */
export async function updateData<T>(request: string, data: T, errorMessage: string, q: QVueGlobals, router: Router) {
  try {
    const response = await backendAPI.put(request, data, { headers: { Authorization: useAuthStore().sessionToken } });
    return response.data;
  } catch (error) {
    console.log('Error: ', error);
    if ((error as AxiosError).response?.status === 401) {
      useAuthStore().handleLogout();
      router.push({ name: 'Login', query: { redirect: router.currentRoute.value.fullPath } });
    } else {
      showNotification(errorMessage, MessageType.Error, q);
    }
    return undefined;
  }
}

/**
 * Executes a request to the OpenRouteService API and reverse geocodes the location based on the coordinates
 * @param coordinates - Coordinates of the location
 * @param locationType - Type of the location we want to retrieve
 * @param errorMessage - Error message to be sent if the request fails
 * @param q - QVueGlobals instance for notification messages
 * @returns - List of locations on the specified location
 */
export async function reverseGeocode(coordinates: Array<number>, locationType: string, errorMessage: string, q: QVueGlobals) {
  try {
    let layer;
    switch (locationType) {
      case APP_CONSTANTS.LOCATION_CITY:
        layer = APP_CONSTANTS.LAYER_LOCALITY;
        break;
      default:
        layer = '';
        break;
    }

    const response = await openRouteServiceAPI.get(`geocode/reverse?point.lon=${coordinates[0]}&point.lat=${coordinates[1]}&${layer ? 'layer=' + layer : ''}`);
    return response.data;
  } catch (error) {
    console.log('Error: ', error);
    showNotification(errorMessage, MessageType.Error, q);
    return undefined;
  }
}

/**
 * Shows the notification message
 * @param message - Message to be shown()
 * @param type - Message type (Success | Error)
 * @param q - QVueGlobals instance for notification messages
 * @param t - i18n instance for language identification (used in the message)
 */
export function showNotification(message: string, type: MessageType, q: QVueGlobals) {
  q.notify({
    message,
    color: type === MessageType.Success ? APP_CONSTANTS.COLOR_POSITIVE : APP_CONSTANTS.COLOR_NEGATIVE,
    actions: [{ icon: 'close', color: APP_CONSTANTS.COLOR_WHITE }],
  });
}

/**
 * Exports an object to a json file
 * @param jsonObject - Object to be exported
 * @param fileName - Name of the file downloaded
 */
export function exportJSON(jsonObject: object, fileName: string) {
  const jsonString = JSON.stringify(jsonObject, null, 2); // Pretty print with 2 spaces
  const blob = new Blob([jsonString], { type: APP_CONSTANTS.FILE_JSON });
  const url = URL.createObjectURL(blob);

  // Create an anchor element
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();

  // Remove the anchor from the document
  document.body.removeChild(a);

  // Revoke the Blob URL
  URL.revokeObjectURL(url);
}
