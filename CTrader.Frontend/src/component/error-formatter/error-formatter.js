import React, { Component } from 'react';
import './error-formatter.css';
import { Session } from './../../helper/session';
class ErrorFormatter extends Component {
  render() {
    var trader_id = this.props.data.trader_id;
    return this.showPage(this.props.value, trader_id);
  }
  isErrorPanicInLog(_trader_id) {
    var _ergonomy = Session.getItem("ergonomy");
    var _flag = false;
    if (_ergonomy != null) {
      var _tempList = JSON.parse(_ergonomy);
      var _temp = _tempList.filter(p => p.trader_id === _trader_id);
      _flag = _temp.length > 0 ? true : false;
    }
    return _flag;
  }
  showPage(text) {
    return (
      <div id={this.isErrorPanicInLog(text) ? "error-formatter" : "error"}>{text}</div>
    );
  }
}

export default ErrorFormatter;
