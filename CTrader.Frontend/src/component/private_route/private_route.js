// This is used to determine if a user is authenticated and
// if they are allowed to visit the page they navigated to.

// If they are: they proceed to the page
// If not: they are redirected to the login page.
import React from 'react'
import { Redirect, Route } from 'react-router-dom'
import config from './../../config';
import { Session } from './../../helper/session';

const PrivateRoute = ({ component: Component, roles, ...rest }) => {

  return (
    <Route {...rest} render={props => {
      // Add your own authentication on the below line.
      const loggedUser = JSON.parse(Session.getItem(config.token));
      if (!loggedUser) {
        // not logged in so redirect to login page with the return url
        return <Redirect to={{ pathname: '/login', state: { from: props.location } }} />
      }

      // check if route is restricted by role
      if (roles && roles.indexOf("Admin") === -1) {
        // role not authorised so redirect to home page
        return <Redirect to={{ pathname: '/' }} />
      }
      else if(roles && roles.indexOf("Admin") === 0 && loggedUser.isAdmin === false){
        // role not authorised so redirect to home page
        return <Redirect to={{ pathname: '/dashboard' }} />
      }

      // authorised so return component
      return <Component {...props} />
    }} />
  )
}

export default PrivateRoute