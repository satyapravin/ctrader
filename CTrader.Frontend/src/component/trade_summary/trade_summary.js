import React, { Component } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSync } from "@fortawesome/free-solid-svg-icons";
import { faStopCircle } from '@fortawesome/free-solid-svg-icons';
import { faPlayCircle } from '@fortawesome/free-solid-svg-icons';
import { faRedo } from '@fortawesome/free-solid-svg-icons';
import { consoleService } from '../../bal/console.bal';
import OrderGrid from '../order_grid/OrderGrid';

class TradeSummary extends Component {

  constructor(props) {
    super(props);

    this.state = {
      total: 0
    };
    this.start = this.start.bind(this);
    this.stop = this.stop.bind(this);
    this.rebalance = this.rebalance.bind(this);
  }

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
          <div id="home-display-data"></div>
        </div>
      </div>
    );
  }
}
export default TradeSummary;