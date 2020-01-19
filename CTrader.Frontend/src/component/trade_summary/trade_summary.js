import config from './../../config';
import React, { Component } from 'react';
import { AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import 'ag-grid-community/dist/styles/ag-theme-balham-dark.css';

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
      defaultColDefPosition: { sortable: true, filter: true, resizable: true },
      columnDefsPosition: [
        { headerName: "Sym", field: "symbol", width: 75},
        { headerName: "Acc", field: "account", width: 75},
        { headerName: "Size", field: "currentQty", width: 75},
        { headerName: "Value", field: "homeNotional", width: 100},
        { headerName: "Entry Price", field: "avgEntryPrice", width: 100},
        { headerName: "Mark Price", field: "markPrice", width: 100},
        { headerName: "Liq. Price", field: "liquidationPrice", width: 100},
        { headerName: "Margin", field: "marginCallPrice", width: 100},
        { headerName: "UnRealised Pnl", field: "unrealisedPnl", width: 100},
        { headerName: "ROE%", field: "unrealisedRoePcnt", width: 100},
        { headerName: "RealisedPnl", field: "realisedPnl", width: 100}
      ],
      defaultColDefOrder: { sortable: true, filter: true, resizable: true },
      columnDefsOrder: [
        { headerName: "Status", field: "ordStatus", width: 80},
        { headerName: "Acc", field: "account", width: 75},
        { headerName: "Side", field: "side", width: 75},
        { headerName: "Sym", field: "symbol", width: 75},
        { headerName: "Qty", field: "orderQty", width: 65},
        { headerName: "OrdType", field: "ordType", width: 90},
        { headerName: "Price", field: "price", width: 90},
        { headerName: "Curr Settlement", field: "settlCurrency", width: 100},
        { headerName: "TimeInForce", field: "timeInForce", width: 150},
        { headerName: "Timestamp", field: "timestamp", width: 170},
        { headerName: "TransTime", field: "transactTime", width: 170},
        { headerName: "Working Ind", field: "workingIndicator", width: 100}
      ],
      rowData: [],
      total: 0,
      ws: null,
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
        const authMessage = JSON.stringify(_updateConnection);
        ws.send(authMessage);
      }
    };

    ws.onmessage = e => {
      console.log(e) ;
      const { user } = this.state;
      let row = JSON.parse(e.data);
      //console.log("New Message - " + row.Traders);

      const { tradersList, logsList, tradersDictionary, logsDictionary } = this.state;
      if (row && row.table && row.table === 'order') {
        //console.log('snapshotTraders:%o', row.Traders);
        if (this.mounted) {
          for (var index = 0; index < row.data.length; index++) {
            this.props.actions.updateOrderRow(row.data[index]);
          }
        }
      } else if (row && row.table && row.table === 'position') {
        //console.log('snapshotTraders:%o', row.Traders);
        if (this.mounted) {
          for (var index = 0; index < row.data.length; index++) {
            this.props.actions.updatePositionRow(row.data[index]);
          }
        }
      } else if (row && row.success === true && row.request && row.request.op === "authKeyExpires") {
        console.log("Authentication successful!")
        var _Data = {"op" : "subscribe", "args":["position", "order"]};
        ws.send(JSON.stringify(_Data));
      } else if (row && row.success === true && row.request && row.request.op === "subscribe") {
        console.log("Subscription successful for " + row.request.args);
        //ws.send(_Data);
      } 
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

  
  isEmpty(value) {
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
        <div className="content-container">
          <div id="home-accept-data">
            <div className="row">
              <div className="col-xs-12 col-sm-12 col-md-12 col-lg-12">
                <div className="form-group">
                    <table>
                      <thead>
                        <tr>
                        <th></th>
                        <th></th>
                        <th></th>
                        <th></th>
                        <th style={{"white-space": "nowrap"}}></th>
                        </tr>
                      </thead>
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
                          <td colSpan="4">
                            <br/>
                            <div title="Rebalance" style={{float:"right", paddingRight:"15px", cursor:"pointer"}} onClick={this.rebalance}><FontAwesomeIcon icon={faRedo} /></div>
                            <div title="Stop" style={{float:"right", paddingRight:"15px", cursor:"pointer"}} onClick={this.stop}><FontAwesomeIcon icon={faStopCircle} /></div>
                            <div title="Start" style={{float:"right", paddingRight:"15px", cursor:"pointer"}} onClick={this.start}><FontAwesomeIcon icon={faPlayCircle} /></div>
                          </td>
                        </tr>
                        </tbody>
                        </table> 
                            <div style={{ resize: "vertical", overflow: "auto", height: "18vh", width: 'auto', padding: "5px 0px 8px 0px", position: "relative", borderTop: "solid 1px white" }} className="ag-theme-balham-dark">
                            <AgGridReact
                              ref="agGrid"
                              modules={this.state.modules}
                              columnDefs={this.state.columnDefsPosition}
                              defaultColDef={this.state.defaultColDefPosition}
                              rowData={this.props.positionRows}
                              //onGridReady={params => params.api.sizeColumnsToFit()}
                              deltaRowDataMode={true}
                              getRowNodeId={data => data.__row_id__} 
                              />
                            </div>
                          
                            <div style={{ resize: "vertical", overflow: "auto", height: "18vh", width: 'auto', padding: "5px 0px 8px 0px", position: "relative", borderTop: "solid 1px white" }} className="ag-theme-balham-dark">
                            <AgGridReact
                              ref="agGrid"
                              modules={this.state.modules}
                              columnDefs={this.state.columnDefsOrder}
                              defaultColDef={this.state.defaultColDefOrder}
                              rowData={this.props.orderRows}
                              //onGridReady={params => params.api.sizeColumnsToFit()}
                              deltaRowDataMode={true}
                              getRowNodeId={data => data.__row_id__} 
                              />
                            </div>
                </div>
              </div>
            </div>
          </div>
          <div id="home-display-data"> </div>
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

const mapStateToProps = (state) => ({positionRows: state.positionRows, orderRows: state.orderRows});
const mapDispatchToProps = (dispatch) => ({actions: bindActionCreators(actions, dispatch)});
export default connect(mapStateToProps, mapDispatchToProps)(TradeSummary);

/* Order
------------------------------------------------
avgPx:null
clOrdID:""
clOrdLinkID:""
contingencyType:""
cumQty:0
currency:"USD"
displayQty:null
exDestination:"XBME"
execInst:""
leavesQty:1
multiLegReportingType:"SingleSecurity"
pegOffsetValue:null
pegPriceType:""
simpleCumQty:null
simpleLeavesQty:null
simpleOrderQty:null
stopPx:null
account:271696
orderID:"4d4f01f8-a7e6-265e-f7d3-7c898906f858"
orderQty:1
ordRejReason:"" 
ordStatus:"New"
ordType:"Limit"
price:8867.5
settlCurrency:"XBt"
side:"Buy"
symbol:"XBTUSD"
text:"Submission from testnet.bitmex.com"
timeInForce:"GoodTillCancel"
timestamp:"2020-01-18T03:29:28.896Z"
transactTime:"2020-01-18T03:29:28.896Z"
triggered:""
workingIndicator:true
------------------------------------------------
Position
------------------------------------------------
account:271696
avgCostPrice:8950
avgEntryPrice:8950
bankruptPrice:9440
breakEvenPrice:8918
commission:0.00075
crossMargin:true
currency:"XBt"
currentComm:20877
currentCost:7508152
currentQty:-672
currentTimestamp:"2020-01-18T08:29:00.326Z"
deleveragePercentile:1
execBuyCost:0
execBuyQty:0
execComm:0
execCost:0
execQty:0
execSellCost:0
execSellQty:0
foreignNotional:672
grossExecCost:0
grossOpenCost:0
grossOpenPremium:0
homeNotional:-0.07600992
indicativeTax:0
indicativeTaxRate:null
initMargin:0
initMarginReq:0.01
isOpen:true
lastPrice:8840.76
lastValue:7600992
leverage:100
liquidationPrice:9381.5
longBankrupt:0
maintMargin:173523
maintMarginReq:0.005
marginCallPrice:9381.5
markPrice:8840.76
markValue:7600992
openingComm:20877
openingCost:7508152
openingQty:-672
openingTimestamp:"2020-01-18T08:00:00.000Z"
openOrderBuyCost:-23634
openOrderBuyPremium:0
openOrderBuyQty:2
openOrderSellCost:0
openOrderSellPremium:0
openOrderSellQty:0
posAllowance:0
posComm:5704
posCost:7508256
posCost2:7529104
posCross:20848
posInit:75083
posLoss:20848
posMaint:50198
posMargin:80787
posState:""
prevClosePrice:8835.93
prevRealisedPnl:0
prevUnrealisedPnl:0
quoteCurrency:"USD"
realisedCost:-104
realisedGrossPnl:104
realisedPnl:-20773
realisedTax:0
rebalancedPnl:-5639
riskLimit:20000000000
riskValue:7600992
sessionMargin:0
shortBankrupt:0
simpleCost:null
simplePnl:null
simplePnlPcnt:null
simpleQty:null
simpleValue:null
symbol:"XBTUSD"
targetExcessMargin:0
taxableMargin:0
taxBase:0
timestamp:"2020-01-18T08:29:00.326Z"
underlying:"XBT"
unrealisedCost:7508256
unrealisedGrossPnl:92736
unrealisedPnl:92736
unrealisedPnlPcnt:0.0124
unrealisedRoePcnt:1.2351
unrealisedTax:0
varMargin:0
*/