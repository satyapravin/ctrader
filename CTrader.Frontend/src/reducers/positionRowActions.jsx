import {types} from './types.jsx'

export const actions = {
  newPositionRow(newPositionRow) {
    return {
      type: types.ADDPOSITION,
      payload: {newPositionRow}
    };
  },
  updatePositionRow(updatedPositionRow) {
    return {
      type: types.UPDATEPOSITION,
      payload: {updatedPositionRow}
    };
  },
  deletePositionRow(deletedPositionRow) {
    return {
      type: types.DELETEPOSITION,
      payload: {deletedPositionRow}
    };
  },
  newOrderRow(newOrderRow) {
    return {
      type: types.ADDORDER,
      payload: {newOrderRow}
    };
  },
  updateOrderRow(updatedOrderRow) {
    return {
      type: types.UPDATEORDER,
      payload: {updatedOrderRow}
    };
  },
  deleteOrderRow(deletedOrderRow) {
    return {
      type: types.DELETEORDER,
      payload: {deletedOrderRow}
    };
  }
};