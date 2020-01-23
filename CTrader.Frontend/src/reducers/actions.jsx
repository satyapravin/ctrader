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
  },
  newInstrumentRow(newInstrumentRow) {
    return {
      type: types.ADDINSTRUMENT,
      payload: {newInstrumentRow}
    };
  },
  updateInstrumentRow(updatedInstrumentRow) {
    return {
      type: types.UPDATEINSTRUMENT,
      payload: {updatedInstrumentRow}
    };
  },
  deleteInstrumentRow(deletedInstrumentRow) {
    return {
      type: types.DELETEINSTRUMENT,
      payload: {deletedInstrumentRow}
    };
  },
  newLogRow(newLogRow) {
    return {
      type: types.ADDLOG,
      payload: {newLogRow}
    };
  },
  updateLogRow(updatedLogRow) {
    return {
      type: types.UPDATELOG,
      payload: {updatedLogRow}
    };
  },
  deleteLogRow(deletedLogRow) {
    return {
      type: types.DELETELOG,
      payload: {deletedLogRow}
    };
  }
};