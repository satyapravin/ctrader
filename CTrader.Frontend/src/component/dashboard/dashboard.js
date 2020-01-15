import React, { Component } from 'react';
import { Session } from './../../helper/session';
import config from './../../config';
class Dashboard extends Component {
  constructor(props) {
    super(props)
    this.state = {
      user: Session.getItem(config.token),
    }
  }

  render() {
    return this.showPage();
  }
  componentDidMount() {
  }

  showPage() {
    return (
      <div className='contact'>
        <div className="title">Dashboard!</div>
        <div className="content-container">
        <h4>CTrader</h4>
        </div>
      </div>
    );
  }
}

export default Dashboard;
