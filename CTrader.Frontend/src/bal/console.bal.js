
const config = require('../config');

export const consoleService = {
    start,
    stop,
    rebalance
}

function start() {
    const requestOptions = { method: 'GET' };
    return fetch(`${config.apiUrl}/console/start`, requestOptions).then(handleResponse).then(isokay => { return isokay; });
}

function stop() {
    const requestOptions = { method: 'GET' };
    return fetch(`${config.apiUrl}/console/stop`, requestOptions).then(handleResponse).then(isokay => { return isokay; });
}

function rebalance() {
    const requestOptions = { method: 'GET' };
    return fetch(`${config.apiUrl}/console/rebalance`, requestOptions).then(handleResponse).then(isokay => { return isokay; });
}

function handleResponse(response) {
    return response.text().then(text => {
        const data = text && JSON.parse(text);
        if (!response.ok || data.error_message) {
            if (response.status === 401) {
                // auto logout if 401 response returned from api
                //logout();
                //this.location.reload(true);
            }

            const error = (data && data.error_message) || (data && data.message) || response.statusText;
            return Promise.reject(error);
        }

        return data;
    });
}