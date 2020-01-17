
import { Session } from './session';
export function authHeader() {
    // return authorization header with jwt token
    let authToken = JSON.parse(Session.getItem('moneto-token'));

    if (authToken && authToken.token) {
        return { 'Authorization': 'Bearer ' + authToken.token };
    } else {
        return {};
    }
}