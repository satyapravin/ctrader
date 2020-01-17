import {createStore, applyMiddleware} from 'redux';

import traderReducer from './reducers/traderReducer.jsx';
import logger from "./middleware/logger.jsx";

const initialState = {
  traderRows: [
  ]
};

export default createStore(traderReducer, initialState, applyMiddleware(logger));
