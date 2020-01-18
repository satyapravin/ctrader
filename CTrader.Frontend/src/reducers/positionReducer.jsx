import {types} from './types.jsx'

export default function positionReducer(state = {}, action) {
  const payload = action.payload;
  switch (action.type) {
    case types.ADD:
        payload.newPositionRow.__row_id__ = payload.newPositionRow.account + "~" + payload.newPositionRow.symbol;
      return {
        positionRows: [
          ...state.positionRows,
          payload.newPositionRow
        ]
      };
    case types.UPDATE:
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
          previous_row ] };
      }
      else 
      {
        return {
          positionRows: [
            ...state.positionRows,
            payload.updatedPositionRow
          ]
        };
    }
    case types.DELETE:
        payload.deletedPositionRow.__row_id__ = payload.deletedPositionRow.account + "~" + payload.deletedPositionRow.symbol;
        return {
        positionRows: deletePositionRow(state.positionRows, payload.deletedPositionRow.__row_id__)
      };
    default:
      return state;
  }
}
const deletePositionRow = (positionRows, __row_id__) => positionRows.slice().filter(f => !same(f.__row_id__, __row_id__));

const same = (arr1, arr2) => (arr1 && arr2 && arr1.toString() === arr2.toString());