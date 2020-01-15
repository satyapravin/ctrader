import React, { Component } from 'react';
import { Utils } from './../../helper/utils';
import { Session } from './../../helper/session';

class StatusFormatter extends Component {
  render() {
    // const value = Number(this.props.value);
    // const text = value.toLocaleString(undefined, { style: 'currency', currency: 'EUR' });
    return this.showPage(this.props.value, this.props.data);
  }
  handleClick(traderRow) {
    Session.setItem("editTraderRow", traderRow);
    document.getElementById("trader-dummy").click();
  }
  showPage(text, traderRow) {
    return (
      <div style={{cursor:"pointer", backgroundColor: Utils.getByEnum(text), textAlign:"center", color:"black", fontWeight:"bold" }} onClick={() => this.handleClick(traderRow)}>{text}</div>
    );
  }
}

export default StatusFormatter;
