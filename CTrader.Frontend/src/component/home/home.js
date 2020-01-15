import React, { Component } from 'react';

class Home extends Component {
  render() {
    return this.getTemplate();
  }

  getTemplate() {
    return (
      <div className='home'>
        <div className="title">Home!</div>
        <div className="content-container">
          <div id="home-accept-data">
            <div className="row">
              <div className="col-xs-12 col-sm-12 col-md-12 col-lg-12">
                <div className="form-group">
                    <h4>CTrader</h4>
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
export default Home;