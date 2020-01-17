import React, { Component } from 'react';
import config from './config';
import history from './history/history';
import NavRoutes from './component/nav_routes/nav_routes';
import './App.css';
import './semantic-ui-css/semantic.min.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import { Session } from './helper/session';


class App extends Component {
  constructor(props) {
    super(props);
    this.mounted = false;
    this.state = {
      navbarToggler: 'navbar-toggler',
      ariaExpanded: 'false',
      navbarCollapse: 'navbar-collapse collapse',
      user: Session.getItem(config.token)
    }
    this.handleNavBarToggler = this.handleNavBarToggler.bind(this);
    this.updateState = this.updateState.bind(this);
    this.handleGoto = this.handleGoto.bind(this);
  }
  updateState() {
    if(this.mounted){
    const _user = JSON.parse(Session.getItem(config.token));
    this.setState({ user: _user });
    }
  }
  handleNavBarToggler() {
    if(this.mounted){
      this.setState({
        navbarToggler: this.state.navbarToggler === 'navbar-toggler' ? 'navbar-toggler collapsed' : 'navbar-toggler',
        ariaExpanded: this.state.ariaExpanded === 'false' ? 'true' : 'false',
        navbarCollapse: this.state.navbarCollapse === 'navbar-collapse collapse' ? 'navbar-collapse collapse show' : 'navbar-collapse collapse'
      });
    }
  }
  handleGoto(page) {
    history.replace(page);
  }
  componentDidMount() {
    this.mounted = true;
  }

  render() {
    const { user } = this.state;
    return (
      <div className="trade-container">
        <div>
          <nav className="navbar navbar-expand-lg navbar-dark nav-dark">
            <span className="navbar-brand">CTrader</span>
            <button className={this.state.navbarToggler} type="button" data-toggle="collapse"
              data-target="#navbarColor01" aria-controls="navbarColor01"
              aria-expanded={this.state.ariaExpanded} aria-label="Toggle navigation" onClick={this.handleNavBarToggler}>
              <span className="navbar-toggler-icon"></span>
            </button>
            <div className={this.state.navbarCollapse} id="navbarColor01">
              <ul className="navbar-nav mr-auto">
                <li className="nav-item active">
                  <span className="nav-link" onClick={() => this.handleGoto('/home')}>Home <span className="sr-only">(current)</span></span>
                </li>
                <li className="nav-item active">
                  <span className="nav-link" onClick={() => this.handleGoto('/trade_summary')}>Trades</span>
                </li>
                <li className="nav-item active">
                  <span className="nav-link" onClick={() => this.handleGoto('/contact')}>Contact</span>
                </li>
                <li className="nav-item active">
                  <span className="nav-link" onClick={() => this.handleGoto('/about')}>About</span>
                </li>
              </ul>
              <form className="form-inline">
                <button type="button" id="userDisplay" onClick={this.updateState} style={{ display: 'none' }}>Dummy</button>
                <span style={{ paddingRight: "5px" }}>{user ? user.username : 'Anonymous'}</span>
                <span className="nav-link login" onClick={() => this.handleGoto('/login')}>{user ? 'Sign Out' : 'Sign In'}</span>
              </form>
            </div>
          </nav>
        </div>
        <div className="Content">
          <div className="subContent">
            <NavRoutes></NavRoutes>
          </div>
        </div> 
          <div style={{ textAlign: 'center', padding: "10px", color: '#fff', height: '10px' }}>© 2020 CTrader</div>
      </div>
    );
  }
}

export default App;
