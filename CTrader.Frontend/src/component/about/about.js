import React, { Component } from 'react';

class About extends Component {

  constructor() {
    super();
    this._handleClick = this._handleClick.bind(this);
  }
  _handleClick() {
    this.props.history.push("/contact");
  }

  render() {
    return this.getTemplate();
  }

  getTemplate() {
    return (
      <div className='about'>
          <div className="title">About Us! </div>
          <div className="content-container">
            <h4>CTrader</h4>
          </div>
      </div>
    );
  }
}

export default About;
