import config from './../../config';
import React, { Component } from 'react';
import { AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import 'ag-grid-community/dist/styles/ag-theme-balham-dark.css';

import './trade_summary.css';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSync } from "@fortawesome/free-solid-svg-icons";
import { faStopCircle } from '@fortawesome/free-solid-svg-icons';
import { faPlayCircle } from '@fortawesome/free-solid-svg-icons';
import { faRedo } from '@fortawesome/free-solid-svg-icons';
import { consoleService } from '../../bal/console.bal';
import OrderGrid from '../order_grid/OrderGrid';
import { AllModules } from "@ag-grid-enterprise/all-modules";
/**</import>****************************************************************************************************/
import { Session } from './../../helper/session';
import { connect} from "react-redux";
import { bindActionCreators } from 'redux';
import { actions } from '../../reducers/positionRowActions'
import * as PropTypes from "prop-types"; 
import hmacSha256 from 'crypto-js/hmac-sha256';
import CryptoJS from 'crypto-js';
import utf8 from 'utf8';

class TradeSummary extends Component {

  constructor(props) {
    super(props);
    this.mounted = false;
    this.timeout = 250;

    this.state = {
      modules: AllModules,
      columnDefs: [
        { headerName: "Account", field: "account", width: 150},
        { headerName: "Symbol", field: "symbol", width: 75}
      ],
      rowData: [],
      total: 0,
      ws: null,
      defaultColDef: { sortable: true, filter: true, resizable: true },
      user: Session.getItem(config.token),
      showPopup: false,
      display: "none"
    };
    this.start = this.start.bind(this);
    this.stop = this.stop.bind(this);
    this.rebalance = this.rebalance.bind(this);
    this.refresh = this.refresh.bind(this);
    this.refreshRequest = this.refreshRequest.bind(this);
  }
  
  /**<refresh>**************************************************************************************************/
  refresh() {
    window.location.reload();
  }
  /**</refresh>*************************************************************************************************/

  /**<refreshRequest>*******************************************************************************************/
  refreshRequest() {
    try {
      const { ws } = this.state;
      var refreshRequestData = this.getData("refresh_request");
      ws.send(refreshRequestData);
    } catch (error) {
      console.log('error:%o', error);
    }
  }
  /**</refreshRequest>******************************************************************************************/

  componentDidMount() {
    this.mounted = true;
    this.connectServer();
  }

  /**</componentWillUnmount>************************************************************************************/
  componentWillUnmount() {
    if (this.mounted) {
      const { ws } = this.state;
      console.log('Trader : componentWillUnmount');
      if (ws !== null && ws.readyState !== WebSocket.CLOSED) {
        ws.close();
        this.setState({ ws: ws });
      }
    }
  }

  /**<connectServer>********************************************************************************************/
  connectServer = () => {
    var ws = new WebSocket(config.wsUrl);
    var connectInterval;

    ws.onopen = () => {
      console.log("Connected...");
      const { user } = this.state;
      this.timeout = 250;
      clearTimeout(connectInterval);

      if (this.mounted) {
        //console.log("<trader>Setting state...");
        this.setState({ ws: ws, display: "none" });

        const APIKey = "PJwsol2h0OOjl7nS5nkKJIab";
        const secret = utf8.encode("YwWQPTVZpUv2gOQQmBdQ4wdvQtv0FMhC_liK_tzqe_zDJnml");
        const expires = 1580251174;
        const message = utf8.encode('GET/realtime' + expires);
        const signature =  hmacSha256(secret,message);
        const signat =  signature.toString();
        var _updateConnection = {"op" :"authKeyExpires", "args": [APIKey, expires, "39c2bfba7fb3f90b61dfce487473110589eb75442b8df2cbccea3a38a623897f"]};
        const authMessage = JSON.stringify(_updateConnection)
      ws.send(authMessage);
      }
    };

    ws.onmessage = e => {
      console.log(e) ;
      const { user } = this.state;
      let row = JSON.parse(e.data);
      //console.log("New Message - " + row.Traders);

      const { tradersList, logsList, tradersDictionary, logsDictionary } = this.state;
      if (row && row.success === true && row.request && row.request.op === "authKeyExpires") {
        console.log("Authentication successful!")
        var _Data = {"op" : "subscribe", "args":["position"]};
        ws.send(JSON.stringify(_Data));
      } else if (row && row.success === true && row.request && row.request.op === "subscribe") {
        console.log("Subscription successful for " + row.request.args);
        var _Data = {"op" : "subscribe", "args":["position"]};
        //ws.send(_Data);
      }
      else if (row && row.table && row.table === 'position') {
        //console.log('snapshotTraders:%o', row.Traders);
        if (this.mounted) {
          for (var index = 0; index < row.data.length; index++) {
            this.props.actions.updatePositionRow(row.data[index]);
          }
        }
      } else if (row.__MESSAGE__ === 'trader') {
        //console.log("__MESSAGE__ = trader - " + row.trader_id + " " + row.TAG + " " + row.acct);
        if (this.mounted) {
          try {
            //console.log("__MESSAGE__ = trader - " + row.trader_id + " " + row.TAG + " " + row.acct);
            if (!this.isEmpty(row.trader_id) && !this.isEmpty(row.TAG) && !this.isEmpty(row.acct)) {
              //console.log("__MESSAGE__ = trader - " + row.trader_id.toString() + " " + row.TAG.toString() + " " + row.acct.toString());
              this.props.actions.updateTraderRow(row);
            }
          } catch (error) {
            console.log('error:%o ', error);
            //console.log('trader-row:%o ', row);
          }
        }
      } else if (row.__MESSAGE__ === 'update_trader_status') {
        //console.log('update_trader_status');
        if (this.mounted) {
          this.props.actions.updateTraderRow(row);
        }
      } else if (row.__MESSAGE__ === 'delete_trader') {
        //console.log('delete_trader<row.trader_id>:%s', row.trader_id);
        if (this.mounted) {
          this.props.actions.deleteTraderRow(row);
        }
      } else if (row.__MESSAGE__ === 'snapshotLogs') {
        if (this.mounted) {
          var tempLogs = JSON.parse(row.Logs);
          let snapshotLogs_dictionary_key = "";
          for (var index = 0; index < tempLogs.length; index++) {
            snapshotLogs_dictionary_key = tempLogs[index].trader_id.toString().trim() + "~" + tempLogs[index].acct.toString().trim() + "~" + tempLogs[index].TAG.toString().trim();
            logsDictionary[snapshotLogs_dictionary_key] = tempLogs[index];
            logsList.push(tempLogs[index]);
          }
          var tempLogsList = this.copyData(logsDictionary);
          var _tempLogsErrorPanicList = tempLogsList.filter(p => p.level === "ERROR" || p.level === "PANIC");
          Session.setItem("ergonomy", JSON.stringify(_tempLogsErrorPanicList));
          this.setState({ logsDictionary: logsDictionary, logsList: tempLogsList, rowDataLog: tempLogsList });
        }
      }
      else if (row.__MESSAGE__ === 'log') {
        //console.log('%o', logsList);
        console.log('row.__MESSAGE__:%s | logsList: %o',row.__MESSAGE__, logsList);
        if (this.mounted) {
          try {
            if (!this.isEmpty(row.trader_id) && !this.isEmpty(row.TAG) && !this.isEmpty(row.acct)) {
              let log_dictionary_key = row.trader_id.toString().trim() + "~" + row.acct.toString().trim() + "~" + row.TAG.toString().trim();
              var _tempLogsList = this.updateLogsFields(logsDictionary, row, log_dictionary_key);
              var _tempErrorPanicList = _tempLogsList.filter(p => p.level === "ERROR" || p.level === "PANIC");
              Session.setItem("ergonomy", JSON.stringify(_tempErrorPanicList));
              this.setState({ logsDictionary: logsDictionary, logsList: _tempLogsList });
              if (this.refs.agGrid != null && this.refs.agLogGrid != null) {
                this.refs.agLogGrid.api.setRowData(_tempLogsList);
                //this.refs.agGrid.api.setRowData(tradersList);
              }
            }
          } catch (error) {
            console.log('error:%o', error);
            console.log('log-row:%o', error);
          }
        }
      }
      else if (row.__MESSAGE__ === 'tradersCached') {
        if (this.mounted) {
          var tempCachedTraders = JSON.parse(row.tradersCached);
          let tradersCached_dictionary_key = "";
          for (var iCachedIndex = 0; iCachedIndex < tempCachedTraders.length; iCachedIndex++) {
            tradersCached_dictionary_key = tempCachedTraders[iCachedIndex].trader_id.toString().trim() + "~" + tempCachedTraders[iCachedIndex].acct.toString().trim() + "~" + tempCachedTraders[iCachedIndex].TAG.toString().trim();
            tradersDictionary[tradersCached_dictionary_key] = tempCachedTraders[iCachedIndex];
            tradersList.push(tempCachedTraders[iCachedIndex]);
          }
          var tempCachedTradersList = this.copyData(tradersDictionary);
          this.setState({ tradersDictionary: tradersDictionary, tradersList: tempCachedTradersList, rowData: tempCachedTradersList });
          //this.refs.agGrid.api.updateRowData({add: tempTradersList});
        }
      }
      else if (row.__MESSAGE__ === 'logsCached') {
        if (this.mounted) {
          var tempCachedLogs = JSON.parse(row.logsCached);
          let logsCached_dictionary_key = "";
          for (var iCachedLogIndex = 0; iCachedLogIndex < tempCachedLogs.length; iCachedLogIndex++) {
            logsCached_dictionary_key = tempCachedLogs[iCachedIndex].trader_id.toString().trim() + "~" + tempCachedLogs[iCachedIndex].acct.toString().trim() + "~" + tempCachedLogs[iCachedIndex].TAG.toString().trim();
            logsDictionary[logsCached_dictionary_key] = tempCachedLogs[iCachedLogIndex];
            logsList.push(tempCachedLogs[iCachedLogIndex]);
          }
          var tempCachedLogsList = this.copyData(logsDictionary);
          this.setState({ logsDictionary: logsDictionary, logsList: tempCachedLogsList, rowDataLog: tempCachedLogsList });
          //this.refs.agGrid.api.updateRowData({add: tempTradersList});
        }
      }
      //console.log(tradersList);
    };

    ws.onerror = (e) => {
      console.log("On error message...");
      if (this.mounted) { this.setState({ display: 'block' }); }
      console.log(`Socket is closed. Reconnecting in  ${Math.min(10000 / 1000, (this.timeout + this.timeout) / 1000)} second.`);
      this.timeout = this.timeout + this.timeout;
      connectInterval = setTimeout(this.check, Math.min(10000, this.timeout));
      //console.log("End On error message...");
    };
    ws.onclose = (e) => {
      if (ws != null) { ws.close(); }
      //console.log("completed on close message...");
    };
  };
  /**</connectServer>*******************************************************************************************/

  /**<check>***************************************************************************************************/
  check = () => {
    const { ws } = this.state;
    //check if websocket instance is closed, if so call `connectServer` function.
    if (!ws || ws.readyState === WebSocket.CLOSED) this.connectServer();
  };

  
  isEmpty(value){
    return (value == null || value.length === 0);
  }
  /**<getData>**************************************************************************************************/
  getData(command, payload) {
    //Copy the values from the payload object, if one was supplied
    var payloadCopy = {};
    if (payload !== undefined && payload !== null) {
      var keys = Object.keys(payload);
      for (var index in keys) {
        var key = keys[index];
        payloadCopy[key] = payload[key];
      }
    }
    return {"op": command, "args": [payload]};
  }
  /**</getData>*************************************************************************************************/


  start() {
    var result = consoleService.start();
  }

  stop() {
    var result = consoleService.stop();
  }

  rebalance() {
    var result = consoleService.rebalance();
  }

  render() {
    return this.getTemplate();
  }

  getTemplate() {
    return (
      <div className='home'>
        <div className="title">Trade Summary</div>
        <div style={{ resize: "vertical", overflow: "auto", height: "38vh", width: '100%', padding: "5px 0px 8px 0px", position: "relative", borderTop: "solid 1px white" }} className="ag-theme-balham-dark">
          <AgGridReact
                      ref="agGrid"
                      modules={this.state.modules}
                      columnDefs={this.state.columnDefs}
                      defaultColDef={this.state.defaultColDef}
                      rowData={this.props.positionRows}
                      //onGridReady={params => params.api.sizeColumnsToFit()}
                      deltaRowDataMode={true}
                      getRowNodeId={data => data.__row_id__} 
                      />
         </div>
        <div className="content-container">
          <div id="home-accept-data">
            <div className="row">
              <div className="col-xs-12 col-sm-12 col-md-12 col-lg-12">
                <div className="form-group">
                    <h4>CTrader</h4>
                    <table>
                      <tbody>
                        <tr>
                          <td><OrderGrid instrument="XBTUSD"/></td>
                          <td><OrderGrid instrument="ETHUSD"/></td>
                          <td><OrderGrid instrument="ETHH20"/></td>
                          <td>
                            <table>
                              <tbody>
                              <tr>
                                  <td>Total</td>
                                  <td>{this.state.total}</td>
                                </tr>
                                <tr>
                                  <td>Available</td>
                                  <td>{this.state.total}</td>
                                </tr>
                                <tr>
                                  <td>Fair Price</td>
                                  <td>{this.state.total}</td>
                                </tr>
                                <tr>
                                  <td>Target</td>
                                  <td>{this.state.total}</td>
                                </tr>
                                <tr>
                                  <td>Time Left</td>
                                  <td>{this.state.total}</td>
                                </tr>
                              </tbody>
                            </table>
                          </td>
                        </tr>  
                        <tr>
                          <td>
                            <br/>
                            <div title="Start" style={{float:"right", paddingRight:"15px", cursor:"pointer"}} onClick={this.start}><FontAwesomeIcon icon={faPlayCircle} /></div>
                            <div title="Stop" style={{float:"right", paddingRight:"15px", cursor:"pointer"}} onClick={this.stop}><FontAwesomeIcon icon={faStopCircle} /></div>
                            <div title="Rebalance" style={{float:"right", paddingRight:"15px", cursor:"pointer"}} onClick={this.rebalance}><FontAwesomeIcon icon={faRedo} /></div>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                </div>
              </div>
            </div>
          </div>
          <div id="home-display-data">
             </div>
        </div>
      </div>
    );
  }
}


//const mapStateToProps = (state) => ({traderRows: state.traderRows});
//const mapDispatchToProps = (dispatch) => ({actions: bindActionCreators(actions, dispatch)});

TradeSummary.contextTypes = {
  store: PropTypes.object                         // must be supplied when using redux with AgGridReact
};

const mapStateToProps = (state) => ({positionRows: state.positionRows});
const mapDispatchToProps = (dispatch) => ({actions: bindActionCreators(actions, dispatch)});

export default connect(mapStateToProps, mapDispatchToProps)(TradeSummary);