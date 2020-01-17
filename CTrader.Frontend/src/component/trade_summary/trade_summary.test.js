import React from 'react';
import { shallow } from 'enzyme';
import TradeSummary from './trade_summary';

describe('<trade_summary />', () => {
  test('renders', () => {
    const wrapper = shallow(<TradeSummary />);
    expect(wrapper).toMatchSnapshot();
  });
});
