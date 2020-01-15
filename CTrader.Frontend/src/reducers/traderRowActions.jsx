import {types} from './types.jsx'

export const actions = {
  newTraderRow(newTraderRow) {
    return {
      type: types.ADD,
      payload: {newTraderRow}
    };
  },
  updateTraderRow(updatedTraderRow) {
    return {
      type: types.UPDATE,
      payload: {updatedTraderRow}
    };
  },
  deleteTraderRow(deletedTraderRow) {
    return {
      type: types.DELETE,
      payload: {deletedTraderRow}
    };
  }
};