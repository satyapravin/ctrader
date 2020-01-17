import React from 'react';
import { shallow } from 'enzyme';
import LogFormatter from './log-formatter';

describe('<LogFormatter />', () => {
  test('renders', () => {
    const wrapper = shallow(<LogFormatter />);
    expect(wrapper).toMatchSnapshot();
  });
});
