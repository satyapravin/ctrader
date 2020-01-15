
const config = require('../config');

export const consoleBALService = {
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