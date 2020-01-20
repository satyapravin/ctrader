import {createStore, applyMiddleware} from 'redux';
import reducer from './reducers/reducer.jsx';
import logger from "./middleware/logger.jsx";

const initialState = { 
  data : {
  positionRows: [
  ],
  orderRows: [
  ],
  instrumentRows: [
  ],
  logRows: [
  ]
}
};

export default createStore(reducer, initialState, applyMiddleware(logger));
