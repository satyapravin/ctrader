import React from 'react';
import { shallow } from 'enzyme';
import History from './History';

describe('<History />', () => {
  test('renders', () => {
    const wrapper = shallow(<History />);
    expect(wrapper).toMatchSnapshot();
  });
});
