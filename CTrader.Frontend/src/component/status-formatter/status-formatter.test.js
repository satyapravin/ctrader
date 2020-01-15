import React from 'react';
import { shallow } from 'enzyme';
import StatusFormatter from './status-formatter';

describe('<StatusFormatter />', () => {
  test('renders', () => {
    const wrapper = shallow(<StatusFormatter />);
    expect(wrapper).toMatchSnapshot();
  });
});
