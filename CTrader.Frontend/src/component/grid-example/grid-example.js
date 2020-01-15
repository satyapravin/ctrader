import React, { Component } from 'react';
import { AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import 'ag-grid-community/dist/styles/ag-theme-balham-dark.css';
import StatusFormatter from './../status-formatter/status-formatter';
import StatusUpdate from './../status-update/status-update';
import Popup from './../popup/popup';
import { Session } from './../../helper/session';

class GridExample extends Component {
  constructor(props) {
    super(props);
    this.state = {

      columnDefsTrader: [
        { headerName: "#Id", field: "trader_id", sortable: true, filter: true, width: 80, cellRenderer: 'statusUpdate' },
        { headerName: "Account", field: "acct", sortable: true, filter: true, width: 90 },
        { headerName: "TAG", field: "TAG", sortable: true, filter: true, width: 70 },
        { headerName: "Target Instrument", field: "target", sortable: true, filter: true, width: 145 },
        { headerName: "Currency", field: "currency", sortable: true, filter: true, width: 89 },
        { headerName: "PNL", field: "pnl", sortable: true, filter: true, width: 65 },
        { headerName: "Position", field: "position", sortable: true, filter: true, width: 85 },
        { headerName: "Volume", field: "volume", sortable: true, filter: true, width: 85 },
        { headerName: "Notional", field: "notional", sortable: true, filter: true, width: 90 },
        { headerName: "Status", field: "status", sortable: true, filter: true, width: 138, cellRenderer: 'statusFormatter'},
        { headerName: "Last Updated", field: "last_updated", sortable: true, filter: true, width: 155 },
        { headerName: "Last Traded", field: "last_traded", sortable: true, filter: true, width: 155 },
        { headerName: "New Count", field: "new_count", sortable: true, filter: true, width: 105 },
        { headerName: "Cancel Count", field: "cancel_count", sortable: true, filter: true, width: 113 },
        { headerName: "Reject Count", field: "reject_count", sortable: true, filter: true, width: 113 },
        { headerName: "Free Text", field: "free_text", sortable: true, filter: true, width: 90 },
      ],
      traders: [
        { "trader_id": "TID00001", "acct": "00001", "TAG": "1", "target": "target", "currency": "$", "pnl": "20.50", "position": "1", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "OFF", "free_text": "na" },
        { "trader_id": "TID00002", "acct": "00002", "TAG": "2", "target": "target2", "currency": "$", "pnl": "15.50", "position": "5", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "NORMAL", "free_text": "na" },
        { "trader_id": "TID00003", "acct": "00003", "TAG": "3", "target": "target3", "currency": "$", "pnl": "10.50", "position": "7", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "CLOSE_POSITIONS", "free_text": "na" },
        { "trader_id": "TID00004", "acct": "00004", "TAG": "4", "target": "target4", "currency": "$", "pnl": "9.50", "position": "9", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "CLOSE_NOW", "free_text": "na" },
        { "trader_id": "TID00005", "acct": "00005", "TAG": "5", "target": "target5", "currency": "$", "pnl": "6.50", "position": "1", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "ABNORMAL", "free_text": "na" },
        { "trader_id": "TID00006", "acct": "00006", "TAG": "6", "target": "target6", "currency": "$", "pnl": "5.50", "position": "5", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "ABNORMAL", "free_text": "na" },
        { "trader_id": "TID00007", "acct": "00007", "TAG": "1", "target": "target", "currency": "$", "pnl": "17.50", "position": "3", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "NORMAL", "free_text": "na" },
        { "trader_id": "TID00008", "acct": "00008", "TAG": "8", "target": "target8", "currency": "$", "pnl": "14.50", "position": "7", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "CLOSE_POSITIONS", "free_text": "na" },
        { "trader_id": "TID00009", "acct": "00009", "TAG": "9", "target": "target9", "currency": "$", "pnl": "12.50", "position": "2", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "NORMAL", "free_text": "na" },
        { "trader_id": "TID00010", "acct": "00010", "TAG": "10", "target": "target10", "currency": "$", "pnl": "18.50", "position": "1", "volume": "100", "notional": "10.00", "last_updated": "2019-10-19 09:45:45 AM", "last_traded": "2019-10-19 09:45:45 AM", "new_count": "0", "cancel_count": "0", "reject_count": "0", "status": "CLOSE_POSITIONS", "free_text": "na" }
      ],
      frameworkComponents:{'statusFormatter': StatusFormatter, 'statusUpdate':StatusUpdate},
      showPopup: false,
      trader:null,
      
    }
    this.togglePopup = this.togglePopup.bind(this);
    this.callTogglePopup = this.callTogglePopup.bind(this);
  };
  componentDidMount() {
  }

  callTogglePopup() {
    const {traders} = this.state;
    var _trader_id = Session.getItem("trader_id");
    var trader = traders.filter(p => p.trader_id === JSON.parse(_trader_id));
    this.togglePopup(trader[0], false);
    console.log('trader_id:%s | trader:%o',_trader_id, trader[0]);
  }
  togglePopup(_trader, flag) {
      this.setState({ showPopup: !this.state.showPopup, trader: _trader });

      // if (flag) {}    
  }

  render() {
    return (
      <div>
        <h3>testing</h3>
        <div style={{ height: '350px', width: '100%' }} className="ag-theme-balham-dark">
          <AgGridReact columnDefs={this.state.columnDefsTrader} rowData={this.state.traders} 
          frameworkComponents={this.state.frameworkComponents}>
          </AgGridReact>
        </div>
        
        <button id="trader-dummy" type="button" style={{ display: "none" }} onClick={this.callTogglePopup}>Go!</button>
        {
          this.state.showPopup ?
            <Popup trader={this.state.trader} text='Click "Close Button" to hide popup' closePopup={this.togglePopup} />
            : null
        }
      </div>
    );
  }
}

export default GridExample;
