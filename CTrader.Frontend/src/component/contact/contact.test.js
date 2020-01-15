import React from 'react';
import { shallow } from 'enzyme';
import Contact from './contact';

describe('<Contact />', () => {
  test('renders', () => {
    const wrapper = shallow(<Contact />);
    expect(wrapper).toMatchSnapshot();
  });
});
