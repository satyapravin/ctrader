import React, { Component } from 'react';

class Contact extends Component {
  render() {
    return this.getTemplate();
  }

  getTemplate() {
    return (
      <div className='contact'>
        <div className="title">Contact Us!</div>
        <div className="content-container">
          <h4>CTrader</h4>
    );
  }
}
export default Contact;
