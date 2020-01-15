import React from 'react';
import { shallow } from 'enzyme';
import DateFormatter from './date-formatter';

describe('<DateFormatter />', () => {
  test('renders', () => {
    const wrapper = shallow(<DateFormatter />);
    expect(wrapper).toMatchSnapshot();
  });
});
