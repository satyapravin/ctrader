import {types} from './types.jsx'

export const actions = {
  newPositionRow(newPositionRow) {
    return {
      type: types.ADD,
      payload: {newPositionRow}
    };
  },
  updatePositionRow(updatedPositionRow) {
    return {
      type: types.UPDATE,
      payload: {updatedPositionRow}
    };
  },
  deletePositionRow(deletedPositionRow) {
    return {
      type: types.DELETE,
      payload: {deletedPositionRow}
    };
  }
};