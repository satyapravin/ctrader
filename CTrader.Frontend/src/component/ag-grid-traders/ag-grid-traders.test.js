import React from 'react';
import { shallow } from 'enzyme';
import AgGridExample from './ag-grid-example';

describe('<AgGridExample />', () => {
  test('renders', () => {
    const wrapper = shallow(<AgGridExample />);
    expect(wrapper).toMatchSnapshot();
  });
});
