const config = require('../config');

export const userBALService = {
    login,
    logout,
    getAll,
    getById,
    update,
    getAuthHeader,
    getUserInfo,
    delete: _delete
};

function login(user_name, password) {
    const requestOptions = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ user_name, password })
    };

    return fetch(`${config.apiUrl}/login`, requestOptions)
        .then(handleResponse)
        .then(user => {
            // store user details and jwt token in local storage to keep user logged in between page refreshes
            localStorage.setItem(config.token, JSON.stringify(user));
            return user;
        });
}

function logout() {
    // remove user from local storage to log user out
    localStorage.removeItem(config.token);
}

function getAll() {

}

function getById(id) {

}

function update(user) {

}

function getUserInfo(){
    return JSON.parse(localStorage.getItem(config.token));
}

function getAuthHeader() {
    return {Authorization: 'Bearer ' + this.getUserInfo().access_token };
}

// prefixed function name with underscore because delete is a reserved word in javascript
function _delete(id) {

}

function handleResponse(response) {
    return response.text().then(text => {
        const data = text && JSON.parse(text);
        if (!response.ok || data.error_message) {
            if (response.status === 401) {
                // auto logout if 401 response returned from api
                logout();
                //this.location.reload(true);
            }

            const error = (data && data.error_message) || (data && data.message) || response.statusText;
            return Promise.reject(error);
        }

        return data;
    });
}