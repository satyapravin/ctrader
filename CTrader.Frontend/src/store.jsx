import {createStore, applyMiddleware} from 'redux';

import positionReducer from './reducers/positionReducer.jsx';
import logger from "./middleware/logger.jsx";

const initialState = { 
  positionRows: [
  ],
  orderRows: [
  ]
};

export default createStore(positionReducer, initialState, applyMiddleware(logger));
