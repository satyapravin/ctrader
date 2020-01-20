import {types} from './types.jsx'

export default function positionReducer(state = {}, action) {
  const payload = action.payload;
  switch (action.type) {
    case types.ADDPOSITION:
        payload.newPositionRow.__row_id__ = payload.newPositionRow.account + "~" + payload.newPositionRow.symbol;
      return {
        data : {
          ...state.data,
          positionRows: [
            ...state.data.positionRows,
            payload.newPositionRow
          ]
      }
      };
    case types.UPDATEPOSITION:
      payload.updatedPositionRow.__row_id__ = payload.updatedPositionRow.account + "~" + payload.updatedPositionRow.symbol;
      var existing_row = state.data.positionRows.filter(p => p.__row_id__ === payload.updatedPositionRow.__row_id__);
      if(existing_row.length >= 1) {
        var previous_row = {};
        for(let key of Object.keys(existing_row[0])) {
          previous_row[key] = existing_row[0][key];
        }
        var keys = Object.keys(payload.updatedPositionRow);
        for(let key of keys) {
          previous_row[key] = payload.updatedPositionRow[key];
        }
        return { 
          data : {
            ...state.data,
            positionRows: [
            ...deletePositionRow(state.data.positionRows, payload.updatedPositionRow.__row_id__), 
            previous_row ]
          }
       };
      }
      else {
        return {
          data : {
              ...state.data,
            positionRows: [
              ...state.data.positionRows,
              payload.updatedPositionRow
            ]
          }
        };
    }
    case types.DELETEPOSITION:
        payload.deletedPositionRow.__row_id__ = payload.deletedPositionRow.account + "~" + payload.deletedPositionRow.symbol;
        return {
          data : {
            ...state.data,
            positionRows: deletePositionRow(state.data.positionRows, payload.deletedPositionRow.__row_id__),
          }
      };
      case types.ADDORDER:
        payload.newOrderRow.__row_id__ = payload.newOrderRow.orderID;
      return {
        data : {
            ...state.data,
          orderRows: [
            ...state.data.orderRows,
            payload.newOrderRow
          ]
        }
      };
    case types.UPDATEORDER:
      payload.updatedOrderRow.__row_id__ = payload.updatedOrderRow.orderID;
      var existing_row = state.data.orderRows.filter(p => p.__row_id__ === payload.updatedOrderRow.__row_id__);
      if(existing_row.length >= 1) {
        var previous_row = {};
        for(let key of Object.keys(existing_row[0])) {
          previous_row[key] = existing_row[0][key];
        }
        var keys = Object.keys(payload.updatedOrderRow);
        for(let key of keys) {
          previous_row[key] = payload.updatedOrderRow[key];
        }
        return { 
          data : {
            ...state.data, 
            orderRows: [ ...deleteOrderRow(state.data.orderRows, payload.updatedOrderRow.__row_id__), previous_row ]
          }
         };
      }
      else 
      {
        return {
          data : {
            ...state.data, 
            orderRows: [
              ...state.data.orderRows,
              payload.updatedOrderRow
            ]
          }
        };
    }
    case types.DELETEORDER:
        payload.deletedOrderRow.__row_id__ = payload.deletedOrderRow.orderID;
        return {
          data : {
            ...state.data, 
            orderRows: deleteOrderRow(state.data.orderRows, payload.deletedOrderRow.__row_id__)
          }
        };
    case types.ADDINSTRUMENT:
          payload.newInstrumentRow.__row_id__ = payload.newInstrumentRow.symbol;
        return {
          data : {
              ...state.data,
            instrumentRows: [
              ...state.data.instrumentRows,
              payload.newInstrumentRow
            ]
          }
        };
      case types.UPDATEINSTRUMENT:
        payload.updatedInstrumentRow.__row_id__ = payload.updatedInstrumentRow.symbol;
        var existing_row = state.data.instrumentRows.filter(p => p.__row_id__ === payload.updatedInstrumentRow.__row_id__);
        if(existing_row.length >= 1) {
          var previous_row = {};
          for(let key of Object.keys(existing_row[0])) {
            previous_row[key] = existing_row[0][key];
          }
          var keys = Object.keys(payload.updatedInstrumentRow);
          for(let key of keys) {
            previous_row[key] = payload.updatedInstrumentRow[key];
          }
          return { 
            data : {
              ...state.data, 
              instrumentRows: [ ...deleteInstrumentRow(state.data.instrumentRows, payload.updatedInstrumentRow.__row_id__), previous_row ]
            }
           };
        }
        else 
        {
          return {
            data : {
              ...state.data, 
              instrumentRows: [
                ...state.data.instrumentRows,
                payload.updatedInstrumentRow
              ]
            }
          };
      }
      case types.DELETEINSTRUMENT:
          payload.deletedInstrumentRow.__row_id__ = payload.deletedInstrumentRow.symbol;
          return {
            data : {
              ...state.data, 
              instrumentRows: deleteInstrumentRow(state.data.instrumentRows, payload.deletedInstrumentRow.__row_id__)
            }
          };
      default:
      return state;
  }
}
const deletePositionRow = (positionRows, __row_id__) => positionRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const deleteOrderRow = (orderRows, __row_id__) => orderRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const deleteInstrumentRow = (instrumentRows, __row_id__) => instrumentRows.slice().filter(f => !same(f.__row_id__, __row_id__));

const same = (arr1, arr2) => (arr1 && arr2 && arr1.toString() === arr2.toString());