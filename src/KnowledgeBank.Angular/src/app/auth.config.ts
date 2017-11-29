import { environment } from '../environments/environment';

export function getAuthSettings(base: string) {
    const devOrigin = 'https://localhost:44321/';

    const appUrl = window.location.origin + base;
    const identityServerAuthority = (environment.production ? appUrl : devOrigin) + 'identity';

    return {
        authority: identityServerAuthority,
        client_id: 'js',
        redirect_uri: appUrl + 'auth-callback.html',
        post_logout_redirect_uri: appUrl,
        response_type: 'id_token token',
        scope: 'openid profile webapi',

        silent_redirect_uri: appUrl + 'silent-renew.html',
        automaticSilentRenew: true,

        filterProtocolClaims: true,
        loadUserInfo: true
    };
};
