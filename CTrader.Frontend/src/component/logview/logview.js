import React, { Component } from 'react';
import config from './../../config';

import { Session } from './../../helper/session';
import { connect} from "react-redux";
import { bindActionCreators } from 'redux';
import { actions } from '../../reducers/actions'
import * as PropTypes from "prop-types"; 

import { AllModules } from "@ag-grid-enterprise/all-modules";
import { AgGridReact } from 'ag-grid-react';
import 'ag-grid-community/dist/styles/ag-grid.css';
import 'ag-grid-community/dist/styles/ag-theme-balham.css';
import 'ag-grid-community/dist/styles/ag-theme-balham-dark.css';

class LogView extends Component {

  constructor(props) {
    super(props);
    this.state = {
       user: JSON.parse(Session.getItem(config.token))
      ,defaultColDefLog: { sortable: true, filter: true, resizable: true }
      ,columnDefsLog: [
        { headerName: "Message", field: "message", width: 200}
        ,{ headerName: "Type", field: "type", width: 50} 
        ,{ headerName: "Timestamp", field: "timestamp", width: 160}
        ,{ headerName: "Source", field: "source", width: 160}
      ]
      ,logsList: []
      ,modules: AllModules
    };
  }

  render() {
    return this.showPage();
  }

  showPage() {
    return (
      <AgGridReact
        ref="agGrid"
        modules={this.state.modules}
        columnDefs={this.state.columnDefsLog}
        defaultColDef={this.state.defaultColDefLog}
        rowData={this.props.data.logRows}
        onGridReady={params => params.api.sizeColumnsToFit()}
        deltaRowDataMode={true}
        getRowNodeId={data => data.__row_id__} 
        />
    );
  }
}

LogView.contextTypes = {
  store: PropTypes.object                         // must be supplied when using redux with AgGridReact
};

const mapStateToProps = (state) => ({data: state.data});
const mapDispatchToProps = (dispatch) => ({actions: bindActionCreators(actions, dispatch)});
export default connect(mapStateToProps, mapDispatchToProps)(LogView);