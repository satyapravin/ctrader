import React from 'react';
import { shallow } from 'enzyme';
import StatusUpdate from './status-update';

describe('<StatusUpdate />', () => {
  test('renders', () => {
    const wrapper = shallow(<StatusUpdate />);
    expect(wrapper).toMatchSnapshot();
  });
});
