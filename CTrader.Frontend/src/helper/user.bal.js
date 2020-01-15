// import { authHeader } from '../helper/auth-header';
import { Session } from './session';

const config = require('../config');
var usersData = require('../models/users.json');

export const userBALService = {
    login,
    logout,
    getAll,
    getById,
    update,
    delete: _delete
};

function login(email, password) {
    try {
        let user = null;
        var usersList = JSON.parse(JSON.stringify(usersData.users));
        var users = usersList.filter(p => p.email === email);
        if (users.length > 0) {
            var tag = users[0].tags && users[0].tags.filter(t => t.tag === password);
            if (tag.length > 0) {
                user = users[0];
            } else {
                user = null;
            }
        }
        Session.setItem(config.token, JSON.stringify(user));
        return user;
    } catch (error) {
        console.log(error);
    }
}

function logout() {
    // remove user from local storage to log user out
    Session.removeItem(config.token);
}

function getAll() {

}

function getById(id) {

}

function update(user) {

}

// prefixed function name with underscore because delete is a reserved word in javascript
function _delete(id) {

}
