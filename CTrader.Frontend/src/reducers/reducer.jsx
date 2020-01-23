import { types } from './types.jsx'

export default function Reducer(state = {}, action) {
  const payload = action.payload;
  switch (action.type) {
    case types.ADDPOSITION:
        payload.newPositionRow.__row_id__ = payload.newPositionRow.account + "~" + payload.newPositionRow.symbol;
      return {
        data : { ...state.data, positionRows: [ ...state.data.positionRows, payload.newPositionRow ] }
      };
    case types.UPDATEPOSITION:
      payload.updatedPositionRow.__row_id__ = payload.updatedPositionRow.account + "~" + payload.updatedPositionRow.symbol;
      var existingPositionRow = state.data.positionRows.filter(p => p.__row_id__ === payload.updatedPositionRow.__row_id__);
      if(existingPositionRow.length >= 1) {
        var previousPositionRow = {};
        for(let key of Object.keys(existingPositionRow[0])) {
          previousPositionRow[key] = existingPositionRow[0][key];
        }
        for(let key of Object.keys(payload.updatedPositionRow)) {
          previousPositionRow[key] = payload.updatedPositionRow[key];
        }
        return { 
          data : { ...state.data, positionRows: [ ...deletePositionRow(state.data.positionRows, payload.updatedPositionRow.__row_id__), previousPositionRow ] }
       };
      } else {
        return { data : { ...state.data, positionRows: [ ...state.data.positionRows, payload.updatedPositionRow ] } };
      }
    case types.DELETEPOSITION:
      payload.deletedPositionRow.__row_id__ = payload.deletedPositionRow.account + "~" + payload.deletedPositionRow.symbol;
      return { data : { ...state.data, positionRows: deletePositionRow(state.data.positionRows, payload.deletedPositionRow.__row_id__) } };
    case types.ADDORDER:
      payload.newOrderRow.__row_id__ = payload.newOrderRow.orderID;
      return { data : { ...state.data, orderRows: [ ...state.data.orderRows, payload.newOrderRow ] } };
    case types.UPDATEORDER:
      payload.updatedOrderRow.__row_id__ = payload.updatedOrderRow.orderID;
      var existingOrderRow = state.data.orderRows.filter(p => p.__row_id__ === payload.updatedOrderRow.__row_id__);
      if(existingOrderRow.length >= 1) {
        var previousOrderRow = { };
        for(let key of Object.keys(existingOrderRow[0])) {
          previousOrderRow[key] = existingOrderRow[0][key];
        }
        var keys = Object.keys(payload.updatedOrderRow);
        for(let key of keys) {
          previousOrderRow[key] = payload.updatedOrderRow[key];
        }
        return { data : { ...state.data, orderRows: [ ...deleteOrderRow(state.data.orderRows, payload.updatedOrderRow.__row_id__), previousOrderRow ] } };
      }
      else {
        return { data : { ...state.data, orderRows: [ ...state.data.orderRows, payload.updatedOrderRow ] } };
      }
    case types.DELETEORDER:
        payload.deletedOrderRow.__row_id__ = payload.deletedOrderRow.orderID;
        return { data : { ...state.data, orderRows: deleteOrderRow(state.data.orderRows, payload.deletedOrderRow.__row_id__) } };
    case types.ADDINSTRUMENT:
          payload.newInstrumentRow.__row_id__ = payload.newInstrumentRow.symbol;
        return { data : { ...state.data, instrumentRows: [ ...state.data.instrumentRows, payload.newInstrumentRow ] } };
    case types.UPDATEINSTRUMENT:
      payload.updatedInstrumentRow.__row_id__ = payload.updatedInstrumentRow.symbol;
      var existingInstrumentRow = state.data.instrumentRows.filter(p => p.__row_id__ === payload.updatedInstrumentRow.__row_id__);
      if(existingInstrumentRow.length >= 1) {
        var previousInstrumentRow = {};
        for(let key of Object.keys(existingInstrumentRow[0])) {
          previousInstrumentRow[key] = existingInstrumentRow[0][key];
        }
        for(let key of Object.keys(payload.updatedInstrumentRow)) {
          previousInstrumentRow[key] = payload.updatedInstrumentRow[key];
        }
        return { data : { ...state.data, instrumentRows: [ ...deleteInstrumentRow(state.data.instrumentRows, payload.updatedInstrumentRow.__row_id__), previousInstrumentRow ] } };
      }
      else {
        return { data : { ...state.data, instrumentRows: [ ...state.data.instrumentRows, payload.updatedInstrumentRow ] } };
      }
    case types.DELETEINSTRUMENT:
      payload.deletedInstrumentRow.__row_id__ = payload.deletedInstrumentRow.symbol;
      return { data : { ...state.data, instrumentRows: deleteInstrumentRow(state.data.instrumentRows, payload.deletedInstrumentRow.__row_id__) } };
    case types.ADDLOG:
      payload.newLogRow.__row_id__ = payload.newLogRow.timestamp + payload.newLogRow.message;
      return { data : { ...state.data, logRows: [ ...state.data.logRows, payload.newLogRow ] } };
    case types.UPDATELOG:
      payload.updatedLogRow.__row_id__ = payload.updatedLogRow.timestamp + payload.updatedLogRow.message;
      var existingLogRow = state.data.logRows.filter(p => p.__row_id__ === payload.updatedLogRow.__row_id__);
      if(existingLogRow.length >= 1) {
        var previousLogRow = {};
        for(let key of Object.keys(existingLogRow[0])) {
          previousLogRow[key] = existingLogRow[0][key];
        }
        for(let key of Object.keys(payload.updatedLogRow)) {
          previousLogRow[key] = payload.updatedLogRow[key];
        }
        return {  data : { ...state.data, logRows: [ ...deleteLogRow(state.data.logRows, payload.updatedLogRow.__row_id__), previousLogRow ] } };
      } else {
        return { data : { ...state.data, logRows: [ ...state.data.logRows, payload.updatedLogRow ] } };
      }
    case types.DELETELOG:
      payload.deletedLogRow.__row_id__ = payload.deletedLogRow.timestamp + payload.deletedLogRow.message;
      return { data : { ...state.data, logRows: deleteLogRow(state.data.logRows, payload.deletedLogRow.__row_id__) } };
    default:
      return state;
  }
}

const deletePositionRow = (positionRows, __row_id__) => positionRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const deleteOrderRow = (orderRows, __row_id__) => orderRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const deleteInstrumentRow = (instrumentRows, __row_id__) => instrumentRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const deleteLogRow = (logRows, __row_id__) => logRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const same = (arr1, arr2) => (arr1 && arr2 && arr1.toString() === arr2.toString());
