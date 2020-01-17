import React from 'react';
import { shallow } from 'enzyme';
import Logs from './logs';

describe('<Logs />', () => {
  test('renders', () => {
    const wrapper = shallow(<Logs />);
    expect(wrapper).toMatchSnapshot();
  });
});
