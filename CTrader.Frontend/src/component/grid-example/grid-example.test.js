import React from 'react';
import { shallow } from 'enzyme';
import GridExample from './grid-example';

describe('<GridExample />', () => {
  test('renders', () => {
    const wrapper = shallow(<GridExample />);
    expect(wrapper).toMatchSnapshot();
  });
});
