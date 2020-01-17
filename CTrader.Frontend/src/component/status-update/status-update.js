import React, { Component } from 'react';
import { Session } from './../../helper/session';

class StatusUpdate extends Component {
  constructor(props) {
    super(props);
    this.handleClick = this.handleClick.bind(this);
  }

  handleClick(text,flag){
    Session.setItem("editTraderRow", JSON.stringify(text));
    document.getElementById("trader-dummy").click();
  }
  render() {
    return this.showPage(this.props.value);
  }
  showPage(text) {
    return (
      <div style={{cursor:"pointer"}} onClick={() => this.handleClick(text,false)}>{text}</div>
    );
  }
}

export default StatusUpdate;
