import React from 'react';
import { shallow } from 'enzyme';
import NavRoutes from './nav_routes';

describe('<NavRoutes />', () => {
  test('renders', () => {
    const wrapper = shallow(<NavRoutes />);
    expect(wrapper).toMatchSnapshot();
  });
});
