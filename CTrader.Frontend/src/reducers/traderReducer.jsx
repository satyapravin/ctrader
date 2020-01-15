import {types} from './types.jsx'

export default function traderReducer(state = {}, action) {
  const payload = action.payload;
  switch (action.type) {
    case types.ADD:
        payload.newTraderRow.__row_id__ = payload.newTraderRow.trader_id + "~" + payload.newTraderRow.acct + "~" + payload.newTraderRow.TAG;
      return {
        traderRows: [
          ...state.traderRows,
          payload.newTraderRow
        ]
      };
    case types.UPDATE:
      payload.updatedTraderRow.__row_id__ = payload.updatedTraderRow.trader_id + "~" + payload.updatedTraderRow.acct + "~" + payload.updatedTraderRow.TAG;
      var existing_row = state.traderRows.filter(p => p.__row_id__ === payload.updatedTraderRow.__row_id__);
      if(existing_row.length >= 1) {
        var previous_row = {};
        for(let key of Object.keys(existing_row[0])) {
          previous_row[key] = existing_row[0][key];
        }
        var keys = Object.keys(payload.updatedTraderRow);
        for(let key of keys) {
          previous_row[key] = payload.updatedTraderRow[key];
        }
        return { traderRows: [
          ...deleteTraderRow(state.traderRows, payload.updatedTraderRow.__row_id__), 
          previous_row ] };
      }
      else 
      {
        return {
          traderRows: [
            ...state.traderRows,
            payload.updatedTraderRow
          ]
        };
    }
    case types.DELETE:
        payload.deletedTraderRow.__row_id__ = payload.deletedTraderRow.trader_id + "~" + payload.deletedTraderRow.acct + "~" + payload.deletedTraderRow.TAG;
        return {
        traderRows: deleteTraderRow(state.traderRows, payload.deletedTraderRow.__row_id__)
      };
    default:
      return state;
  }
}
const deleteTraderRow = (traderRows, __row_id__) => traderRows.slice().filter(f => !same(f.__row_id__, __row_id__));

const same = (arr1, arr2) => (arr1 && arr2 && arr1.toString() === arr2.toString());