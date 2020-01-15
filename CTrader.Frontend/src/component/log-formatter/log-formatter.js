import React, { Component } from 'react';
import { Utils } from './../../helper/utils';

class LogFormatter extends Component {
  render() {
    return this.showPage(this.props.value);
  }
  showPage(level) {
    return (
      <div style={{ backgroundColor: Utils.getLevelByEnum(level), color: "black", textAlign:"center" }}>{level}</div>
    );
  }
}

export default LogFormatter;
