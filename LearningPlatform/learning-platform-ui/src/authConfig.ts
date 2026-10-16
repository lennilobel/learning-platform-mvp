import type { Configuration } from '@azure/msal-browser'

const tenantId = '6128879d-86d5-4470-aa2b-77e84c83aa56'
const clientId = 'af960a29-8b27-4e3a-85cd-c4d2d5a1ed27'                                             // learning-platform-ui client ID
export const enrollmentWriteScope = 'api://964b4c3a-58d4-4511-a180-06b97dba72eb/Enrollment.Write'   // learning-platform-api client ID

export const msalConfig: Configuration = {
    auth: {
        clientId,
        authority: `https://login.microsoftonline.com/${tenantId}`,
        redirectUri: window.location.origin,
    },
    cache: {
        cacheLocation: 'sessionStorage',
    },
}

export const loginRequest = {
    scopes: [enrollmentWriteScope],
}
