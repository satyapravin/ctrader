import React, { Component } from 'react';
// import history from '../../history/history';
import config from './../../config';
import './login.css';
import { Session } from './../../helper/session';
import { userBALService } from '../../bal/user.bal';
import { message} from 'antd'
class Login extends Component {
  constructor(props) {
    super(props);
    this.timeout = 250;
    this.mounted = false;
    this.state = {
      username: '',
      password: '', 
      token: config.token,
      showHide: 'alert alert-danger alert-dismissible fade',
      error_username: null,
      error_password: null,
      Data: {
        __MESSAGE__: 'login',
        key: 'USER_AUTHENTICATION',
        username: '',
        password: '',
        GUID:''
        //UserCredential: {},
      },
      userInfo: null,
      display: "none",
      ws: null,
    };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleClose = this.handleClose.bind(this);
  }


  componentWillUnmount() {
    
  }
  connectServer = () => {
  };

  check = () => {
  };

  handleClose() {
    var _showHide = this.state.showHide === 'alert alert-danger alert-dismissible fade show' ? 'alert alert-danger alert-dismissible fade' : 'alert alert-danger alert-dismissible fade show';
    if (this.mounted) {
      this.setState({ showHide: _showHide });
    }
  }


  handleSubmit(e) {
    e.preventDefault();
    const { username, password } = this.state;
    if (username && password) {
      this.login(username, password);
    }
  }
  login(_username, _password) {
    userBALService.login(_username, _password).then(_user => {
      this.resetState();
      localStorage.setItem('loginStatus', 'login');
      this.setState({ userInfo: _user });
      Session.setItem(config.token, JSON.stringify(_user));
      document.getElementById('userDisplay').click();
      this.props.history.push('/dashboard');
    },
      error => {
        this.error(error.toString());
        console.log(error.toString());
        this.handleClose();
      }
    );
  }
  error(_error){
    message.error(_error);
  };

  logout() {
    userBALService.logout();
    document.getElementById('userDisplay').click();
   
  }

  
  componentDidMount() {
    this.mounted = true;
    try {
      if (Session.getItem(config.token) && JSON.parse(Session.getItem(config.token))) {
        Session.removeItem(config.token);
        document.getElementById('userDisplay').click();
      }
    } catch (error) {
      Session.removeItem(config.token);
      document.getElementById('userDisplay').click();
    }
    this.connectServer();
  }



  handleChange(e) {
    const { name, value } = e.target;
     
      this.setState({
        [name]: value,
        ['error_' + name]: value
      });
 
  }
  resetState() {
    if (this.mounted) {
      this.setState({
        username: '',
        password: '',
        error_username: null,
        error_password: null
      });
    }
  }

  render() {
    return this.showPage();
  }
  showPage() {
    const { username, password, error_username, error_password } = this.state;
    return (
      <div>
        <div className="row">
          <div className="col-xs-12 col-sm-12 col-md-4 col-lg-4">
            <div className='login-left'>
              <form name="form">
                <div className={this.state.showHide} role="alert">
                  Incorrect email or password!
                  <span className="close" data-dismiss="alert" aria-label="Close" style={{ cursor: "pointer" }} onClick={this.handleClose}>
                    <span aria-hidden="true">&times;</span>
                  </span>
                </div>

                <span className='login-lable'>LOGIN</span>
                <div className="spacer">
                  <input type="text"
                    placeholder="Email"
                    name="username"
                    value={username}
                    onChange={this.handleChange}
                    autoComplete="off"
                    className={'form-control ' + (error_username ? 'is-valid' : 'is-invalid')}
                  />
                </div>
                <div className="spacer">
                  <input type="password"
                    placeholder="Password"
                    name="password"
                    value={password}
                    onChange={this.handleChange}
                    className={'form-control ' + (error_password ? 'is-valid' : 'is-invalid')}
                  />
                </div>
                <div className="spacer">
                  <input className='login-button' type="button" value="Login" onClick={this.handleSubmit} />
                </div>
              </form>
            </div>
          </div>
          <div className="col-xs-12 col-sm-12 col-md-8 col-lg-8">
            <div className='login-right'>
              CTrader
          </div>
          </div>
        </div>

      </div>
    );
  }
}

export default Login;
