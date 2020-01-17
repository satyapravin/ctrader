import React from 'react';
import { shallow } from 'enzyme';
import OrderDisplay from './order-display';

describe('<OrderDisplay />', () => {
  test('renders', () => {
    const wrapper = shallow(<OrderDisplay />);
    expect(wrapper).toMatchSnapshot();
  });
});
