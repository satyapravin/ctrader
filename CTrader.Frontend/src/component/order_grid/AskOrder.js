import React from 'react';
import AbstractOrder from './AbstractOrder';

class AskOrder extends AbstractOrder {

  render() {
    return (
      <tr className="ask">
        <td></td>
        <td>{this.props.price}</td>
      </tr>
    );
  }
}

export default AskOrder;
