import React from 'react';
import { shallow } from 'enzyme';
import ErrorFormatter from './error-formatter';

describe('<ErrorFormatter />', () => {
  test('renders', () => {
    const wrapper = shallow(<ErrorFormatter />);
    expect(wrapper).toMatchSnapshot();
  });
});
