/**
 * setItem(key, value) – store key/value pair.
getItem(key) – get the value by key.
removeItem(key) – remove the key with its value.
clear() – delete everything.
key(index) – get the key on a given position.
length – the number of stored items.
Use Object.keys to get all keys.
 */
export const Session = {
    setItem,
    getItem,
    removeItem,
    clear,
    getKeys,
    getKey,
};

function setItem(key, value) {
    try {
        sessionStorage.setItem(key, value);
    } catch (error) {
        console.log(error)
    }
}

function getItem(key) {
    try {
        return sessionStorage.getItem(key);
    } catch (error) {
        console.log(error)
    }
}

function removeItem(key) {
    try {
        sessionStorage.removeItem(key);
    } catch (error) {
        console.log(error)
    }
}
function clear() {
    try {
        sessionStorage.clear();
    } catch (error) {
        console.log(error)
    }
}
function getKeys() {
    try {
        let keys = Object.keys(sessionStorage);
        let keysValue = [];
        for (let key of keys) {
            keysValue[key] = { "key": key, "value": sessionStorage.getItem(key) };
        }
        return keysValue;
    } catch (error) {
        console.log(error)
    }
}
function getKey(index) {
    try {
        return sessionStorage.key(index);
    } catch (error) {
        console.log(error)
    }
}