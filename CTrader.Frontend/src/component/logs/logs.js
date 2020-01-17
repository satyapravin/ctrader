import React, { Component } from 'react';
import config from './../../config';
import { Session } from './../../helper/session';

class Logs extends Component {

  constructor(props) {
    super(props);
    this.state = {
      user: JSON.parse(Session.getItem(config.token)),
      logsList: [],
    };
    this.ws = new WebSocket(config.Url);
  }

  componentDidMount() {
    this.ws.onopen = () => {
      this.ws.send(JSON.stringify(this.state.user));
    }

    this.ws.onmessage = e => {
      var packageData = JSON.parse(e.data)[1];
      this.setState({ logsList: packageData });
    }

    this.ws.onclose = () => {
      this.ws.close();
    }
  }

  componentWillUnmount() {
    this.ws.close();
  }
  showLogs(){
    const { logsList } = this.state;
    let logRows = logsList.map(function (_log, index) {
      return (
        <tr key={index}>
          <td>{_log.trader_id}</td>
          <td>{_log.acct}</td>
          <td>{_log.TAG}</td>
          <td>{_log.ts}</td>
          <td>{_log.level}</td>
          <td>{_log.text}</td>
        </tr>
      );
    });
    return logRows;
  }

  render() {
    return this.showPage();
  }

  showPage() {
    return (
      <div id='logs'>
          <div className="title">Logs View</div>
          <div className="content-container">
            <div className="display-data-area">
              <table className="trader-table">
                <thead>
                  <tr>
                    <th>#Id</th>
                    <th>Account</th>
                    <th>TAG</th>
                    <th>TS</th>
                    <th>Level</th>
                    <th>Text</th>
                  </tr>
                </thead>
                <tbody className="table-body">
                  {this.showLogs()}
                </tbody>
              </table>
            </div>
          </div>
      </div>
    );
  }
}

export default Logs;
