import React from 'react';
import AbstractOrder from './AbstractOrder';

class BidOrder extends AbstractOrder {

  render() {
    return (
      <tr className="bid">
        <td>{this.props.price}</td>
        <td></td>
      </tr>
    );
  }
}

export default BidOrder;
