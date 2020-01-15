import React, { Component } from 'react';

const URL = 'ws://localhost:3030';
class OrderDisplay extends Component {
  constructor(props) {
    super(props);
    this.state = {
      order: {
        instrumentName: '',
        accountName: '',
        quantity: '',
        price: ''
      },
      orders: [],
    };

    this.ws = new WebSocket(URL);
  }

  componentDidMount() {
    this.ws.onopen = () => {
      // on connecting, do nothing but log it to the console
      console.log('connected');
    }

    this.ws.onmessage = evt => {
      // on receiving a message, add it to the list of messages
      const order = JSON.parse(evt.data);
      console.log(evt);
      this.addOrders(order[0]);
    }

    this.ws.onclose = () => {
      console.log('disconnected')
      // automatically try to reconnect on connection loss
      this.setState({
        ws: new WebSocket(URL),
      })
    }
  };

  addOrders(order){
    this.setState(state => ({ orders: [order, ...state.orders] }));
    console.log(order);
  }
  render() {
    return this.showPage();
  }

  showPage() {
    let orderDetails = this.state.orders.map((order, index)=>{
      return (
      <ul key={index}>
        <li>{order.instrumentName}</li>
        <li>{order.accountName}</li>
        <li>{order.quantity}</li>
        <li>{order.price}</li>
      </ul>);
    });
    return (
      <div>
        {orderDetails}
      </div>
    );
  }
}

export default OrderDisplay;
