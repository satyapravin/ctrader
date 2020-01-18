import {types} from './types.jsx'

export default function positionReducer(state = {}, action) {
  const payload = action.payload;
  switch (action.type) {
    case types.ADDPOSITION:
        payload.newPositionRow.__row_id__ = payload.newPositionRow.account + "~" + payload.newPositionRow.symbol;
      return {
        positionRows: [
          ...state.positionRows,
          payload.newPositionRow
        ],
        orderRows: state.orderRows
      };
    case types.UPDATEPOSITION:
      payload.updatedPositionRow.__row_id__ = payload.updatedPositionRow.account + "~" + payload.updatedPositionRow.symbol;
      var existing_row = state.positionRows.filter(p => p.__row_id__ === payload.updatedPositionRow.__row_id__);
      if(existing_row.length >= 1) {
        var previous_row = {};
        for(let key of Object.keys(existing_row[0])) {
          previous_row[key] = existing_row[0][key];
        }
        var keys = Object.keys(payload.updatedPositionRow);
        for(let key of keys) {
          previous_row[key] = payload.updatedPositionRow[key];
        }
        return { positionRows: [
          ...deletePositionRow(state.positionRows, payload.updatedPositionRow.__row_id__), 
          previous_row ],
          orderRows: state.orderRows };
      }
      else 
      {
        return {
          positionRows: [
            ...state.positionRows,
            payload.updatedPositionRow
          ],
          orderRows: state.orderRows
        };
    }
    case types.DELETEPOSITION:
        payload.deletedPositionRow.__row_id__ = payload.deletedPositionRow.orderID;
        return {
        positionRows: deletePositionRow(state.positionRows, payload.deletedPositionRow.__row_id__),
        orderRows: state.orderRows
      };
    case types.ADDORDER:
        payload.newOrderRow.__row_id__ = payload.newOrderRow.orderID;
      return {
        orderRows: [
          ...state.orderRows,
          payload.newOrderRow
        ],
        positionRows: state.positionRows
      };
    case types.UPDATEORDER:
      payload.updatedOrderRow.__row_id__ = payload.updatedOrderRow.orderID;
      var existing_row = state.orderRows.filter(p => p.__row_id__ === payload.updatedOrderRow.__row_id__);
      if(existing_row.length >= 1) {
        var previous_row = {};
        for(let key of Object.keys(existing_row[0])) {
          previous_row[key] = existing_row[0][key];
        }
        var keys = Object.keys(payload.updatedOrderRow);
        for(let key of keys) {
          previous_row[key] = payload.updatedOrderRow[key];
        }
        return { orderRows: [
          ...deleteOrderRow(state.orderRows, payload.updatedOrderRow.__row_id__), previous_row ],
          positionRows: state.positionRows
         };
      }
      else 
      {
        return {
          orderRows: [
            ...state.orderRows,
            payload.updatedOrderRow
          ],
          positionRows: state.positionRows
        };
    }
    case types.DELETEORDER:
        payload.deletedOrderRow.__row_id__ = payload.deletedOrderRow.account + "~" + payload.deletedOrderRow.symbol;
        return {
          orderRows: deleteOrderRow(state.orderRows, payload.deletedOrderRow.__row_id__),
          positionRows: state.positionRows
      };
    default:
      return state;
  }
}
const deletePositionRow = (positionRows, __row_id__) => positionRows.slice().filter(f => !same(f.__row_id__, __row_id__));
const deleteOrderRow = (orderRows, __row_id__) => orderRows.slice().filter(f => !same(f.__row_id__, __row_id__));

const same = (arr1, arr2) => (arr1 && arr2 && arr1.toString() === arr2.toString());