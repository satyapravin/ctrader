import React from 'react';
import { shallow } from 'enzyme';
import LogView from './logview';

describe('<LogView />', () => {
  test('renders', () => {
    const wrapper = shallow(<LogView />);
    expect(wrapper).toMatchSnapshot();
  });
});
