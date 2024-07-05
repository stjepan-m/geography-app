const AppSettings = {
    'BasePath' : process.env.APP_BASE_PATH ?? '',
    'OpenRouteKey' : process.env.OPEN_ROUTE_KEY,
    'ApiUrl' : process.env.API_URL,
    'GoogleClientId': process.env.GOOGLE_CLIENT_ID,
    'LoginEnabled': process.env.LOGIN_ENABLED === 'true'
}

export default AppSettings;
