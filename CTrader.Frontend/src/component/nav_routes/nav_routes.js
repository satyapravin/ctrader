import React, { Component } from 'react';
import { Switch, Route,Router } from 'react-router-dom';
import history from '../../history/history';
import PrivateRoute from './../private_route/private_route';

// Our Components
import Dashboard from './../dashboard/dashboard';
import Home from '../home/home';
import About from '../about/about';
import Contact from '../contact/contact';
import Login from './../login/login'; 
import TradeSummary from '../trade_summary';

class NavRoutes extends Component {
  render() {
    return (
      <main>
        <Router history={history}>
        <Switch>
            (// Publicly visible components})
            <Route exact path='/' component={Home}/>
            <Route path="/login" component={Login}/>
            <Route path='/about' component={About}/>
            <Route path='/contact' component={Contact}/>
            <Route exact path="/trade_summary" component={TradeSummary}/>
            (// Privately visible components, i.e. one need to login to see these components)
            <PrivateRoute exact path='/dashboard' component={Dashboard}/>
            <Route component={Home} />
          </Switch>
          </Router>
        </main>
    );
  }
}
export default NavRoutes;