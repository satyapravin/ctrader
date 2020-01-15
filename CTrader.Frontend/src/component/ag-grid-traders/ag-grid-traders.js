/**<import>****************************************************************************************************/
import React, { Component } from 'react';
import * as PropTypes from "prop-types";
import { connect} from "react-redux";
import { bindActionCreators } from 'redux';
import { AgGridReact } from 'ag-grid-react';
import { actions } from '../../reducers/traderRowActions'

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSync, faRetweet } from "@fortawesome/free-solid-svg-icons";
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import 'ag-grid-community/dist/styles/ag-theme-balham-dark.css';
import { AllModules } from "@ag-grid-enterprise/all-modules";

import StatusFormatter from '../status-formatter/status-formatter';
import DateFormatter from '../date-formatter/date-formatter';
import LogFormatter from '../log-formatter/log-formatter';
import ErrorFormatter from '../error-formatter/error-formatter';

import config from './../../config';
import './ag-grid-traders.css';
/**</import>****************************************************************************************************/
import { Session } from './../../helper/session';

/**<class AgGridTraders>****************************************************************************************/
class AgGridTraders extends Component {
  /**<constructor>**********************************************************************************************/
  constructor(props) {
    super(props);
    this.mounted = false;
    this.timeout = 250;
      
    //<this.state>
    this.state = {
      modules: AllModules,
      columnDefs: [
        { headerName: "#Id", field: "trader_id", width: 200,cellRenderer: 'errorFormatter' },
        { headerName: "Account", field: "acct", width: 150},
        { headerName: "TAG", field: "TAG", width: 75},
        {
          headerName: "Status-Edit", field: "status", width: 138, sortable: true, filter: true, resizable: true, editable:true, cellRenderer: 'statusFormatter',
          cellEditor: "agSelectCellEditor",
          cellEditorParams: {  cellHeight: 150, values: ['OFF', 'NORMAL', 'CLOSE_POSITIONS','CLOSE_NOW','ABNORMAL']}
        },        
        { headerName: "Target Instrument", field: "target", width: 150},
        { headerName: "Currency", field: "currency", width: 100},
        { headerName: "Volume", field: "volume", width: 100},
        { headerName: "Volume Cancelled", field: "volume_cancelled", width: 150},
        { headerName: "Fill Ratio", valueGetter: "(data.volume / (data.volume + data.volume_cancelled)).toFixed(2)", width: 150},
        { headerName: "PNL", field: "pnl", width: 100},
        { headerName: "Position", field: "position", width: 100},
        { headerName: "Notional", field: "notional", width: 100},
        { headerName: "Last Updated", field: "last_updated", width: 150, cellRenderer: 'dateFormatter' },
        { headerName: "Last Traded", field: "last_traded", width: 150, cellRenderer: 'dateFormatter' },
        { headerName: "New Count", field: "new_count", width: 150},
        { headerName: "Cancel Count", field: "cancel_count", width: 150},
        { headerName: "Reject Count", field: "reject_count", width: 150},
        { headerName: "Free Text", field: "free_text", width: 150},
      ],
      rowData: [],
      columnDefsLog: [
        { headerName: "#Id", field: "trader_id"},
        { headerName: "Account", field: "acct"},
        { headerName: "TAG", field: "TAG"},
        { headerName: "TS", field: "ts"},
        { headerName: "Level", field: "level", cellRenderer: 'logFormatter' },
        { headerName: "Text", field: "txt", width: 315},
      ],
      defaultColDef: { sortable: true, filter: true, resizable: true },
      rowDataLog: [],
      ws: null,
      tradersDictionary: {},
      logsDictionary: {},
      user: Session.getItem(config.token),
      logsList: [],
      tradersList: [],
      frameworkComponents: { 'statusFormatter': StatusFormatter, 'dateFormatter': DateFormatter, 'errorFormatter': ErrorFormatter },
      frameworkComponentsLog: { 'logFormatter': LogFormatter },
      showPopup: false,
      display: "none",
      trader: null,
    }
    //</this.state>

    this.togglePopup = this.togglePopup.bind(this);
    this.cancelPopup = this.cancelPopup.bind(this);
    this.callTogglePopup = this.callTogglePopup.bind(this);
    this.refresh = this.refresh.bind(this);
    this.refreshRequest = this.refreshRequest.bind(this);
  };
  /**</constructor>*********************************************************************************************/

  /**<callTogglePopup>******************************************************************************************/
  callTogglePopup() {
    var editTraderRow = Session.getItem("editTraderRow");
  this.togglePopup(editTraderRow, false);
    console.log('__row_id__:%s | trader:%o', editTraderRow.__row_id__, editTraderRow);
  }
  /**</callTogglePopup>*****************************************************************************************/

  /**<togglePopup>**********************************************************************************************/
  togglePopup(_trader, flag) {
    if (this.mounted) {
      this.setState({ showPopup: !this.state.showPopup, trader: _trader });
      if (flag) {
        const { ws } = this.state;
        var editTraderRow = Session.getItem("editTraderRow");
        var updateData = this.getData("update_trader_status", editTraderRow);
        ws.send(updateData);
      }
    }
  }
  /**</togglePopup>*********************************************************************************************/

  /**<cancelPopup>**********************************************************************************************/
  cancelPopup() {
    if (this.mounted) {
      this.setState({ showPopup: !this.state.showPopup });
    }
  }
  /**</cancelPopup>*********************************************************************************************/

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

  /**<componentDidMount>****************************************************************************************/
  componentDidMount() {
    this.mounted = true;
    if (this.mounted) this.connectServer();
  }
  /**</componentDidMount>***************************************************************************************/

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
  /**</componentWillUnmount>************************************************************************************/

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
        var _updateConnection = this.getData("updateConnection", JSON.parse(user));
        ws.send(_updateConnection);
      }
    };

    ws.onmessage = e => {
      //console.log("On new message...");
      const { user } = this.state;
      let row = JSON.parse(e.data);
      //console.log("New Message - " + row.Traders);

      const { tradersList, logsList, tradersDictionary, logsDictionary } = this.state;
      if (row.__MESSAGE__ === "updateConnection") {
        var _Data = this.getData("getSnapshotTraders", JSON.parse(user));
        ws.send(_Data);

        var _LogData = this.getData("getSnapshotLog", JSON.parse(user));
        ws.send(_LogData);
      }
      else if (row.__MESSAGE__ === 'snapshotTraders') {
        //console.log('snapshotTraders:%o', row.Traders);
        if (this.mounted) {
          var tempTraders = JSON.parse(row.Traders);
          for (var i = 0; i < tempTraders.length; i++) {
            this.props.actions.newTraderRow(tempTraders[i]);
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
  /**</check>*************************************************************************************************/

  isEmpty(value){
    return (value == null || value.length === 0);
  }
  /**<getData>**************************************************************************************************/
  getData(messageType, payload) {
    //Copy the values from the payload object, if one was supplied
    var payloadCopy = {};
    if (payload !== undefined && payload !== null) {
      var keys = Object.keys(payload);
      for (var index in keys) {
        var key = keys[index];
        payloadCopy[key] = payload[key];
      }
    }
    payloadCopy['__MESSAGE__'] = messageType;
    return JSON.stringify(payloadCopy);
  }
  /**</getData>*************************************************************************************************/

  /**<copyData>*************************************************************************************************/
  copyData(tradersDictionary) {
    var tempTradersList = [];
    const { user } = this.state;
    const tag_array = JSON.parse(user).Tags.split(',');

    for (var key in tradersDictionary) {
      let _volume, volume_cancelled, _fill_ratio;
      if (tradersDictionary[key]) {
        _volume = tradersDictionary[key].volume ? tradersDictionary[key].volume : 0;
        volume_cancelled = tradersDictionary[key].volume_cancelled ? tradersDictionary[key].volume_cancelled : 0;
        _fill_ratio = (parseFloat(_volume) / (parseFloat(_volume) + parseFloat(volume_cancelled))).toFixed(2);
        tradersDictionary[key].fill_ratio = isNaN(_fill_ratio) ? 0 : _fill_ratio;
      }
      for (var index in tag_array) {
        var _tag = tag_array[index];
        if (_tag === '999999' && !this.isEmpty(tradersDictionary[key].TAG) && !this.isEmpty(tradersDictionary[key].trader_id) && !this.isEmpty(tradersDictionary[key].acct)) {
          tempTradersList.push(tradersDictionary[key]);
        } else if (_tag === tradersDictionary[key].TAG.toString() && !this.isEmpty(tradersDictionary[key].trader_id) && !this.isEmpty(tradersDictionary[key].acct)) {
          tempTradersList.push(tradersDictionary[key]);
        }
      }
    }
    return tempTradersList;
  }
  /**</copyData>************************************************************************************************/

  /**<updateTradersFields>************************************************************************************************/
  updateTradersFields(tradersDictionary, traderRow, key) {
    let _tempExistingTraderRow = tradersDictionary[key];
    //console.log("_tempExistingTraderRow:%o", _tempExistingTraderRow);
    if (_tempExistingTraderRow != null) {
      _tempExistingTraderRow.trader_id = traderRow.trader_id;
      _tempExistingTraderRow.acct = traderRow.acct;
      _tempExistingTraderRow.TAG = traderRow.TAG;
      _tempExistingTraderRow.target = this.isEmpty(traderRow.target) ? traderRow.target : _tempExistingTraderRow.target;
      _tempExistingTraderRow.currency = this.isEmpty(traderRow.currency) ? traderRow.currency : _tempExistingTraderRow.currency;
      _tempExistingTraderRow.pnl = traderRow.pnl ? traderRow.pnl : _tempExistingTraderRow.pnl;
      _tempExistingTraderRow.position = this.isEmpty(traderRow.position) ? traderRow.position : _tempExistingTraderRow.position;
      _tempExistingTraderRow.volume = this.isEmpty(traderRow.volume) ? traderRow.volume : _tempExistingTraderRow.volume;
      _tempExistingTraderRow.notional = this.isEmpty(traderRow.notional) ? traderRow.notional : _tempExistingTraderRow.notional;
      _tempExistingTraderRow.last_updated = this.isEmpty(traderRow.last_updated) ? traderRow.last_updated : _tempExistingTraderRow.last_updated;
      _tempExistingTraderRow.last_traded = this.isEmpty(traderRow.last_traded) ? traderRow.last_traded : _tempExistingTraderRow.last_traded;
      _tempExistingTraderRow.new_count = this.isEmpty(traderRow.new_count) ? traderRow.new_count : _tempExistingTraderRow.new_count;
      _tempExistingTraderRow.cancel_count = this.isEmpty(traderRow.cancel_count) ? traderRow.cancel_count : _tempExistingTraderRow.cancel_count;
      _tempExistingTraderRow.reject_count = this.isEmpty(traderRow.reject_count) ? traderRow.reject_count : _tempExistingTraderRow.reject_count;
      _tempExistingTraderRow.status = this.isEmpty(traderRow.status) ? traderRow.status : _tempExistingTraderRow.status;
      _tempExistingTraderRow.free_text = this.isEmpty(traderRow.free_text) ? traderRow.free_text : _tempExistingTraderRow.free_text;
    } else {
      _tempExistingTraderRow = traderRow;
    }
    tradersDictionary[key] = _tempExistingTraderRow;
    return this.copyData(tradersDictionary);
  }
  /**</updateTradersFields>************************************************************************************************/

  /**<updateTradersFields>************************************************************************************************/
  updateLogsFields(logsDictionary, logRow, key) {
    let _tempExistingLogRow = logsDictionary[key];
    if (_tempExistingLogRow != null) {
      _tempExistingLogRow.trader_id = logRow.trader_id;
      _tempExistingLogRow.acct = logRow.acct;
      _tempExistingLogRow.TAG = logRow.TAG;
      _tempExistingLogRow.ts = logRow.ts ? logRow.ts : _tempExistingLogRow.ts;
      _tempExistingLogRow.level = logRow.level ? logRow.level : _tempExistingLogRow.level;
      _tempExistingLogRow.txt = logRow.txt ? logRow.txt : _tempExistingLogRow.txt;
    } else {
      _tempExistingLogRow = logRow;
    }
    logsDictionary[key] = _tempExistingLogRow;
    return this.copyData(logsDictionary);
  }
  /**</updateTradersFields>************************************************************************************************/

  /**<render>***************************************************************************************************/
  render() {
    return this.getPage();
  }
  /**<render>***************************************************************************************************/

  /**<onCellEditing>***************************************************************************************************/
  onCellEditing = (params) => {
    if (this.mounted) {
      console.log('params.data:%o', params.data);
      const { ws } = this.state;
      var updateData = this.getData("update_trader_status", params.data);
      ws.send(updateData);
    }
  }
  /**</onCellEditing>***************************************************************************************************/
  invokeErgonomy(_errorPanicList) {
    Session.setItem("ergonomy", JSON.stringify(_errorPanicList));
    this.getPage();
  }
  /**<getPage>**************************************************************************************************/
  getPage() {
    return (
      <div id='traders'>
        <div style={{ display: this.state.display }} className="alert alert-danger alert-dismissible fade show" role="alert">
          <span>Cannot connect to server.</span>
          <span className="close" data-dismiss="alert" aria-label="Close" style={{ cursor: "pointer" }} onClick={this.handleClose}>
            <span aria-hidden="true">&times;</span>
          </span>
        </div>
        <div className="row">
          <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6">
            <div className="title" >Traders View</div>
          </div>

          <div className="col-xs-6 col-sm-6 col-md-6 col-lg-6">
            <div style={{ float: "right", paddingRight: "15px", cursor: "pointer" }} onClick={this.refresh} title="Refresh"><FontAwesomeIcon icon={faSync} /></div>
            <div style={{ float: "right", paddingRight: "15px", cursor: "pointer" }} onClick={this.refreshRequest} title="Refresh Request"><FontAwesomeIcon icon={faRetweet} /></div>
          </div>
        </div>
        <div className="content-container">
          <div style={{ resize: "vertical", overflow: "auto", height: "38vh", width: '100%', padding: "5px 0px 8px 0px", position: "relative", borderTop: "solid 1px white" }} className="ag-theme-balham-dark">
            <AgGridReact
              ref="agGrid"
              modules={this.state.modules}
              columnDefs={this.state.columnDefs}
              defaultColDef={this.state.defaultColDef}
              onCellEditingStopped={this.onCellEditing}
              rowData={this.props.traderRows}
              //onGridReady={params => params.api.sizeColumnsToFit()}
              deltaRowDataMode={true}
              getRowNodeId={data => data.__row_id__}
              frameworkComponents={this.state.frameworkComponents}
              >
            </AgGridReact>
          </div>
          <div className="title" style={{ borderBottom: "solid 1px white" }}>Logs View</div>
          <div style={{ resize: "vertical", overflow: "auto", height: "36vh", width: '100%', padding: "5px 0px 8px 0px" }} className="ag-theme-balham-dark">
            <AgGridReact
              ref="agLogGrid"
              columnDefs={this.state.columnDefsLog}
              defaultColDef={this.state.defaultColDef}
              frameworkComponents={this.state.frameworkComponentsLog}
              rowData={this.state.rowDataLog}>
            </AgGridReact>
          </div>
        </div>
   
        <button id="trader-dummy" type="button" style={{ display: "none" }} onClick={this.callTogglePopup}>Go!</button>
      </div>
    );
  }
  /**</getPage>*************************************************************************************************/
}
/**</class AgGridTraders>***************************************************************************************/

AgGridTraders.contextTypes = {
  store: PropTypes.object                         // must be supplied when using redux with AgGridReact
};

const mapStateToProps = (state) => ({traderRows: state.traderRows});
const mapDispatchToProps = (dispatch) => ({actions: bindActionCreators(actions, dispatch)});

export default connect(mapStateToProps, mapDispatchToProps)(AgGridTraders);
